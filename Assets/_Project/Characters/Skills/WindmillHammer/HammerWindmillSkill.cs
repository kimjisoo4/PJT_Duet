using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Hammer Windmill Skill", fileName = "GA_Skill_HammerWindmill")]
    public class HammerWindmillSkill : CharacterSkill
    {
        [Header(" [ Windmill Hammer Skill ] ")]
        [Header(" Animation ")]
        [SerializeField] private string _startAnimationName = "HammerWindmill_Start";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0.2f;
        [SerializeField] private string _loopAnimationName = "HammerWindmill_Loop";
        [SerializeField] private string _endAnimationName = "HammerWindmill_End";
        [SerializeField][Range(0f, 1f)] private float _blendEndTime = 0.2f;

        [Header(" Attack Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField] private Variable_LayerMask _traceLayer;
        [SerializeField] private float _traceRadius = 1f;
        [SerializeField] private float _hitInterval = 0.2f;

        [Header(" Gameplay Tag ")]
        [SerializeField] private FGameplayTags _grantMovementTag;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        [Header(" Gameplay Cue ")]
        [Header(" Windmill Loop Cue ")]
        [SerializeField][Range(0f, 1f)] private float _onWindmillTime = 0.2f;
        [SerializeField] private FGameplayCue _onWimdmillCue = FGameplayCue.Default;

        [Header(" Attack Cue ")]
        [SerializeField] private FGameplayCue _onAttackCue = FGameplayCue.Default;

        [Space(5f)]
        [SerializeField] private FGameplayCue _onHitToOtherCue = FGameplayCue.Default;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit = FGameplayCue.Default;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }
        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec, ISkillState
        {
            public enum EState
            {
                None,
                Start,
                Loop,
                End,
            }

            protected new readonly HammerWindmillSkill _ability;

            private readonly int _startAnimationHash;
            private readonly int _loopAnimationHash;
            private readonly int _endAnimationHash;

            private readonly IPawnSystem _pawnSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private readonly AnimationPlayer _animationPlayer;
            private readonly AnimationPlayer.Events _animationEvents;
            private bool _wasStartedAnimation;

            private readonly TrailSphereCast _sphereCast = new();
            private readonly Timer _hitTimer = new();

            private bool _wasReleased = false;
            private EState _animationState;
            private bool _wasRemovedTag = false;

            private bool _wasPlayWindmillCue;
            private Cue _windmillCue;

            private CoolTimeEffect.Spec _coolTimeSpec;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 0f : _coolTimeSpec.RemainTime;
            public float NormalizedCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 1f : _coolTimeSpec.NormalizedTime;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as HammerWindmillSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _bodySystem = gameObject.GetBodySystem();
                _pawnSystem = gameObject.GetPawnSystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _startAnimationHash = Animator.StringToHash(_ability._startAnimationName);
                _loopAnimationHash = Animator.StringToHash(_ability._loopAnimationName);
                _endAnimationHash = Animator.StringToHash(_ability._endAnimationName);

                _animationEvents = new();
                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnNotify += _animationPlayer_OnNotify;
                _animationEvents.OnStartedBlendOut += _animationPlayer_OnStartedBlendOut;
                _animationEvents.OnCanceled += _animationPlayer_OnCanceled;
                _animationEvents.OnFailed += _animationPlayer_OnFailed;
                _animationEvents.OnNotify += _animationPlayer_OnNotify;

                _ability._onWimdmillCue.Initialization();
                _ability._onAttackCue.Initialization();
                _ability._onHitToOtherCue.Initialization();
                _ability._onSuccessedPlayerHit.Initialization();
            }


            public override bool CanActiveAbility()
            {
                if (_ability._coolTimeEffect && _gameplayEffectSystem.HasEffect(_ability._coolTimeEffect))
                    return false;

                return base.CanActiveAbility();
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                TransitionState(EState.Start);

                _wasPlayWindmillCue = !_ability._onWimdmillCue.Cue;

                if (_ability._coolTimeEffect)
                {
                    if (_gameplayEffectSystem.TryApplyGameplayEffect(_ability._coolTimeEffect, gameObject, Level, null, out var spec))
                    {
                        _coolTimeSpec = spec as CoolTimeEffect.Spec;
                        _coolTimeSpec.OnEndedEffect += _coolTimeSpec_OnEndedEffect;
                    }
                }
            }
            private void _coolTimeSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeSpec_OnEndedEffect;

                _coolTimeSpec = null;
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _hitTimer.EndTimer();
                _sphereCast.EndTrace();
                EndRemoveTag();

                TransitionState(EState.None);
            }
            protected override void OnReleaseAbility()
            {
                base.OnReleaseAbility();

                _wasReleased = true;
            }

            public void UpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    UpdateState(deltaTime);

                    if (_sphereCast.IsPlaying)
                    {
                        _hitTimer.UpdateTimer(deltaTime);

                        if (_hitTimer.IsFinished)
                        {
                            if(_ability._onAttackCue.Cue)
                            {
                                _ability._onAttackCue.PlayAttached(transform);
                            }
                            
                            EndTrace();
                            OnTrace();

                            _hitTimer.OnTimer();
                        }
                    }
                }
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    UpdateTrace();
                }
            }

            private void TransitionState(EState newState)
            {
                if (_animationState == newState)
                    return;

                switch (_animationState)
                {
                    case EState.None:
                        break;
                    case EState.Start:
                        EndWindmillStart();
                        break;
                    case EState.Loop:
                        EndWindmillLoop();
                        break;
                    case EState.End:
                        EndWindmillEnd();
                        break;
                    default:
                        break;
                }

                Log($"Current State - {newState} || Prev State - {_animationState}");
                _animationState = newState;


                switch (_animationState)
                {
                    case EState.None:
                        break;
                    case EState.Start:
                        OnWindmillStart();
                        break;
                    case EState.Loop:
                        OnWindmillLoop();
                        break;
                    case EState.End:
                        OnWindmillEnd();
                        break;
                    default:
                        break;
                }
            }

            private void UpdateState(float deltaTime)
            {
                switch (_animationState)
                {
                    case EState.None:
                        break;
                    case EState.Start:
                        UpdateWindmillStart(deltaTime);
                        break;
                    case EState.Loop:
                        UpdateWindmillLoop(deltaTime);
                        break;
                    case EState.End:
                        UpdateWindmillEnd(deltaTime);
                        break;
                    default:
                        break;
                }
            }


            private void OnWindmillStart()
            {
                _wasReleased = false;

                _wasStartedAnimation = false;
                _animationPlayer.Play(_startAnimationHash, _ability._fadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                _hitTimer.OnTimer(_ability._hitInterval);
            }
            private void UpdateWindmillStart(float deltaTime)
            {
                if(!_wasPlayWindmillCue)
                {
                    if(_animationPlayer.NormalizedTime >= _ability._onWindmillTime)
                    {
                        _wasPlayWindmillCue = true;
                        _windmillCue = _ability._onWimdmillCue.PlayAttached(transform);
                    }
                }
            }
            private void EndWindmillStart()
            {
            }


            private void OnWindmillLoop()
            {
            }

            private void UpdateWindmillLoop(float deltaTime)
            {
                if (_animationPlayer.Animator.TryGetAnimatorState(0, _loopAnimationHash, out AnimatorStateInfo animatorState))
                {
                    float normalizedTime = animatorState.normalizedTime % 1f;

                    if (normalizedTime >= 0.8f && _wasReleased)
                    {
                        TransitionState(EState.End);
                    }
                }
            }
            private void EndWindmillLoop()
            {
                if(_windmillCue is not null)
                {
                    _windmillCue.Stop();
                    _windmillCue = null;
                }
            }


            private void OnWindmillEnd()
            {
                _animationPlayer.Play(_endAnimationHash, _ability._blendEndTime);
                _animationPlayer.AnimationEvents = _animationEvents;
            }
            private void UpdateWindmillEnd(float deltaTime)
            {
            }
            private void EndWindmillEnd()
            {
            }

            private void _animationPlayer_OnStartedBlendOut()
            {
                switch (_animationState)
                {
                    case EState.None:
                        break;
                    case EState.Start:
                        TransitionState(EState.Loop);
                        break;
                    case EState.Loop:
                        break;
                    case EState.End:
                        TryFinishAbility();
                        break;
                    default:
                        break;
                }
            }
            private void _animationPlayer_OnFailed()
            {
                if (!IsPlaying)
                    return;

                switch (_animationState)
                {
                    case EState.None:
                        break;
                    case EState.Start:
                        if (_animationPlayer.IsPlayingHash(_startAnimationHash))
                            CancelAbility();
                        break;
                    case EState.Loop:
                        break;
                    case EState.End:
                        if (_animationPlayer.IsPlayingHash(_endAnimationHash))
                            CancelAbility();
                        break;
                    default:
                        break;
                }
            }

            private void _animationEvents_OnStarted()
            {
                _wasStartedAnimation = true;
            }

            private void _animationPlayer_OnCanceled()
            {
                if (!IsPlaying)
                    return;

                if (!_wasStartedAnimation)
                    return;

                switch (_animationState)
                {
                    case EState.None:
                        break;
                    case EState.Start:
                        if (_animationPlayer.IsPlayingHash(_startAnimationHash))
                            CancelAbility();
                        break;
                    case EState.Loop:
                        break;
                    case EState.End:
                        if (_animationPlayer.IsPlayingHash(_endAnimationHash))
                            CancelAbility();
                        break;
                    default:
                        break;
                }
            }

            private void _animationPlayer_OnNotify(string eventName)
            {
                switch (eventName)
                {
                    case "OnTrace":
                        OnTrace();
                        break;
                    case "EndTrace":
                        EndTrace();
                        break;
                    case "OnRemoveTag":
                        OnRemoveTag();
                        break;
                    case "EndRemoveTag":
                        EndRemoveTag();
                        break;
                    default:
                        break;
                }
            }

            

            private void OnRemoveTag()
            {
                if (_wasRemovedTag)
                    return;

                _wasRemovedTag = true;

                GameplayTagSystem.RemoveGameplayTags(_ability._grantMovementTag);
            }
            private void EndRemoveTag()
            {
                if (!_wasRemovedTag)
                    return;

                _wasRemovedTag = false;

                GameplayTagSystem.AddGameplayTags(_ability._grantMovementTag);
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

            private bool CheckAffilation(Transform target)
            {
                if (!target)
                    return false;

                if (!target.TryGetReleationshipSystem(out IRelationshipSystem releationship))
                    return false;

                if (_relationshipSystem.CheckRelationship(releationship) != ERelationship.Hostile)
                    return false; 

                return true;
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


                        if (!CheckAffilation(hit.transform))
                            continue;

                        Log($"HIT :: {hit.transform.name}");

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
        }
    }
}
