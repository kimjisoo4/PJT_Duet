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
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Appear Arrow Skill", fileName = "GA_Skill_Appear_Arrow")]
    public class AppearArrowSkill : CharacterSkill
    {
        [Header(" [ Appear Arrow Skill ] ")]
        [Header(" Animations ")]
        [SerializeField] private string _animationName = "AppearArrow";
        [SerializeField][Range(0f, 1f)] private float _fadeInTime = 0f;

        [Header(" Movement ")]
        [SerializeField] private float _addForcePower = 10f;

        [Header(" Arrow Rain ")]
        [SerializeField] private PoolContainer _arrowRainPool;

        [Header(" Trace ")]
        [Header(" Spawn ")]
        [SerializeField][Min(0f)] private float _spawnTraceRadius = 1f;
        [SerializeField] private float _spawnLimitDistance = 100f;
        [SerializeField] private Variable_LayerMask _spawnTraceLayer;
        [SerializeField] private Variable_LayerMask _groundTraceLayer;

        [Header(" Attack ")]
        [SerializeField][Min(0f)] private float _traceRadius = 1f;
        [SerializeField] private Variable_LayerMask _traceLayer;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToSelf;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        [Header(" Area Effects")]
        [SerializeField] private GameplayEffect[] _areaEnterEffects;
        [SerializeField] private GameplayEffect[] _areaExitEffects;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _onAbilityCue;
        [SerializeField] private FGameplayCue _onImpactCue;
        [SerializeField] private FGameplayCue _onAreaPointCue;
        [SerializeField] private FGameplayCue _onHitToOtherCue;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit;


        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, ISkillState, IAreaAbility, IUpdateableAbilitySpec
        {
            protected new readonly AppearArrowSkill _ability;

            private readonly IPawnSystem _pawnSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IDilationSystem _dilationSystem;
            private readonly IAddForceable _addForceable;

            private readonly AnimationPlayer _animationPlayer;

            private readonly int _animationID;
            private readonly AnimationPlayer.Events _animationEvents;
            private bool _wasStartedAnimation;

            private readonly OverlapSphereCast _overlapSphere = new();

            private Vector3 _startPosition;
            private Vector3 _hitPoint;

            private Cue _impactCue;
            private Vector3 _moveDirection;

            private CoolTimeEffect.Spec _coolTimeEffectSpec;
            private bool InCoolTime => _coolTimeEffectSpec is not null && _coolTimeEffectSpec.IsActivate;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => InCoolTime ? _coolTimeEffectSpec.RemainTime : 0f;
            public float NormalizedCoolTime => InCoolTime ? _coolTimeEffectSpec.NormalizedTime : 1f;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as AppearArrowSkill;

                _pawnSystem = gameObject.GetPawnSystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _dilationSystem = gameObject.GetDilationSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
                _movementSystem = gameObject.GetMovementSystem();
                _rotationSystem = gameObject.GetRotationSystem();
                _bodySystem = gameObject.GetBodySystem();
                _addForceable = gameObject.GetComponent<IAddForceable>();

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);

                _animationID = Animator.StringToHash(_ability._animationName);
                _animationEvents = new();
                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnNotify += _animationEvents_OnNotify;

                _ability._arrowRainPool.Initialization();

                _ability._onAbilityCue.Initialization();
                _ability._onImpactCue.Initialization();
                _ability._onAreaPointCue.Initialization();
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

                _startPosition = transform.position;
                _wasStartedAnimation = false;

                _animationPlayer.Play(_animationID, _ability._fadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;

                _moveDirection = transform.HorizontalDirection(_pawnSystem.LookPosition);

                _rotationSystem.SetRotation(Quaternion.LookRotation(_moveDirection, Vector3.up), false);

                if (_ability._onAbilityCue.Cue)
                {
                    _ability._onAbilityCue.PlayFromTarget(transform);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                EndTrace();

                if (_impactCue is not null)
                {
                    _impactCue.Stop();
                    _impactCue = null;
                }

                if (_ability._coolTimeEffect)
                {
                    if (_gameplayEffectSystem.TryApplyGameplayEffect(_ability._coolTimeEffect, gameObject, Level, null, out var spec))
                    {
                        _coolTimeEffectSpec = spec as CoolTimeEffect.Spec;
                        _coolTimeEffectSpec.OnEndedEffect += _coolTimeEffectSpec_OnEndedEffect;
                    }
                }
            }

            public void UpdateAbility(float deltaTime)
            {
                return;
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                if (!IsPlaying)
                    return;

                if(_overlapSphere.IsPlaying)
                {
                    UpdateTrace();
                    EndTrace();
                }
            }

            private void OnStartedAnimation()
            {
                _wasStartedAnimation = true;
            }
            private void OnImpactVFX()
            {
                if (_ability._onImpactCue.Cue)
                {
                    _impactCue = _ability._onImpactCue.PlayFromTarget(transform);
                }
            }

            private void OnSpawnArea()
            {
                Log(nameof(OnSpawnArea));

                Vector3 start = transform.position;
                Vector3 direction = start.Direction(_startPosition);

                if (!Physics.SphereCast(start, _ability._spawnTraceRadius, direction, out RaycastHit hit, _ability._spawnLimitDistance, _ability._spawnTraceLayer.Value, QueryTriggerInteraction.Ignore))
                    return;

                if(!hit.transform.gameObject.ContainLayer(_ability._groundTraceLayer.Value))
                {
                    if (!Physics.Raycast(hit.point, Vector3.down, out hit, _ability._spawnLimitDistance, _ability._groundTraceLayer.Value, QueryTriggerInteraction.Ignore))
                        return;
                }

                _hitPoint = hit.point;

                var poolObject = _ability._arrowRainPool.Get();

                poolObject.transform.SetPositionAndRotation(_hitPoint, Quaternion.identity);

                var spawnActor = poolObject.GetComponent<ISpawnedActorByAbility>();

                spawnActor.Activate(gameObject, this);

                spawnActor.Play();

                if(_ability._onAreaPointCue.Cue)
                {
                    var cue = _ability._onAreaPointCue;

                    _ability._onAreaPointCue.Cue.Play(_hitPoint, transform.eulerAngles, cue.Scale, cue.Volume);
                }

                _addForceable.AddForce(transform.forward * -_ability._addForcePower);

                OnTrace();
                
            }
            public void EnterArea(ISpawnedActorByAbility spawnedActor, GameObject enterActor)
            {
                Log(enterActor);

                if (!enterActor.TryGetReleationshipSystem(out IRelationshipSystem releationship)
                    || _relationshipSystem.CheckRelationship(releationship) != ERelationship.Hostile)
                    return;

                if(enterActor.TryGetGameplayEffectSystem(out IGameplayEffectSystem gameplayEffectSystem))
                {
                    foreach (var effect in _ability._areaEnterEffects)
                    {
                        gameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level);
                    }
                }
            }

            public void ExitArea(ISpawnedActorByAbility spawnedActor, GameObject exitActor)
            {
                Log(exitActor);

                if (!exitActor.TryGetReleationshipSystem(out IRelationshipSystem releationship)
                    || _relationshipSystem.CheckRelationship(releationship) != ERelationship.Hostile)
                    return;

                if (exitActor.TryGetGameplayEffectSystem(out IGameplayEffectSystem gameplayEffectSystem))
                {
                    foreach (var effect in _ability._areaExitEffects)
                    {
                        gameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level);
                    }
                }
            }

            private void OnTrace()
            {
                Log(nameof(OnTrace));

                _overlapSphere.SetOwner(gameObject);

                _overlapSphere.TracePosition = _hitPoint;
                _overlapSphere.TraceRadius = _ability._traceRadius;
                _overlapSphere.TraceLayer = _ability._traceLayer.Value;
                _overlapSphere.UseInLocalSpace = false;
                _overlapSphere.UseDebug = _ability.UseDebug;

                _overlapSphere.OnTrace();
            }

            private void EndTrace()
            {
                Log(nameof(EndTrace));

                _overlapSphere.EndTrace();
            }

            private void UpdateTrace()
            {
                var trace = _overlapSphere.UpdateTrace();

                if (trace.hitCount <= 0)
                    return;

                for(int i = 0; i < trace.hitCount; i++)
                {
                    var hitResult = trace.raycastHits[i];
                    Log(hitResult.collider.gameObject);

                    if (!hitResult.transform.TryGetActor(out IActor hitActor))
                        continue;

                    if (!hitActor.gameObject.TryGetReleationshipSystem(out IRelationshipSystem relationship)
                        || _relationshipSystem.CheckRelationship(relationship) != ERelationship.Hostile)
                        continue;

                    if (hitActor.gameObject.TryGetGameplayEffectSystem(out IGameplayEffectSystem gameplayEffectSystem))
                    {
                        if (_ability._takeDamageEffect)
                        {
                            Vector3 attackDirection = transform.forward;
                            var element = TakeDamageEffect.Element.Get(hitResult.point, hitResult.normal, hitResult.collider, attackDirection, gameObject, gameObject);
                            gameplayEffectSystem.TryApplyGameplayEffect(_ability._takeDamageEffect, gameObject, Level, element);
                            element.Release();
                        }

                        if (!_ability._applyGameplayEffectsOnHitToOther.IsNullOrEmpty())
                        {
                            foreach (var gameplayEffect in _ability._applyGameplayEffectsOnHitToOther)
                            {
                                gameplayEffectSystem.TryApplyGameplayEffect(gameplayEffect, gameObject, Level);
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

            private void _animationEvents_OnStarted()
            {
                OnStartedAnimation();
            }

            private void _animationEvents_OnStartedBlendOut()
            {
                TryFinishAbility();
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
                    case "Shoot":
                        OnSpawnArea();
                        break;
                    default:
                        break;
                }
            }

            
        }
    }
}
