using PF.PJT.Duet.Pawn.Effect;
using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Default Bow Attack Skill", fileName = "GA_Skill_", order = -999999)]
    public class DefaultBowAttackSkill : CharacterSkill
    {
        [Header(" [ Default Bow Attack Skill ] ")]
        [Header(" Animation ")]
        [SerializeField] private string _animationName = "BowAttack_01";
        [SerializeField] private float _fadeInTime = 0.2f;

        [Header(" Rotation ")]
        [SerializeField] private GameplayTagSO _turnTag;

        [Header(" Projectile ")]
        [SerializeField] private PoolContainer _projectilePool;
        [SerializeField] private BodyTag _projectileSpawnPoint;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;
        [SerializeField] private TakeDamageEffect _takeDamageEffect;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;

        [Header(" Gameplay Cue ")]
        [Header(" Attack Cue")]
        [SerializeField][Range(0f, 1f)] private float _attackCueTime = 0.2f;
        [SerializeField] private FGameplayCue _onAttackCue = FGameplayCue.Default;
        
        [Header(" Hit Cue ")]
        [SerializeField] private FGameplayCue _onHitToOtherCue = FGameplayCue.Default;
        [SerializeField] private FGameplayCue _onSuccessedPlayerHit = FGameplayCue.Default;

        [Header(" Combo ")]
        [SerializeField] private GameplayTagSO _comboTag;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec, ITakeDamageAbility
        {
            protected new readonly DefaultBowAttackSkill _ability;
            private readonly AnimationPlayer _animationPlayer;
            private readonly IBodySystem _bodySystem;
            private readonly IRelationshipSystem _relationshipSystem;

            private readonly AnimationPlayer.Events _animationEvents;
            private readonly int _animationID;
            private bool _wasStartedAnimation = false;

            private bool _wasPlayAttackCue;
            private ISpawnedActorByAbility _spawnedActorByAbility;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DefaultBowAttackSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>();
                _bodySystem = gameObject.GetBodySystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();

                _animationID = Animator.StringToHash(_ability._animationName);

                _animationEvents = new AnimationPlayer.Events();
                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnNotify += _animationEvents_OnNotify;
                _animationEvents.OnEnterNotifyState += _animationEvents_OnEnterNotifyState;
                _animationEvents.OnExitNotifyState += _animationEvents_OnExitNotifyState;
            }


            protected override void EnterAbility()
            {
                base.EnterAbility();

                _wasStartedAnimation = false;

                _wasPlayAttackCue = !_ability._onAttackCue.Cue;

                _animationPlayer.Play(_animationID, _ability._fadeInTime);
                _animationPlayer.AnimationEvents = _animationEvents;
            }
            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

                if(_spawnedActorByAbility is not null)
                {
                    _spawnedActorByAbility.Inactivate();
                    _spawnedActorByAbility = null;
                }
            }
            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
                {
                    
                    if (!_wasPlayAttackCue)
                    {
                        float normalizedTime = _animationPlayer.NormalizedTime;

                        if(normalizedTime >= _ability._attackCueTime)
                        {
                            _wasPlayAttackCue = true;

                            _ability._onAttackCue.PlayFromTarget(transform);
                        }
                    }
                }
                
            }
            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }
            private void SpawnProjectile()
            {
                var pooledObject = _ability._projectilePool.Get();
                var bodypart = _bodySystem.GetBodyPart(_ability._projectileSpawnPoint);

                pooledObject.transform.SetPositionAndRotation(bodypart.transform.position, bodypart.transform.rotation);
                pooledObject.transform.SetParent(bodypart.transform, true);

                _spawnedActorByAbility = pooledObject.GetComponent<ISpawnedActorByAbility>();
                _spawnedActorByAbility.Activate(gameObject, this);

                pooledObject.gameObject.SetActive(true);
            }
            private void ShootProjectile()
            {
                if(_spawnedActorByAbility is not null)
                {
                    _spawnedActorByAbility.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
                    _spawnedActorByAbility.Play();

                    _spawnedActorByAbility = null;
                }
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
            private void OnCombo()
            {
                if (_ability._comboTag)
                    GameplayTagSystem.AddOwnedTag(_ability._comboTag);
            }
            private void EndCombo()
            {
                if (_ability._comboTag)
                    GameplayTagSystem.RemoveOwnedTag(_ability._comboTag);
            }
            public void OnHit(ISpawnedActorByAbility spawnedActor, RaycastHit[] hits, int hitCount)
            {
                if (hitCount == 0)
                    return;

                for(int i = 0; i < hitCount; i++)
                {
                    var hit = hits[i];

                    if (!hit.transform.TryGetActor(out IActor actor))
                        continue;

                    var hitActor = actor.gameObject;

                    if (!hitActor.TryGetReleationshipSystem(out IRelationshipSystem hitRelationshipSystem)
                        || _relationshipSystem.CheckRelationship(hitRelationshipSystem) != ERelationship.Hostile)
                        continue;

                    if (!hitActor.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                        continue;

                    if(_ability._takeDamageEffect)
                    {
                        Vector3 attackDirection = spawnedActor.transform.forward;

                        var element = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, attackDirection, spawnedActor.gameObject, gameObject);
                        hitGameplayEffectSystem.TryApplyGameplayEffect(_ability._takeDamageEffect, gameObject, Level, element);
                        element.Release();
                    }

                    if(!_ability._applyGameplayEffectsOnHitToOther.IsNullOrEmpty())
                    {
                        foreach (var gameplayEffect in _ability._applyGameplayEffectsOnHitToOther)
                        {
                            hitGameplayEffectSystem.TryApplyGameplayEffect(gameplayEffect, gameObject, Level);
                        }
                    }
                }

                spawnedActor.Inactivate();
            }

            private void _animationEvents_OnStarted()
            {
                _wasStartedAnimation = true;
            }
            private void _animationEvents_OnFailed()
            {
                CancelAbility();
            }
            private void _animationEvents_OnCanceled()
            {
                if (!_wasStartedAnimation)
                    return;

                CancelAbility();
            }
            private void _animationEvents_OnStartedBlendOut()
            {
                TryFinishAbility();
            }

            private void _animationEvents_OnNotify(string eventName)
            {
                switch (eventName)
                {
                    case "Spawn":
                        SpawnProjectile();
                        break;
                    case "Shoot":
                        ShootProjectile();
                        break;
                    default:
                        break;
                }
            }

            private void _animationEvents_OnEnterNotifyState(string eventName)
            {
                switch (eventName)
                {
                    case "Combo":
                        OnCombo();
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
                switch (eventName)
                {
                    case "Combo":
                        EndCombo();
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
