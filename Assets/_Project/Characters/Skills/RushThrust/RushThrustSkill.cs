using PF.PJT.Duet.Define;
using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.MovementSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PF.PJT.Duet.Pawn.PawnSkill
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Rush Thrust Skill", fileName = "GA_Skill_RushThrust")]
    public class RushThrustSkill : GASAbility, ISkill
    {
        [Header(" [ Rush Thrust Skill ] ")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private ESkillType _skillType;

        [Header(" Animation ")]
        [SerializeField] private string _rushAnimationName;
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0.2f;

        [Header(" Movement ")]
        [SerializeField] private float _moveDistance = 5f;
        [SerializeField][Range(0f, 1f)] private float _moveStartTime = 0.2f;
        [SerializeField][Range(0f, 1f)] private float _moveEndTime = 0.8f;

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
        [SerializeField] private FGameplayCue _onHitToOtherCue;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit;

        public Sprite Icon => _icon;
        public ESkillType SkillType => _skillType;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec, ISkillState
        {
            protected new readonly RushThrustSkill _ability;

            private readonly AnimationPlayer _animationPlayer;
            private readonly IPawnSystem _pawnSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private readonly int _animationHash;
            private readonly TrailSphereCast _sphereCast = new();
            private readonly ReachValueToTime _movementReachValue = new();


            private readonly MatchTargetWeightMask _matchTargetWeight = new MatchTargetWeightMask(new Vector3(1, 0, 1), 0);
            private Vector3 _moveDirection;

            private CoolTimeEffect.Spec _coolTimeSpec;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => _coolTimeSpec is null || !_coolTimeSpec .IsActivate ? 0f : _coolTimeSpec.RemainTime;
            public float NormalizedCoolTime => _coolTimeSpec is null || !_coolTimeSpec .IsActivate ? 1f : _coolTimeSpec.NormalizedTime;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as RushThrustSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _pawnSystem = gameObject.GetPawnSystem();
                _bodySystem = gameObject.GetBodySystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _animationHash = Animator.StringToHash(_ability._rushAnimationName);
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
                _animationPlayer.OnStarted += _animationPlayer_OnStarted;
                _animationPlayer.OnEnterNotifyState += _animationPlayer_OnEnterNotifyState;
                _animationPlayer.OnExitNotifyState += _animationPlayer_OnExitNotifyState;

                _movementReachValue.OnMovement(_ability._moveDistance);

                if(_ability._coolTimeEffect)
                {
                    var effect = _gameplayEffectSystem.TryTakeEffect(_ability._coolTimeEffect, gameObject, Level, null);

                    if(effect.isActivate)
                    {
                        _coolTimeSpec = effect.effectSpec as CoolTimeEffect.Spec;
                        _coolTimeSpec.OnEndedEffect += _coolTimeSpec_OnEndedEffect;
                    }
                }
            }

            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

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
                    switch (_animationPlayer.State)
                    {
                        case EAnimationState.Failed:
                        case EAnimationState.Canceled:
                            CancelAbility();
                            break;
                        case EAnimationState.BlendOut:
                        case EAnimationState.Finish:
                            TryFinishAbility();
                            break;
                        case EAnimationState.None:
                            break;
                        case EAnimationState.TryPlay:
                            break;
                        case EAnimationState.Start:
                        case EAnimationState.Playing:
                            float normalizedTime = _animationPlayer.NormalizedTime;

                            UpdateMovement(normalizedTime);

                            break;
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
                                Vector3 rotation = Quaternion.LookRotation(hit.normal, Vector3.up).eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
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

            private void _coolTimeSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeSpec_OnEndedEffect;

                _coolTimeSpec = null;
            }
            private void _animationPlayer_OnStarted()
            {
                _moveDirection = transform.HorizontalForward();
            }

            private void _animationPlayer_OnEnterNotifyState(string eventName)
            {
                Log($"{nameof(_animationPlayer.OnEnterNotifyState)} [ Trigger - {eventName} ] ");
                switch (eventName)
                {
                    case "Trace":
                        OnTrace();
                        break;

                    default:
                        break;
                }
            }
            private void _animationPlayer_OnExitNotifyState(string eventName)
            {
                Log($"{nameof(_animationPlayer.OnExitNotifyState)} [ Trigger - {eventName} ] ");

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