using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Rush Thrust Skill", fileName = "GA_Skill_RushThrust")]
    public class RushThrustSkill : CharacterSkill
    {
        [Header(" [ Rush Thrust Skill ] ")]
        [Header(" Animation ")]
        [SerializeField] private string _rushAnimationName;
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0.2f;

        [Header(" Movement ")]
        [SerializeField] private float _moveDistance = 5f;
        [SerializeField][Range(0f, 1f)] private float _moveStartTime = 0.2f;
        [SerializeField][Range(0f, 1f)] private float _moveEndTime = 0.8f;

        [Header(" Turn ")]
        [SerializeField] private GameplayTagSO _turnTag;

        [Header(" Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField] private float _traceRadius = 1f;
        [SerializeField] private Variable_LayerMask _traceLayer;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        [Header(" Gameplay Cue ")]
        [Header(" Attack Cue")]
        [SerializeField] private FGameplayCue _onAttackCue = FGameplayCue.Default;
        [SerializeField][Range(0f, 1f)] private float _attackCueTime = 0.2f;

        [Header(" Hit Cue ")]
        [SerializeField] private FGameplayCue _onHitToOtherCue = FGameplayCue.Default;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit = FGameplayCue.Default;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec, ISkillState
        {
            protected new readonly RushThrustSkill _ability;

            private readonly AnimationPlayer _animationPlayer;
            private readonly IPawnSystem _pawnSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IDilationSystem _dilationSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private readonly int _animationHash;
            private readonly TrailSphereCast _sphereCast = new();
            private readonly ReachValueToTime _movementReachValue = new();
            private readonly AnimationPlayer.Events _animationEvents = new();
            private bool _wasStartedAnimation;

            private readonly MatchTargetWeightMask _matchTargetWeight = new MatchTargetWeightMask(new Vector3(1, 0, 1), 0);
            private Vector3 _moveDirection;

            private bool _wasPlayAttackCue;
            private Cue _onAttackCue;

            private CoolTimeEffect.Spec _coolTimeSpec;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => _coolTimeSpec is null || !_coolTimeSpec .IsActivate ? 0f : _coolTimeSpec.RemainTime;
            public float NormalizedCoolTime => _coolTimeSpec is null || !_coolTimeSpec .IsActivate ? 1f : _coolTimeSpec.NormalizedTime;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as RushThrustSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _pawnSystem = gameObject.GetPawnSystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _bodySystem = gameObject.GetBodySystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
                _dilationSystem = gameObject.GetDilationSystem();

                _dilationSystem.OnChangedDilation += _dilationSystem_OnChangedDilation;

                _animationHash = Animator.StringToHash(_ability._rushAnimationName);
                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;
            }

           

            private void _dilationSystem_OnChangedDilation(IDilationSystem dilation, float currentDilation, float prevDilation)
            {
                if (_onAttackCue is null)
                    return;

                if(currentDilation.SafeEquals(0f))
                {
                    _onAttackCue.Pause();
                }
                else if(currentDilation.SafeEquals(1f))
                {
                    _onAttackCue.Resume();
                }
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

                _wasStartedAnimation = false;

                _animationPlayer.Play(_animationHash, _ability._fadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                _wasPlayAttackCue = !_ability._onAttackCue.Cue;

                if (_ability._coolTimeEffect)
                {
                    if (_gameplayEffectSystem.TryApplyGameplayEffect(_ability._coolTimeEffect, gameObject, Level, null, out var spec))
                    {
                        _coolTimeSpec = spec as CoolTimeEffect.Spec;
                        _coolTimeSpec.OnEndedEffect += _coolTimeSpec_OnEndedEffect;
                    }
                }
            }

            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

                if(_onAttackCue is not null)
                {
                    _onAttackCue.Stop();
                }

                if(_animationPlayer.IsPlayingHash(_animationHash))
                {
                    if (_animationPlayer.Animator.isMatchingTarget)
                        _animationPlayer.Animator.InterruptMatchTarget(false);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _sphereCast.EndTrace();
                _movementReachValue.EndMovement();
            }

            public void UpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    if(_animationPlayer.IsPlaying)
                    {
                        float normalizedTime = _animationPlayer.NormalizedTime;

                        UpdateMovement(normalizedTime);

                        if(!_wasPlayAttackCue && normalizedTime >= _ability._attackCueTime)
                        {
                            _wasPlayAttackCue = true;

                            OnAttackCue();
                        } 
                    }
                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                UpdateTrace();
            }
            private void UpdateMovement(float normalizedTime)
            {
                if (normalizedTime < _ability._moveStartTime || normalizedTime > _ability._moveEndTime)
                    return;

                if(!_movementReachValue.IsPlaying)
                {
                    _moveDirection = transform.HorizontalForward();
                    _movementReachValue.OnMovement(_ability._moveDistance);
                }

                float moveTime = Mathf.InverseLerp(_ability._moveStartTime, _ability._moveEndTime, normalizedTime);

                _movementReachValue.UpdateMovement(moveTime);

                var movePosition = _moveDirection * _movementReachValue.RemainDistance;

                if (_animationPlayer.Animator.isMatchingTarget)
                    _animationPlayer.Animator.InterruptMatchTarget(false);

                _animationPlayer.Animator.MatchTarget(transform.position + movePosition,
                                                      Quaternion.identity,
                                                      AvatarTarget.Root,
                                                      _matchTargetWeight,
                                                      _ability._moveStartTime,
                                                      _ability._moveEndTime);
            }
            private void OnAttackCue()
            {
                if (!IsPlaying)
                    return;

                if (!_ability._onAttackCue.Cue)
                    return;

                _onAttackCue = _ability._onAttackCue.PlayAttached(transform);
                _onAttackCue.OnEndedCue += _onAttackCue_OnEndedCue;
            }

            private void _onAttackCue_OnEndedCue(Cue cue)
            {
                _onAttackCue = null;
            }

            private void OnTurn()
            {
                if (_ability._turnTag)
                    GameplayTagSystem.AddOwnedTag(_ability._turnTag);
            }
            private void EndTurn()
            {
                if (_ability._turnTag)
                    GameplayTagSystem.RemoveOwnedTag(_ability._turnTag);
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

                        if (hit.transform.TryGetReleationshipSystem(out IRelationshipSystem relationship) && _relationshipSystem.CheckRelationship(relationship) != ERelationship.Hostile)
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
                                Vector3 rotation = Quaternion.LookRotation(hit.normal, Vector3.up).eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
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


            private void _coolTimeSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeSpec_OnEndedEffect;

                _coolTimeSpec = null;
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

            private void _animationEvents_OnStartedBlendOut()
            {
                TryFinishAbility();
            }
            private void _animationEvents_OnEnterNotifyState(string eventName)
            {
                Log($"{nameof(_animationPlayer.OnEnterNotifyState)} [ Trigger - {eventName} ] ");
                switch (eventName)
                {
                    case "Trace":
                        OnTrace();
                        break;
                    case "Turn":
                        OnTurn();
                        break;
                    default:
                        break;
                }
            }
            private void _animationEvents_OnExitNotifyState(string eventName)
            {
                Log($"{nameof(_animationPlayer.OnExitNotifyState)} [ Trigger - {eventName} ] ");

                switch (eventName)
                {
                    case "Trace":
                        EndTrace();
                        break;
                    case "Turn":
                        EndTurn();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
