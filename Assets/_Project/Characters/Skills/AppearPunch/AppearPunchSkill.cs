using StudioScor.Utilities;
using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayEffectSystem;
using PF.PJT.Duet.Pawn.Effect;
using System.Collections.Generic;
using StudioScor.GameplayCueSystem;
using StudioScor.PlayerSystem;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Appear Punch Skill", fileName = "GA_Skill_Appear_Punch")]
    public class AppearPunchSkill : GASAbility
    {
        [Header(" [ Appear Punch Skill ] ")]
        [SerializeField] private string _animationName = "AppearPunch";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0f;

        [Header(" Trace ")]
        [SerializeField] private BodyTag _tracePoint;
        [SerializeField] private float _traceRadius = 1f;
        [SerializeField] private Variable_LayerMask _traceLayer;

        [Header(" Gameplay Effects ")]
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToSelf;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _onImpactCue;
        [SerializeField] private FGameplayCue _onHitToOtherCue;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly AppearPunchSkill _ability;
            private readonly IPawnSystem _pawnSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IDilationSystem _dilationSystem;
            private readonly AnimationPlayer _animationPlayer;
            
            private readonly int _animationID;


            private bool _wasEnabledTrace = false;
            private readonly List<Transform> _ignoreTransforms = new();
            private Vector3 _prevTracePoint;
            private RaycastHit[] _hitResults = new RaycastHit[10];

            private Cue _impactCue;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as AppearPunchSkill;

                
                _pawnSystem = gameObject.GetPawnSystem();
                _dilationSystem = gameObject.GetDilationSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
                _bodySystem = gameObject.GetBodySystem();
                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);

                _animationID = Animator.StringToHash(_ability._animationName);
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

                _animationPlayer.Play(_animationID, _ability._fadeInTime);

                _animationPlayer.OnNotify += _animationPlayer_OnNotify;
                _animationPlayer.OnEnterNotifyState += _animationPlayer_OnEnterNotifyState;
                _animationPlayer.OnExitNotifyState += _animationPlayer_OnExitNotifyState;
            }

            private void _animationPlayer_OnNotify(string eventName)
            {
                switch (eventName)
                {
                    case "Impact":
                        OnImpactVFX();
                        break;
                    default:
                        break;
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

                EndTrace();
            }
            private void _animationPlayer_OnEnterNotifyState(string eventName)
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
            private void _animationPlayer_OnExitNotifyState(string eventName)
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

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
                {
                    UpdateTrace();

                    switch (_animationPlayer.State)
                    {
                        case EAnimationState.BlendOut:
                        case EAnimationState.Finish:
                            TryFinishAbility();
                            return;
                        case EAnimationState.Failed:
                        case EAnimationState.Canceled:
                            CancelAbility();
                            return;
                        default:
                            break;
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
            private void OnTrace()
            {
                if (_wasEnabledTrace)
                    return;

                _wasEnabledTrace = true;

                _ignoreTransforms.Clear();
                _ignoreTransforms.Add(transform);

                var body = _bodySystem.GetBodyPart(_ability._tracePoint);

                _prevTracePoint = body.transform.position;
            }
            private void EndTrace()
            {
                if (!_wasEnabledTrace)
                    return;

                _wasEnabledTrace = false;
            }

            private void UpdateTrace()
            {
                if (!_wasEnabledTrace)
                    return;

                var bodyPart = _bodySystem.GetBodyPart(_ability._tracePoint);

                Vector3 prevPosition = _prevTracePoint;
                Vector3 currentPosition = bodyPart.transform.position;

                _prevTracePoint = currentPosition;

                var hitCount = SUtility.Physics.DrawSphereCastAllNonAlloc(prevPosition, currentPosition, _ability._traceRadius, _hitResults, _ability._traceLayer.Value, QueryTriggerInteraction.Ignore, _ability.UseDebug);

                if (hitCount > 0)
                {
                    bool isHit = false;

                    for (int i = 0; i < hitCount; i++)
                    {
                        var hit = _hitResults[i];

                        if (!_ignoreTransforms.Contains(hit.transform))
                        {
                            _ignoreTransforms.Add(hit.transform);
                            Log($"HIT :: {hit.transform.name}");

                            if (hit.transform.TryGetPawnSystem(out IPawnSystem hitPawn) && hitPawn.Controller.CheckAffiliation(_pawnSystem.Controller) != EAffiliation.Hostile)
                                continue;

                            if (hit.transform.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                            {
                                isHit = true;

                                if (_ability._takeDamageEffect)
                                {
                                    var data = new TakeDamageEffect.FElement(hit.point, hit.normal, hit.collider, prevPosition.Direction(currentPosition), bodyPart.gameObject, gameObject);

                                    hitGameplayEffectSystem.TryTakeEffect(_ability._takeDamageEffect, gameObject, Level, data);
                                }

                                for (int effectIndex = 0; effectIndex < _ability._applyGameplayEffectsOnHitToOther.Length; effectIndex++)
                                {
                                    var effect = _ability._applyGameplayEffectsOnHitToOther[effectIndex];

                                    hitGameplayEffectSystem.TryTakeEffect(effect, gameObject, Level, null);
                                }

                                if (_ability._onHitToOtherCue.Cue)
                                {
                                    Vector3 position = hit.distance > 0 ? hit.point + hit.transform.TransformDirection(_ability._onHitToOtherCue.Position)
                                                                        : hit.collider.ClosestPoint(prevPosition);
                                    Vector3 rotation = Quaternion.LookRotation(hit.normal, Vector3.up).eulerAngles + hit.transform.TransformDirection(_ability._onHitToOtherCue.Rotation);
                                    Vector3 scale = _ability._onHitToOtherCue.Scale;

                                    _ability._onHitToOtherCue.Cue.Play(position, rotation, scale);
                                }
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

                                _gameplayEffectSystem.TryTakeEffect(effect, gameObject, Level, null);
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
        }
    }
}
