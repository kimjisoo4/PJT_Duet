using PF.PJT.Duet.Pawn;
using PF.PJT.Duet.Pawn.Effect;
using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.RotationSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Explosive Arrow Skill", fileName = "GA_Skill_ExplosiveArrow")]
    public class ExplosiveArrowSkill : CharacterSkill
    {
        [Header(" [ Explosive Arrow Skill ] ")]
        [Header(" Animation ")]
        [SerializeField] private string _animationName = "";
        [SerializeField] private float _fadeInTime = 0.2f;

        [Header(" Find Target ")]
        [SerializeField][SUnit(SUtility.UNIT_METER)] private float _findTargetRadius = 20f;
        [SerializeField][SUnit(SUtility.UNIT_DEGREE)] private float _findTargetAngle = 15f;
        [SerializeField] private int _findTargetCount = 20;
        [SerializeField] private SOLayerMaskVariable _findTargetLayer;
        [SerializeField] private SOLayerMaskVariable _obstacleLayer;

        [Header(" Spawned Actor ")]
        [Header(" Projectile Arrow ")]
        [SerializeField] private PoolContainer _arrowPoolContainer;
        [SerializeField] private BodyTag _arrowSpawnPointTag;
        [SerializeField] private GameplayTag _arrowTag;

        [Header(" Explosive Arrow ")]
        [SerializeField] private PoolContainer _explosivePoolContainer;

        [Header(" Turning ")]
        [SerializeField][SUnit(SUtility.UNIT_DEGREE_PER_SEC)] private float _turnSpeed = 720f;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;

        [Header(" Arrow Hit ")]
        [SerializeField] private TakeDamageEffect _arrowTakeDamageEffect;
        [SerializeField] private GameplayEffect[] _arrowApplyGameplayEffectOnHitToOther;

        [Header(" Explosive Hit ")]
        [SerializeField] private TakeDamageEffect _explosiveTakeDamageEffect;
        [SerializeField] private TakeRadialKnockbackEffect _explosiveTakeRadialKnockbackEffect;
        [SerializeField] private GameplayEffect[] _explosiveApplyGameplayEffectOnHitToOther;

        [Header(" Gameplay Cues ")]
        [SerializeField] private FGameplayCue _onShootCue;
        [SerializeField] private FGameplayCue _onHitCue;
        [SerializeField] private FGameplayCue _onExplosiveCue;
        
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec, ITakeDamageAbility, ISkillState
        {
            protected new readonly ExplosiveArrowSkill _ability;

            private readonly ICharacter _character;
            private readonly IPawnSystem _pawnSystem;
            private readonly IRotationSystem _rotationSystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly IBodySystem _bodySystem;
            private readonly IRelationshipSystem _relationshipSystem;

            private readonly AnimationPlayer _animationPlayer;
            private readonly AnimationPlayer.Events _animationEvents;
            private readonly int _animationHash;
            private bool _wasStartedAnimation = false;
            
            private bool _canTurn;
            private Collider[] _findColliders;

            private ISpawnedActorByAbility _spawnedArrow;
            private CoolTimeEffect.Spec _coolTimeSpec;

            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => _coolTimeSpec is null ? 0f : _coolTimeSpec.RemainTime;
            public float NormalizedCoolTime => _coolTimeSpec is null ? 1f : _coolTimeSpec.NormalizedTime;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as ExplosiveArrowSkill;

                _character = gameObject.GetComponent<ICharacter>();
                _pawnSystem = gameObject.GetPawnSystem();
                _rotationSystem = gameObject.GetRotationSystem();
                _bodySystem = gameObject.GetBodySystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _animationPlayer = _character.Model.GetAnimationPlayer();
                _animationHash = Animator.StringToHash(_ability._animationName);
                _animationEvents = new();

                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnNotify += _animationEvents_OnNotify;
                _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;

                _ability._arrowPoolContainer.Initialization();
                _ability._explosivePoolContainer.Initialization();

                _ability._onShootCue.Initialization();
                _ability._onHitCue.Initialization();
                _ability._onExplosiveCue.Initialization();
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

                _animationPlayer.Play(_animationHash, _ability._fadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;
                
                if(_gameplayEffectSystem.TryApplyGameplayEffect(_ability._coolTimeEffect, gameObject, Level, null, out var spec))
                {
                    _coolTimeSpec = spec as CoolTimeEffect.Spec;
                    _coolTimeSpec.OnEndedEffect += _coolTimeSpec_OnEndedEffect;
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

                EndTurn();
                _animationPlayer.TryStopAnimation(_animationHash);

                if(_spawnedArrow is not null)
                {
                    _spawnedArrow.Deactivate();
                    _spawnedArrow = null;
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
                    UpdateTurning(deltaTime);
                }
            }

            // Find Target 

            private Collider FindTarget(Vector3 origin)
            {
                Log(nameof(FindTarget));

                if (_findColliders is null || _findColliders.Length != _ability._findTargetCount)
                {
                    _findColliders = new Collider[_ability._findTargetCount];
                }

                Vector3 lookDirection = _pawnSystem.LookDirection;

                var hitCount = SUtility.Physics.DrawOverlapSphereNoneAlloc(origin, _ability._findTargetRadius, _findColliders, _ability._findTargetLayer.Value, QueryTriggerInteraction.UseGlobal, UseDebug);
                Collider target = null;
                float bestAngle = _ability._findTargetAngle;

                for (int i = 0; i < hitCount; i++)
                {
                    var col = _findColliders[i];

                    if (!col.TryGetActor(out IActor actor))
                        continue;

                    if (actor.transform == transform)
                        continue;

                    if (actor.transform.TryGetReleationshipSystem(out IRelationshipSystem relationship)
                        && _relationshipSystem.CheckRelationship(relationship) != ERelationship.Hostile)
                        continue;

                    Vector3 targetDirection = origin.Direction(col.transform.position);
                    float angle = Vector3.Angle(lookDirection, targetDirection);

                    if (angle.InRangeAngle(bestAngle))
                    {
                        Vector3 targetCenter = col.bounds.center;
                        Vector3 direction = origin.Direction(targetCenter, false);
                        float distance = direction.magnitude;

                        if (Physics.Raycast(origin, targetCenter, distance, _ability._obstacleLayer.Value))
                            break;

                        bestAngle = angle;
                        target = col;
                    }
                }


                Log($"{nameof(FindTarget)} - Result : {target}");

                return target;
            }




            public void OnHit(ISpawnedActorByAbility spawnedActor, RaycastHit[] hits, int hitCount)
            {
                if (hitCount == 0)
                    return;

                if(spawnedActor.GameplayTagSystem.ContainOwnedTag(_ability._arrowTag))
                {
                    Log($"{nameof(OnHit)} - Arrow");

                    for (int i = 0; i < hitCount; i++)
                    {
                        var hit = hits[i];

                        if (!hit.transform.TryGetActor(out IActor actor))
                            continue;

                        if (!actor.gameObject.TryGetReleationshipSystem(out IRelationshipSystem relationship) || !_relationshipSystem.IsHostile(relationship))
                            continue;

                        if (!actor.gameObject.TryGetGameplayEffectSystem(out IGameplayEffectSystem gameplayEffectSystem))
                            continue;

                        if (_ability._arrowTakeDamageEffect)
                        {
                            var data = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, spawnedActor.transform.forward, spawnedActor.gameObject, gameObject);
                            gameplayEffectSystem.TryApplyGameplayEffect(_ability._arrowTakeDamageEffect, gameObject, Level, data);
                            data.Release();
                        }

                        if(!_ability._arrowApplyGameplayEffectOnHitToOther.IsNullOrEmpty())
                        {
                            foreach (var effect in _ability._arrowApplyGameplayEffectOnHitToOther)
                            {
                                gameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level);
                            }
                        }
                    }

                    OnSpawnExplosive(hits[0].point + spawnedActor.transform.TransformDirection(Vector3.back), spawnedActor.transform.rotation);

                    spawnedActor.Deactivate();
                }
                else
                {
                    Log($"{nameof(OnHit)} - Explosive");

                    for (int i = 0; i < hitCount; i++)
                    {
                        var hit = hits[i];

                        if (!hit.transform.TryGetActor(out IActor actor))
                            continue;

                        if (!actor.gameObject.TryGetReleationshipSystem(out IRelationshipSystem relationship) || !_relationshipSystem.IsHostile(relationship))
                            continue;

                        if (!actor.gameObject.TryGetGameplayEffectSystem(out IGameplayEffectSystem gameplayEffectSystem))
                            continue;

                        if (_ability._explosiveTakeDamageEffect)
                        {
                            var data = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, spawnedActor.transform.forward, spawnedActor.gameObject, gameObject);
                            gameplayEffectSystem.TryApplyGameplayEffect(_ability._explosiveTakeDamageEffect, gameObject, Level, data);
                            data.Release();
                        }

                        if(_ability._explosiveTakeRadialKnockbackEffect)
                        {
                            var data = TakeRadialKnockbackEffect.Element.Get(spawnedActor.transform.TransformPoint(Vector3.back));
                            gameplayEffectSystem.TryApplyGameplayEffect(_ability._explosiveTakeRadialKnockbackEffect, gameObject, Level, data);
                            data.Release();
                        }

                        if (!_ability._explosiveApplyGameplayEffectOnHitToOther.IsNullOrEmpty())
                        {
                            foreach (var effect in _ability._explosiveApplyGameplayEffectOnHitToOther)
                            {
                                gameplayEffectSystem.TryApplyGameplayEffect(effect, gameObject, Level);
                            }
                        }
                    }
                }
            }

            private void OnSpawnArrow()
            {
                var arrowActor = _ability._arrowPoolContainer.Get();
                var body = _bodySystem.GetBodyPart(_ability._arrowSpawnPointTag);

                var spawnPoint = body is not null ? body.transform : transform;

                arrowActor.transform.SetParent(spawnPoint);
                arrowActor.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                _spawnedArrow = arrowActor.GetComponent<ISpawnedActorByAbility>();

                _spawnedArrow.Activate(gameObject, this);
                _spawnedArrow.GameplayTagSystem.AddOwnedTag(_ability._arrowTag);
            }
            private void OnShootArrow()
            {
                if(_spawnedArrow is not null)
                {
                    Vector3 direction;

                    if(_pawnSystem.IsPlayer)
                    {
                        var target = FindTarget(_spawnedArrow.transform.position);

                        if (target)
                        {
                            direction = _spawnedArrow.transform.Direction(target.bounds.center);
                        }
                        else
                        {
                            direction = transform.forward;
                        }
                    }   
                    else
                    {
                        direction = transform.forward;
                    }
                    _spawnedArrow.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

                    _spawnedArrow.Play();

                    _spawnedArrow = null;
                }    
            }
            private void OnSpawnExplosive(Vector3 spawnPosition, Quaternion spawnRotation)
            {
                var explosiveActor = _ability._explosivePoolContainer.Get();

                explosiveActor.transform.SetPositionAndRotation(spawnPosition, spawnRotation);

                var spawnedActor = explosiveActor.GetComponent<ISpawnedActorByAbility>();

                spawnedActor.Activate(gameObject, this);
                spawnedActor.Play();
            }

            // Turning
            private void OnTurn()
            {
                if (_canTurn)
                    return;

                _canTurn = true;
            }
            private void EndTurn()
            {
                if (!_canTurn)
                    return;

                _canTurn = false;

            }
            private void UpdateTurning(float deltaTime)
            {
                if (!_canTurn)
                    return;

                Quaternion lookRotation = Quaternion.LookRotation(_pawnSystem.LookDirection, transform.up);
                float yaw = Mathf.MoveTowardsAngle(transform.eulerAngles.y, lookRotation.eulerAngles.y, _ability._turnSpeed * deltaTime);

                _rotationSystem.SetRotation(Quaternion.Euler(0, yaw, 0), false);
            }
            private void _animationEvents_OnNotify(string eventName)
            {
                if (!IsPlaying)
                    return;

                switch (eventName)
                {
                    case "Spawn":
                        OnSpawnArrow();
                        break;
                    case "Shoot":
                        OnShootArrow();
                        break;
                    default:
                        break;
                }    
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

            private void _animationEvents_OnStarted()
            {
                _wasStartedAnimation = true;
            }
            private void _animationEvents_OnEnterNotifyState(string eventName)
            {
                switch (eventName)
                {
                    case "Turn":
                        OnTurn();
                        break;
                }
            }

            private void _animationEvents_OnExitNotifyState(string eventName)
            {
                switch (eventName)
                {
                    case "Turn":
                        EndTurn();
                        break;
                }
            }
        }
    }
}
