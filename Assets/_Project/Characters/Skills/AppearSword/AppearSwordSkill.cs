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
    public class AppearSwordSkill : CharacterSkill
    {
        [Header(" [ Appear Sword ]")]
        [Header(" Animation ")]
        [SerializeField] private string _animationName = "AppearSword";
        [SerializeField][Range(0f, 1f)] private float _fadeTime = 0f;

        [Header(" Movement ")]
        [SerializeField] private float _moveDistance = 5f;
        [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header(" Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField] private float _traceRadius = 2f;
        [SerializeField] private SOLayerMaskVariable _traceLayer;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private TakeRadialKnockbackEffect _takeRadialKnockbackEffect;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToSelf;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _onAbilityCue;
        [SerializeField] private FGameplayCue _onImpactCue;
        [SerializeField] private FGameplayCue _onHitToOtherCue;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, ISkillState, IUpdateableAbilitySpec
        {
            protected new readonly AppearSwordSkill _ability;

            private readonly AnimationPlayer _animationPlayer;
            private readonly IPawnSystem _pawnSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private readonly TrailSphereCast _sphereCast = new();
            private readonly AnimationPlayer.Events _animationEvents = new();
            private readonly ReachValueToTime _movementValue = new();

            private readonly int _animationID;

            private bool _wasAnimationStarted;
            private CoolTimeEffect.Spec _coolTimeSpec;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 0f : _coolTimeSpec.RemainTime;
            public float NormalizedCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 1f : _coolTimeSpec.NormalizedTime;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as AppearSwordSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _pawnSystem = gameObject.GetPawnSystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _bodySystem = gameObject.GetBodySystem();
                _movementSystem = gameObject.GetMovementSystem();
                _rotationSystem = gameObject.GetRotationSystem();

                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _animationID = Animator.StringToHash(_ability._animationName);
                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;
                _animationEvents.OnNotify += _animationEvents_OnNotify;

                _ability._onAbilityCue.Initialization();
                _ability._onImpactCue.Initialization();
                _ability._onHitToOtherCue.Initialization();
                _ability._onSuccessedPlayerHit.Initialization();
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

                _animationPlayer.Play(_animationID, _ability._fadeTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                Vector3 direction = _pawnSystem.LookDirection;
                
                _rotationSystem.SetRotation(Quaternion.LookRotation(direction, transform.up));

                OnMovement();

                if (_ability._coolTimeEffect)
                {
                    if (_gameplayEffectSystem.TryApplyGameplayEffect(_ability._coolTimeEffect, gameObject, Level, null, out var spec))
                    {
                        _coolTimeSpec = spec as CoolTimeEffect.Spec;
                        _coolTimeSpec.OnEndedEffect += _coolTimeSpec_OnEndedEffect;
                    }
                }

                if (_ability._onAbilityCue.Cue)
                {
                    _ability._onAbilityCue.PlayFromTarget(transform);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _animationPlayer.TryStopAnimation(_animationID);

                EndTrace();
                EndMovement();
            }

            private void _coolTimeSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeSpec_OnEndedEffect;

                _coolTimeSpec = null;
            }

            private void PlayImpactFX()
            {
                if (_ability._onImpactCue.Cue)
                {
                    _ability._onImpactCue.PlayFromTarget(transform);
                }
            }


            public void UpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    UpdateMovement();
                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                UpdateTrace();
            }


            // Movement
            private void OnMovement()
            {
                _movementValue.OnMovement(_ability._moveDistance, _ability._moveCurve);
            }
            private void EndMovement()
            {
                _movementValue.EndMovement();
            }
            private void UpdateMovement()
            {
                if (!_animationPlayer.IsPlaying)
                    return;

                var speed = _movementValue.UpdateMovement(_animationPlayer.NormalizedTime);

                _movementSystem.MovePosition(transform.forward * speed);
            }
            
            // Attack
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
                if (!_sphereCast.IsPlaying)
                    return;

                var trace = _sphereCast.UpdateTrace();

                if (trace.hitCount > 0)
                {
                    bool wasHit = false;

                    for (int i = 0; i < trace.hitCount; i++)
                    {
                        bool isHit = false;
                        var hit = trace.raycastHits[i];

                        Log($"HIT :: {hit.transform.name}");

                        if (hit.transform.TryGetReleationshipSystem(out IRelationshipSystem hitRelationship) && _relationshipSystem.CheckRelationship(hitRelationship) != ERelationship.Hostile)
                            continue;

                        if (hit.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                        {
                            if (_ability._takeDamageEffect)
                            {
                                var data = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, _sphereCast.StartPosition.Direction(_sphereCast.EndPosition), _sphereCast.Owner, gameObject);

                                if (hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeDamageEffect, gameObject, Level, data))
                                {
                                    isHit = true;
                                }

                                data.Release();
                            }

                            if (_ability._takeRadialKnockbackEffect)
                            {
                                var data = TakeRadialKnockbackEffect.Element.Get(_sphereCast.Owner.transform.position);

                                if (hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeRadialKnockbackEffect, gameObject, Level, data))
                                {
                                    isHit = true;
                                }

                                data.Release();
                            }

                            for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnHitToOther.Length; effectIndex++)
                            {
                                var effect = _ability._applyGameplayEffectsOnHitToOther[effectIndex];

                                if (hitGameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level, null))
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
                                float volume = _ability._onHitToOtherCue.Volume;

                                _ability._onHitToOtherCue.Cue.Play(position, rotation, scale, volume);
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

                                _gameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level, null);
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


            private void _animationEvents_OnStartedBlendOut()
            {
                TryFinishAbility();
            }

            private void _animationEvents_OnCanceled()
            {
                CancelAbility();
            }

            private void _animationEvents_OnStarted()
            {
                _wasAnimationStarted = true;
            }
            private void _animationEvents_OnFailed()
            {
                if (!_wasAnimationStarted)
                    return;

                CancelAbility();
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
        }
    }
}