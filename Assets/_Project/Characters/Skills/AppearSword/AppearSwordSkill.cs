using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.MovementSystem;
using StudioScor.PlayerSystem;
using StudioScor.RotationSystem;
using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Appear Sword Skill", fileName = "GA_Skill_Appear_Sword")]
    public class AppearSwordSkill : GASAbility, ISkill
    {
        [Header(" [ Appear Sword ]")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private ESkillType _skillType = ESkillType.Appear;

        [Header(" Animation ")]
        [SerializeField] private string _appearAnimation = "AppearSword";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0f;

        [Header(" Movement ")]
        [SerializeField] private float _moveDistance = 2f;
        [SerializeField][Range(0f, 1f)] private float _moveStartTime = 0f;
        [SerializeField][Range(0f, 1f)] private float _moveEndTime = 0.6f;
        [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header(" Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField] private float _traceRadius = 2f;
        [SerializeField] private Variable_LayerMask _traceLayer;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private TakeRadialKnockbackEffect _takeRadialKnockbackEffect;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToSelf;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _onImpactCue;
        [SerializeField] private FGameplayCue _onHitToOtherCue;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit;
        public Sprite Icon => _icon;
        public ESkillType SkillType => _skillType;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, ISkillState, IUpdateableAbilitySpec
        {
            protected new readonly AppearSwordSkill _ability;

            private readonly AnimationPlayer _animationPlayer;
            private readonly IPawnSystem _pawnSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private readonly int _animationHash;
            private readonly AnimationPlayer.Events _animationEvents;
            private readonly ReachValueToTime _moveReachValue = new();
            private readonly TrailSphereCast _sphereCast = new();

            private Vector3 _moveDirection;

            private CoolTimeEffect.Spec _coolTimeSpec;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 0f : _coolTimeSpec.RemainTime;
            public float NormalizedCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 1f : _coolTimeSpec.NormalizedTime;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as AppearSwordSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _pawnSystem = gameObject.GetPawnSystem();
                _bodySystem = gameObject.GetBodySystem();
                _movementSystem = gameObject.GetMovementSystem();
                _rotationSystem = gameObject.GetRotationSystem();

                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _animationHash = Animator.StringToHash(_ability._appearAnimation);
                _animationEvents = new();

                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;
                _animationEvents.OnNotify += _animationEvents_OnNotify;
            }

            

            public override bool CanActiveAbility()
            {
                if (_ability._coolTimeEffect && _gameplayEffectSystem.HasEffect(_ability._coolTimeEffect))
                    return false;

                if (!base.CanActiveAbility())
                    return false;

                return true;
            }


            protected override void EnterAbility()
            {
                base.EnterAbility();

                _animationPlayer.Play(_animationHash, _ability._fadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                _moveDirection = transform.HorizontalDirection(_pawnSystem.LookPosition);

                _rotationSystem.SetRotation(Quaternion.LookRotation(_moveDirection, Vector3.up), false);
                _moveReachValue.OnMovement(_ability._moveDistance, _ability._moveCurve);

                if (_ability._coolTimeEffect)
                {
                    var effect = _gameplayEffectSystem.TryTakeEffect(_ability._coolTimeEffect, gameObject, Level, null);

                    if (effect.isActivate)
                    {
                        _coolTimeSpec = effect.effectSpec as CoolTimeEffect.Spec;
                        _coolTimeSpec.OnEndedEffect += _coolTimeSpec_OnEndedEffect;
                    }
                }
            }

            private void _coolTimeSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeSpec_OnEndedEffect;

                _coolTimeSpec = null;
            }

            private void _animationEvents_OnNotify(string eventName)
            {
                switch (eventName)
                {
                    case "Impact":
                        PlayImpactFX();
                        break;
                    default:
                        break;
                }
            }

            private void PlayImpactFX()
            {
                if(_ability._onImpactCue.Cue)
                {
                    _ability._onImpactCue.PlayFromTarget(transform);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _moveReachValue.EndMovement();
                _sphereCast.EndTrace();
            }
            public void UpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    if(_animationPlayer.IsPlaying)
                    {
                        float normalizedTime = _animationPlayer.NormalizedTime;

                        UpdateMovement(normalizedTime);
                    }
                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                UpdateTrace();
            }
            private void _animationEvents_OnStartedBlendOut()
            {
                TryFinishAbility();
            }

            private void _animationEvents_OnFailed()
            {
                CancelAbility();
            }

            private void _animationEvents_OnCanceled()
            {
                CancelAbility();
            }
            private void _animationEvents_OnEnterNotifyState(string eventName)
            {
                switch (eventName)
                {
                    case "Trace":
                        OnTrace();
                        break;

                    default:
                        break;
                }
            }
            private void _animationEvents_OnExitNotifyState(string eventName)
            {
                switch (eventName)
                {
                    case "Trace":
                        EndTrace();
                        break;

                    default:
                        break;
                }
            }

            private void UpdateMovement(float normalizedTime)
            {
                if (normalizedTime < _ability._moveStartTime || normalizedTime > _ability._moveEndTime)
                    return;

                float moveTime = Mathf.InverseLerp(_ability._moveStartTime, _ability._moveEndTime, normalizedTime);
                _moveReachValue.UpdateMovement(moveTime);

                _movementSystem.MovePosition(_moveReachValue.DeltaDistance * _moveDirection);
            }
            private void OnTrace()
            {
                var bodypart = _bodySystem.GetBodyPart(_ability._tracePoint);

                _sphereCast.SetOwner(bodypart.gameObject);

                _sphereCast.AddIgnoreTransform(transform);

                _sphereCast.TraceRadius = _ability._traceRadius;
                _sphereCast.TraceLayer = _ability._traceLayer.Value;
                _sphereCast.MaxHitCount = 20;
                _sphereCast.UseDebug = UseDebug;

                _sphereCast.OnTrace();
            }

            private void EndTrace()
            {
                _sphereCast.EndTrace();
            }
            private void UpdateTrace()
            {
                var trace = _sphereCast.UpdateTrace();

                if (trace.hitCount > 0)
                {
                    bool wasHit = false;

                    for (int i = 0; i < trace.hitCount; i++)
                    {
                        bool isHit = false;
                        var hit = trace.raycastHits[i];

                        Log($"HIT :: {hit.transform.name}");

                        if (hit.transform.TryGetPawnSystem(out IPawnSystem hitPawn) && hitPawn.Controller.CheckAffiliation(_pawnSystem.Controller) != EAffiliation.Hostile)
                            continue;

                        if (hit.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                        {
                            if (_ability._takeDamageEffect)
                            {
                                var data = new TakeDamageEffect.FElement(hit.point, hit.normal, hit.collider, _sphereCast.StartPosition.Direction(_sphereCast.EndPosition), _sphereCast.Owner, gameObject);

                                if (hitGameplayEffectSystem.TryTakeEffect(_ability._takeDamageEffect, gameObject, Level, data).isActivate)
                                {
                                    isHit = true;
                                }
                            }

                            if(_ability._takeRadialKnockbackEffect)
                            {
                                var data = new TakeRadialKnockbackEffect.FElement(_sphereCast.Owner.transform.position);

                                if (hitGameplayEffectSystem.TryTakeEffect(_ability._takeRadialKnockbackEffect, gameObject, Level, data).isActivate)
                                {
                                    isHit = true;
                                }
                            }

                            for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnHitToOther.Length; effectIndex++)
                            {
                                var effect = _ability._applyGameplayEffectsOnHitToOther[effectIndex];

                                if (hitGameplayEffectSystem.TryTakeEffect(effect, gameObject, Level, null).isActivate)
                                {
                                    isHit = true;
                                }
                            }
                        }

                        if (isHit)
                        {
                            wasHit = true;

                            if (_ability._onHitToOtherCue.Cue)
                            {
                                Vector3 position = hit.distance > 0 ? hit.point + hit.transform.TransformDirection(_ability._onHitToOtherCue.Position)
                                                                    : hit.collider.ClosestPoint(_sphereCast.StartPosition);
                                Quaternion hitRotation = hit.normal.SafeEquals(Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(hit.normal, Vector3.up);
                                Vector3 rotation = hitRotation.eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
                                Vector3 scale = _ability._onHitToOtherCue.Scale;

                                _ability._onHitToOtherCue.Cue.Play(position, rotation, scale);
                            }
                        }
                    }

                    if (wasHit)
                    {
                        if (_gameplayEffectSystem is not null)
                        {
                            for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnSuccessedHit.Length; effectIndex++)
                            {
                                var effect = _ability._applyGameplayEffectsOnSuccessedHit[effectIndex];

                                _gameplayEffectSystem.TryTakeEffect(effect, gameObject, Level, null);
                            }
                        }

                        if (_pawnSystem.IsPlayer)
                        {
                            if (_ability._onSuccessedPlayerHit.Cue)
                            {
                                _ability._onSuccessedPlayerHit.PlayFromTarget(transform);
                            }
                        }
                    }

                }
            }
        }
    }
}