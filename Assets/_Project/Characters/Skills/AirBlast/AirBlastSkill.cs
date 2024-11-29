using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.BodySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System;
using System.Linq;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Air Blast Skill", fileName = "GA_Skill_AirBlast")]
    public class AirBlastSkill : CharacterSkill
    {
        [System.Serializable]
        private struct FExplosionGameplayEffect
        {
            [SerializeField] private GameplayEffect _applyTakeDamageEffect;
            [SerializeField] private GameplayEffect _applyRadialKnockbackEffect;
            [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
            public readonly GameplayEffect ApplyTakeDamageEffect => _applyTakeDamageEffect;
            public readonly GameplayEffect ApplyRadialKnockbackEffect => _applyRadialKnockbackEffect;
            public readonly GameplayEffect[] ApplyGameplayEffectsOnHitToOther => _applyGameplayEffectsOnHitToOther;
        }

        [System.Serializable]
        private struct FProjectileGameplayEffect
        {
            [SerializeField] private GameplayEffect _applyTakeDamageEffect;
            [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
            public readonly GameplayEffect ApplyTakeDamageEffect => _applyTakeDamageEffect;
            public readonly GameplayEffect[] ApplyGameplayEffectsOnHitToOther => _applyGameplayEffectsOnHitToOther;
        }


        [Header(" [ Air Blast Skill ] ")]
        [Header(" Animations ")]
        [SerializeField] private string _readyAnimationName = "AirBlast_Start";
        [SerializeField] private float _readyAnimFadeInTime = 0.2f;
        [SerializeField] private bool _readyAnimFixedTransition = true;
        [Space(5f)]
        [SerializeField] private string _chargingMotionTime = "motionTime";
        [SerializeField][Min(0f)] private float _maxChargeTime = 3f;
        [Space(5f)]
        [SerializeField] private string _shotAnimationName = "AirBlast_Shot";
        [SerializeField][Range(0f, 1f)] private float _shotAnimFadeInTime = 0.2f;
        [SerializeField] private bool _shotAnimFixedTransition = false;

        [Header(" Turn ")]
        [SerializeField] private GameplayTag _turnTag;

        [Header(" Projectile ")]
        [SerializeField] private PoolContainer _projectile;
        [SerializeField] private BodyTag _spawnPoint;
        [SerializeField] private FProjectileGameplayEffect[] _applyProjectileGameplayEffects;

        [Header(" Explosion ")]
        [SerializeField] private PoolContainer _explosionPool;
        [SerializeField] private FExplosionGameplayEffect[] _applyExplosionGameplayEffects;

        [Header(" CoolTime ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _spawnedCue;
        [SerializeField] private FGameplayCue[] _shotCues;

        public override string GetDescription()
        {
            float time = _maxChargeTime;

            var takeDamage_01 = _applyExplosionGameplayEffects[0].ApplyTakeDamageEffect as IDisplayDamage;
            var takeDamage_02 = _applyExplosionGameplayEffects[1].ApplyTakeDamageEffect as IDisplayDamage;
            var takeDamage_03 = _applyExplosionGameplayEffects[2].ApplyTakeDamageEffect as IDisplayDamage;

            return string.Format(Description, time, takeDamage_01.Damage, takeDamage_02.Damage, takeDamage_03.Damage);
        }

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec, ISkillState, ITakeDamageAbility
        {
            public abstract class AirBlastState : BaseStateClass
            {
                private readonly Spec _spec;

                protected Spec Spec => _spec;
                protected AirBlastSkill Ability => _spec._ability;
                protected AnimationPlayer AnimationPlayer => _spec._animationPlayer;
                protected FiniteStateMachineSystemWithKey<EState, AirBlastState> StateMachine => _spec._stateMachine;

                public override bool UseDebug => _spec.UseDebug;
                public override UnityEngine.Object Context => _spec.Context;

                public AirBlastState(Spec spec)
                {
                    _spec = spec;
                }

                public virtual void UpdateState(float deltaTime)
                {

                }
            }
            public class ReadyState : AirBlastState
            {
                private readonly int _animationHash;
                private readonly AnimationPlayer.Events _animationEvents;
                private bool _wasStartedAnimation = false;

                public ReadyState(Spec spec) : base(spec)
                {
                    _animationHash = Animator.StringToHash(Ability._readyAnimationName);

                    _animationEvents = new();
                    _animationEvents.OnStarted += _animationEvents_OnStarted;
                    _animationEvents.OnFailed += _animationEvents_OnFailed;
                    _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                    _animationEvents.OnFinished += _animationEvents_OnFinished;
                }
                protected override void EnterState()
                {
                    _wasStartedAnimation = false;

                    AnimationPlayer.Play(_animationHash, Ability._readyAnimFadeInTime, fixedTransition: Ability._readyAnimFixedTransition);
                    AnimationPlayer.AnimationEvents = _animationEvents;

                    Spec._turnToggle.OnToggle();
                }

                protected override void ExitState()
                {
                    Spec._turnToggle.OffToggle();
                }
                private void _animationEvents_OnStarted()
                {
                    _wasStartedAnimation = true;
                }

                private void _animationEvents_OnFailed()
                {
                    if (!IsActivate)
                        return;

                    Spec.CancelAbility();
                }

                private void _animationEvents_OnCanceled()
                {
                    if (!IsActivate)
                        return;

                    if (!_wasStartedAnimation)
                        return;

                    Spec.CancelAbility();
                }

                private void _animationEvents_OnFinished()
                {
                    if (!IsActivate)
                        return;

                    Spec.OnSpawnAirBlast();

                    if (!StateMachine.TrySetState(EState.Charge))
                    {
                        StateMachine.TrySetState(EState.Shoot);
                    }
                }
            }
            public class ChargeState : AirBlastState
            {
                private readonly ITimer _timer = new Timer();
                private readonly int _motionTimeHash;

                public ChargeState(Spec spec) : base(spec)
                {
                    _motionTimeHash = Animator.StringToHash(Ability._chargingMotionTime);
                }

                public override bool CanEnterState()
                {
                    if (!base.CanEnterState())
                        return false;

                    if (Spec._wasReleased)
                        return false;

                    return true;
                }
                protected override void EnterState()
                {
                    Spec._turnToggle.OnToggle();
                    Spec._chargeable.OnCharging();

                    _timer.OnTimer(Ability._maxChargeTime);

                    AnimationPlayer.Animator.SetFloat(_motionTimeHash, 0f);
                }

                public override void UpdateState(float deltaTime)
                {
                    if (StateMachine.TrySetState(EState.Shoot))
                        return;

                    if (!_timer.IsPlaying)
                        return;

                    _timer.UpdateTimer(deltaTime);

                    float normalizedTime = _timer.NormalizedTime;

                    Spec._animationPlayer.Animator.SetFloat(_motionTimeHash, normalizedTime);
                    Spec._chargeable.SetStrength(normalizedTime);
                }

                protected override void ExitState()
                {
                    Spec._turnToggle.OffToggle();
                    _timer.EndTimer();

                    if (Spec._chargeable is not null)
                        Spec._chargeable.FinishCharging();
                }
            }

            public class ShootState : AirBlastState
            {
                private readonly int _animationHash;
                private readonly AnimationPlayer.Events _animationEvents;
                private bool _wasStartedAnimation = false;
                public ShootState(Spec spec) : base(spec)
                {
                    _animationHash = Animator.StringToHash(Ability._shotAnimationName);

                    _animationEvents = new();
                    _animationEvents.OnStarted += _animationEvents_OnStarted;
                    _animationEvents.OnFailed += _animationEvents_OnFailed;
                    _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                    _animationEvents.OnFinished += _animationEvents_OnFinished;
                    _animationEvents.OnNotify += _animationEvents_OnNotify;
                }

                

                public override bool CanEnterState()
                {
                    if (!base.CanEnterState())
                        return false;

                    if (!Spec._wasReleased)
                        return false;

                    return true;
                }
                protected override void EnterState()
                {
                    _wasStartedAnimation = false;
                    AnimationPlayer.Play(_animationHash, Ability._shotAnimFadeInTime, fixedTransition: Ability._shotAnimFixedTransition);
                    AnimationPlayer.AnimationEvents = _animationEvents;
                }
                private void _animationEvents_OnStarted()
                {
                    _wasStartedAnimation = true;
                }
                private void _animationEvents_OnFailed()
                {
                    Spec.CancelAbility();
                }
                private void _animationEvents_OnCanceled()
                {
                    if (_wasStartedAnimation)
                        return;

                    Spec.CancelAbility();
                }
                private void _animationEvents_OnFinished()
                {
                    Spec.TryFinishAbility();
                }

                private void _animationEvents_OnNotify(string eventName)
                {
                    switch (eventName)
                    {
                        case "Shoot":
                            Spec.OnShootAirBlast();
                            break;
                        default:
                            break;
                    }
                }
            }

            public enum EState
            {
                None,
                Ready,
                Charge,
                Shoot,
            }

            protected new readonly AirBlastSkill _ability;

            private readonly IBodySystem _bodySystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly IRelationshipSystem _relationshipSystem;
            private readonly AnimationPlayer _animationPlayer;

            private readonly FiniteStateMachineSystemWithKey<EState, AirBlastState> _stateMachine;
            private readonly ReadyState _readyState;
            private readonly ChargeState _chargeState;
            private readonly ShootState _shootState;

            private readonly Toggleable _turnToggle;

            private bool _wasReleased = false;
            private IBodyPart _bodypart;
            private ISpawnedActorByAbility _spawnedActor;
            private IChargeable _chargeable;

            private CoolTimeEffect.Spec _coolTimeEffectSpec;

            private bool InCoolTime => _coolTimeEffectSpec is not null && _coolTimeEffectSpec.IsActivate;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0;
            public float RemainCoolTime => InCoolTime ? _coolTimeEffectSpec.RemainTime : 0f;
            public float NormalizedCoolTime => InCoolTime ? _coolTimeEffectSpec.NormalizedTime : 1f;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as AirBlastSkill;

                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
                _bodySystem = gameObject.GetBodySystem();
                _relationshipSystem = gameObject.GetRelationshipSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _turnToggle = new();
                _turnToggle.OnChangedToggleState += _turnToggle_OnChangedToggleState;

                _readyState = new(this);
                _chargeState = new(this);
                _shootState = new(this);

                _stateMachine = new(EState.Ready, _readyState);
                _stateMachine.AddState(EState.Charge, _chargeState);
                _stateMachine.AddState(EState.Shoot, _shootState);

                _ability._projectile.Initialization();
                _ability._explosionPool.Initialization();

                _ability._spawnedCue.Initialization();

                foreach (var shotCue in _ability._shotCues)
                {
                    shotCue.Initialization();
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

                _wasReleased = false;

                _stateMachine.Start();
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                _stateMachine.End();

                _spawnedActor = null;
                _chargeable = null;

                if (_ability._coolTimeEffect)
                {
                    if(_gameplayEffectSystem.TryApplyGameplayEffect(_ability._coolTimeEffect, gameObject, Level, null, out var spec))
                    {
                        _coolTimeEffectSpec = spec as CoolTimeEffect.Spec;
                        _coolTimeEffectSpec.OnEndedEffect += _coolTimeEffectSpec_OnEndedEffect;
                    }
                }
            }
            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

                if (_chargeable is not null)
                {
                    _chargeable.CancelCharging();
                    _chargeable = null;
                }

                if (_spawnedActor is not null)
                {
                    _spawnedActor.Inactivate();
                    _spawnedActor = null;
                }
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
                    if(_stateMachine.IsPlaying)
                    {
                        _stateMachine.CurrentState.UpdateState(deltaTime);
                    }
                }
            }
            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void OnHit(ISpawnedActorByAbility spawnedActor, RaycastHit[] hits, int hitCount)
            {
                if (hitCount == 0)
                    return;

                bool isProjectile = spawnedActor.gameObject.TryGetComponent(out IProjectile projectile);
                Log($"{nameof(OnHit)} -{(isProjectile ? "Projectile" : "Explosion")}");

                int chargeLevel = 0;
                int collisionCount = hitCount;


                if(spawnedActor.gameObject.TryGetComponent(out IChargeable chargeable))
                {
                    chargeLevel = chargeable.CurrentChargeLevel;
                }

                for (int i = 0; i < hitCount; i++)
                {
                    var hit = hits[i];

                    if (!hit.transform.TryGetActor(out IActor actor))
                        continue;

                    var hitActor = actor.gameObject;

                    if (!hitActor.TryGetReleationshipSystem(out IRelationshipSystem hitRelationshipSystem))
                        continue;

                    var relationship = _relationshipSystem.CheckRelationship(hitRelationshipSystem);

                    switch (relationship)
                    {
                        case ERelationship.Hostile:
                            break;
                        case ERelationship.Friendly:
                            collisionCount--;
                            continue;
                        default:
                            continue;
                    }

                    if (!hitActor.TryGetGameplayEffectSystem(out IGameplayEffectSystem hitGameplayEffectSystem))
                        continue;

                    if(isProjectile)
                    {
                        int effectIndex = Mathf.Min(chargeLevel, _ability._applyProjectileGameplayEffects.Length - 1);
                        var projectileEffect = _ability._applyProjectileGameplayEffects.ElementAtOrDefault(effectIndex);

                        if (projectileEffect.ApplyTakeDamageEffect)
                        {
                            Vector3 direction = projectile.Velocity.normalized;

                            var element = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, direction, spawnedActor.gameObject, gameObject);
                            hitGameplayEffectSystem.TryApplyGameplayEffect(projectileEffect.ApplyTakeDamageEffect, gameObject, Level, element);
                            element.Release();
                        }

                        if(!projectileEffect.ApplyGameplayEffectsOnHitToOther.IsNullOrEmpty())
                        {
                            foreach (var gameplayEffect in projectileEffect.ApplyGameplayEffectsOnHitToOther)
                            {
                                hitGameplayEffectSystem.TryApplyGameplayEffect(gameplayEffect, gameObject, Level);
                            }
                        }
                    }
                    else
                    {
                        int effectIndex = Mathf.Min(chargeLevel, _ability._applyExplosionGameplayEffects.Length - 1);
                        var explosionEffect = _ability._applyExplosionGameplayEffects.ElementAtOrDefault(effectIndex);

                        if (explosionEffect.ApplyTakeDamageEffect)
                        {
                            var element = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, spawnedActor.transform.forward, spawnedActor.gameObject, gameObject);
                            hitGameplayEffectSystem.TryApplyGameplayEffect(explosionEffect.ApplyTakeDamageEffect, gameObject, Level, element);
                            element.Release();
                        }

                        if(explosionEffect.ApplyRadialKnockbackEffect)
                        {
                            var element = TakeRadialKnockbackEffect.Element.Get(spawnedActor.transform.position);
                            hitGameplayEffectSystem.TryApplyGameplayEffect(explosionEffect.ApplyRadialKnockbackEffect, gameObject, Level, element);
                            element.Release();
                        }

                        if (!explosionEffect.ApplyGameplayEffectsOnHitToOther.IsNullOrEmpty())
                        {
                            foreach (var gameplayEffect in explosionEffect.ApplyGameplayEffectsOnHitToOther)
                            {
                                hitGameplayEffectSystem.TryApplyGameplayEffect(gameplayEffect, gameObject, Level);
                            }
                        }
                    }
                }

                if (isProjectile && collisionCount > 0)
                {
                    OnSpawnExplosion(spawnedActor.transform.position, spawnedActor.transform.rotation, chargeLevel);
                    spawnedActor.Inactivate();
                }
            } 

            private void OnSpawnAirBlast()
            {
                Log($"{nameof(OnSpawnAirBlast)}");

                _bodypart = _bodySystem.GetBodyPart(_ability._spawnPoint);

                var actor = _ability._projectile.Get();
                actor.transform.SetPositionAndRotation(_bodypart.transform.position, _bodypart.transform.rotation);
                actor.transform.SetParent(_bodypart.transform, true);

                actor.gameObject.SetActive(true);

                _spawnedActor = actor.GetComponent<ISpawnedActorByAbility>();
                _spawnedActor.Activate(gameObject, this);

                _chargeable = actor.GetComponent<IChargeable>();
                _chargeable.OnCharging();

                if (_ability._spawnedCue.Cue)
                {
                    _ability._spawnedCue.PlayFromTarget(_bodypart.transform);
                }
            }

            private void OnShootAirBlast()
            {
                Log($"{nameof(OnShootAirBlast)}");

                _chargeable.FinishCharging();

                int chargedLevel = _chargeable.CurrentChargeLevel;

                FGameplayCue shotCue;

                if (chargedLevel > _ability._shotCues.LastIndex())
                {
                    shotCue = _ability._shotCues.Last();
                }
                else
                {
                    shotCue = _ability._shotCues.ElementAt(chargedLevel);
                }

                if (shotCue.Cue)
                {
                    _bodypart = _bodySystem.GetBodyPart(_ability._spawnPoint);

                    shotCue.PlayFromTarget(_bodypart.transform);
                }

                _spawnedActor.Play();

                _spawnedActor = null;
                _chargeable = null;
            }

            private void OnSpawnExplosion(Vector3 position, Quaternion rotation, int level)
            {
                var pooledActor = _ability._explosionPool.Get();

                pooledActor.transform.SetPositionAndRotation(position, rotation);
                pooledActor.gameObject.SetActive(true);

                if (pooledActor.gameObject.TryGetComponent(out IChargeable chargeable))
                {
                    chargeable.SetChargeLevel(level);
                }

                var spawnedActor = pooledActor.gameObject.GetComponent<ISpawnedActorByAbility>();
                spawnedActor.Activate(gameObject, this);

                spawnedActor.Play();
            }


            private void _coolTimeEffectSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeEffectSpec_OnEndedEffect;

                _coolTimeEffectSpec = null;
            }
            private void _turnToggle_OnChangedToggleState(Toggleable toggleable, bool isOn)
            {
                Log($"{toggleable} - {(isOn ? "On" : "Off")}");

                if (isOn)
                {
                    if (_ability._turnTag)
                        GameplayTagSystem.AddOwnedTag(_ability._turnTag);
                }
                else
                {
                    if (_ability._turnTag)
                        GameplayTagSystem.RemoveOwnedTag(_ability._turnTag);
                }
            }
        }
    }
}
