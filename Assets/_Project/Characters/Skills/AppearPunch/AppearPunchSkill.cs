using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.MovementSystem;
using StudioScor.PlayerSystem;
using StudioScor.RotationSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Appear Punch Skill", fileName = "GA_Skill_Appear_Punch")]
    public class AppearPunchSkill : CharacterSkill
    {
        [Header(" [ Appear Punch Skill ] ")]
        [Header(" Animations ")]
        [SerializeField] private string _animationName = "AppearPunch";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0f;

        [Header(" Movement ")]
        [SerializeField][Min(0f)] private float _moveDistance = 5f;
        [SerializeField][Range(0f, 1f)] private float _moveStart = 0.1f;
        [SerializeField][Range(0f, 1f)] private float _moveEnd = 0.3f;
        [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Header(" Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField][Min(0f)] private float _traceRadius = 1f;
        [SerializeField] private SOLayerMaskVariable _traceLayer;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
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

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec, ISkillState
        {
            protected new readonly AppearPunchSkill _ability;
            private readonly IPawnSystem _pawnSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IDilationSystem _dilationSystem;
            private readonly AnimationPlayer _animationPlayer;
            
            private readonly int _animationID;
            private readonly AnimationPlayer.Events _animationEvents;
            private bool _wasStartedAnimation;

            private readonly ReachValueToTime _movementValue = new();
            private readonly TrailSphereCast _sphereCast = new();

            private Cue _impactCue;
            private Vector3 _moveDirection;

            private CoolTimeEffect.Spec _coolTimeEffectSpec;
            private bool InCoolTime => _coolTimeEffectSpec is not null && _coolTimeEffectSpec.IsActivate;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => InCoolTime ? _coolTimeEffectSpec.RemainTime : 0f;
            public float NormalizedCoolTime => InCoolTime ? _coolTimeEffectSpec.NormalizedTime : 1f;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as AppearPunchSkill;
                
                _pawnSystem = gameObject.GetPawnSystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _dilationSystem = gameObject.GetDilationSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
                _movementSystem = gameObject.GetMovementSystem();
                _rotationSystem = gameObject.GetRotationSystem();
                _bodySystem = gameObject.GetBodySystem();
                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);

                _animationID = Animator.StringToHash(_ability._animationName);
                _animationEvents = new();
                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnNotify += _animationEvents_OnNotify;
                _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;

                _ability._onAbilityCue.Initialization();
                _ability._onImpactCue.Initialization();
                _ability._onHitToOtherCue.Initialization();
                _ability._onSuccessedPlayerHit.Initialization();
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _dilationSystem.OnChangedDilation += _dilationSystem_OnChangedDilation;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                _dilationSystem.OnChangedDilation -= _dilationSystem_OnChangedDilation;
            }

            public override bool CanActiveAbility()
            {
                if (_ability._coolTimeEffect && _gameplayEffectSystem.HasEffect(_ability._coolTimeEffect))
                    return false;

                if (!base.CanActiveAbility())
                    return false;
                
                return true;
            }
            private void _dilationSystem_OnChangedDilation(IDilationSystem dilation, float currentDilation, float prevDilation)
            {
                if (!IsPlaying)
                    return;

                if (_impactCue is not null)
                {
                    if (dilation.Speed.SafeEquals(0f))
                        _impactCue.Pause();
                    else
                        _impactCue.Resume();
                }
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _wasStartedAnimation = false;

                _animationPlayer.Play(_animationID, _ability._fadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                _moveDirection = transform.HorizontalDirection(_pawnSystem.LookPosition);

                _rotationSystem.SetRotation(Quaternion.LookRotation(_moveDirection, Vector3.up), false);
                _movementValue.OnMovement(_ability._moveDistance, _ability._moveCurve);

                if(_ability._onAbilityCue.Cue)
                {
                    _ability._onAbilityCue.PlayFromTarget(transform);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                if(_impactCue is not null)
                {
                    _impactCue.Stop();
                    _impactCue = null;
                }

                if(_ability._coolTimeEffect)
                {
                    if (_gameplayEffectSystem.TryApplyGameplayEffect(_ability._coolTimeEffect, gameObject, Level, null, out var spec))
                    {
                        _coolTimeEffectSpec = spec as CoolTimeEffect.Spec;
                        _coolTimeEffectSpec.OnEndedEffect += _coolTimeEffectSpec_OnEndedEffect;
                    }
                }
            }

            

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
                {
                    if(_animationPlayer.IsPlaying)
                    {
                        float normalizedTime = _animationPlayer.NormalizedTime;

                        UpdateTrace();
                        UpdateMovement(normalizedTime);
                    }
                }
            }

            private void OnImpactVFX()
            {
                if(_ability._onImpactCue.Cue)
                {
                    _impactCue = _ability._onImpactCue.PlayFromTarget(transform);
                }
            }

            private void UpdateMovement(float normalizedTime)
            {
                if (!_movementValue.IsPlaying)
                    return;

                var value = Mathf.InverseLerp(_ability._moveStart, _ability._moveEnd, normalizedTime);
                var distance = _movementValue.UpdateMovement(value);

                _movementSystem.MovePosition(_moveDirection * distance);
            }

            private void OnTrace()
            {
                if (_sphereCast.IsPlaying)
                    return;

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

                var (hitCount, hitResults) = _sphereCast.UpdateTrace();

                if (hitCount > 0)
                {
                    bool isHit = false;

                    for (int i = 0; i < hitCount; i++)
                    {
                        var hit = hitResults[i];

                            Log($"HIT :: {hit.transform.name}");

                        if (hit.transform.TryGetReleationshipSystem(out IRelationshipSystem relationship) && _relationshipSystem.CheckRelationship(relationship) != ERelationship.Hostile)
                            continue;

                        if (hit.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                        {
                            isHit = true;

                            if (_ability._takeDamageEffect)
                            {
                                var data = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, _sphereCast.StartPosition.Direction(_sphereCast.EndPosition), _sphereCast.Owner, gameObject);

                                hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeDamageEffect, gameObject, Level, data);

                                data.Release();
                            }

                            for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnHitToOther.Length; effectIndex++)
                            {
                                var effect = _ability._applyGameplayEffectsOnHitToOther[effectIndex];

                                hitGameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level, null);
                            }

                            if (_ability._onHitToOtherCue.Cue)
                            {
                                Vector3 position = hit.distance > 0 ? hit.point + hit.transform.TransformDirection(_ability._onHitToOtherCue.Position)
                                                                    : hit.collider.ClosestPoint(_sphereCast.StartPosition);
                                Vector3 rotation = Quaternion.LookRotation(hit.normal, Vector3.up).eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
                                Vector3 scale = _ability._onHitToOtherCue.Scale;
                                float volume = _ability._onHitToOtherCue.Volume;

                                _ability._onHitToOtherCue.Cue.Play(position, rotation, scale, volume);
                            }
                        }
                    }

                    if (isHit)
                    {
                        if (_gameplayEffectSystem is not null)
                        {
                            for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnSuccessedHit.Length; effectIndex++)
                            {
                                var effect = _ability._applyGameplayEffectsOnSuccessedHit[effectIndex];

                                _gameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level, null);
                            }
                        }

                        if(_pawnSystem.IsPlayer)
                        {
                            if (_ability._onSuccessedPlayerHit.Cue)
                            {
                                _ability._onSuccessedPlayerHit.PlayFromTarget(transform);
                            }
                        }
                    }
                }
            }

            private void _coolTimeEffectSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeEffectSpec_OnEndedEffect;

                _coolTimeEffectSpec = null;
            }

            private void _animationEvents_OnStartedBlendOut()
            {
                TryFinishAbility();
            }

            private void _animationEvents_OnStarted()
            {
                _wasStartedAnimation = true;
            }

            private void _animationEvents_OnCanceled()
            {
                if (!_wasStartedAnimation)
                    return;

                CancelAbility();
            }

            private void _animationEvents_OnFailed()
            {
                CancelAbility();
            }

            private void _animationEvents_OnNotify(string eventName)
            {
                if (!IsPlaying)
                    return;

                switch (eventName)
                {
                    case "Impact":
                        OnImpactVFX();
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
