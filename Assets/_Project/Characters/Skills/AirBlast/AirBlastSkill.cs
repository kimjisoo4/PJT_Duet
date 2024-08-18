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
        [SerializeField] private SimplePoolContainer _projectile;
        [SerializeField] private BodyTag _spawnPoint;

        [Header(" Gameplay Effects ")]
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther_01;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther_02;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther_03;

        [Header(" CoolTime ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _spawnedCue;
        [SerializeField] private FGameplayCue[] _shotCues;

        public override string GetDescription()
        {
            float time = _maxChargeTime;

            var takeDamage_01 = _applyGameplayEffectsOnHitToOther_01.First(x => x is TakeDamageEffect) as TakeDamageEffect;
            var takeDamage_02 = _applyGameplayEffectsOnHitToOther_02.First(x => x is TakeDamageEffect) as TakeDamageEffect;
            var takeDamage_03 = _applyGameplayEffectsOnHitToOther_03.First(x => x is TakeDamageEffect) as TakeDamageEffect;

            float damage_01 = takeDamage_01.DamageRatio * 100;
            float damage_02 = takeDamage_02.DamageRatio * 100;
            float damage_03 = takeDamage_03.DamageRatio * 100;

            return string.Format(Description, time, damage_01, damage_02, damage_03);
        }

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec, ISkillState
        {
            public enum EState
            {
                None,
                Ready,
                Charge,
                Shot,
                Finish,
            }

            protected new readonly AirBlastSkill _ability;

            private readonly IBodySystem _bodySystem;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly AnimationPlayer _animationPlayer;

            private readonly int _readyAnimationHash;
            private readonly int _shotAnimationHash;
            private readonly int _chargeMotionTimeHash;

            private readonly Timer _chargingTimer = new();

            private EState _abilityState;
            private IBodyPart _bodypart;
            private bool _wasReleased = false;
            private bool _wasGrantedTurnTag;
            private readonly AnimationPlayer.Events _animationEvents;


            private ISpawnedActorToAbility _spawnedActor;
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
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();

                _readyAnimationHash = Animator.StringToHash(_ability._readyAnimationName);
                _chargeMotionTimeHash = Animator.StringToHash(_ability._chargingMotionTime);
                _shotAnimationHash = Animator.StringToHash(_ability._shotAnimationName);

                _animationEvents = new();
                _animationEvents.OnNotify += _animationPlayer_OnNotify;
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

                TransitionState(EState.Ready);
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                TransitionState(EState.None);
                EndTurn();

                _chargeable = null;
                _spawnedActor = null;

                if (_ability._coolTimeEffect)
                {
                    var takeEffect = _gameplayEffectSystem.TryTakeEffect(_ability._coolTimeEffect, gameObject, Level);

                    if (takeEffect.isActivate)
                    {
                        _coolTimeEffectSpec = takeEffect.effectSpec as CoolTimeEffect.Spec;
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
                }

                if (_spawnedActor is not null)
                {
                    var bodyPart = _bodySystem.GetBodyPart(_ability._spawnPoint);

                    if (_spawnedActor.transform.parent == bodyPart.transform)
                    {
                        _spawnedActor.gameObject.SetActive(false);
                    }
                }
            }

            private void _coolTimeEffectSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeEffectSpec_OnEndedEffect;

                _coolTimeEffectSpec = null;
            }

            protected override void OnReleaseAbility()
            {
                base.OnReleaseAbility();

                if (_abilityState == EState.Charge)
                {
                    TransitionState(EState.Shot);
                }
                else
                {
                    _wasReleased = true;
                }
            }

            public void UpdateAbility(float deltaTime)
            {
                if (IsPlaying)
                {
                    OnUpdateAbility(deltaTime);
                }
                else
                {

                }
            }
            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }


            private void OnUpdateAbility(float deltaTime)
            {
                switch (_abilityState)
                {
                    case EState.None:
                        break;
                    case EState.Ready:
                        UpdateInReady(deltaTime);
                        break;
                    case EState.Charge:
                        UpdateInCharge(deltaTime);
                        break;
                    case EState.Shot:
                        UpdateInShot(deltaTime);
                        break;
                    case EState.Finish:
                        break;
                    default:
                        break;
                }
            }

            private void TransitionState(EState newState)
            {
                switch (_abilityState)
                {
                    case EState.None:
                        break;
                    case EState.Ready:
                        EndReady();
                        break;
                    case EState.Charge:
                        EndCharge();
                        break;
                    case EState.Shot:
                        EndShot();
                        break;
                    case EState.Finish:
                        break;
                    default:
                        break;
                }

                _abilityState = newState;

                switch (_abilityState)
                {
                    case EState.None:
                        break;
                    case EState.Ready:
                        OnReady();
                        break;
                    case EState.Charge:
                        OnCharge();
                        break;
                    case EState.Shot:
                        OnShot();
                        break;
                    case EState.Finish:
                        break;
                    default:
                        break;
                }
            }

            private void OnTurn()
            {
                if (_wasGrantedTurnTag)
                    return;

                _wasGrantedTurnTag = true;

                if (_ability._turnTag)
                    GameplayTagSystem.AddOwnedTag(_ability._turnTag);
            }
            private void EndTurn()
            {
                if (!_wasGrantedTurnTag)
                    return;

                _wasGrantedTurnTag = false;

                if (_ability._turnTag)
                    GameplayTagSystem.RemoveOwnedTag(_ability._turnTag);
            }

            private void OnReady()
            {
                _animationPlayer.Play(_readyAnimationHash, _ability._readyAnimFadeInTime, fixedTransition: _ability._readyAnimFixedTransition);
                _animationPlayer.AnimationEvents = _animationEvents;

                OnTurn();
            }

            private void OnSpawnAirBlast()
            {
                var actor = _ability._projectile.Get();

                _spawnedActor = actor.GetComponent<ISpawnedActorToAbility>();

                _spawnedActor.SetOwner(gameObject, this);

                _bodypart = _bodySystem.GetBodyPart(_ability._spawnPoint);

                _spawnedActor.transform.SetParent(_bodypart.transform, false);
                _spawnedActor.transform.SetLocalPositionAndRotation(default, default);
                _spawnedActor.gameObject.SetActive(true);

                _chargeable = _spawnedActor.gameObject.GetComponent<IChargeable>();
                _chargeable.OnCharging(0f);

                if (_ability._spawnedCue.Cue)
                {
                    _ability._spawnedCue.PlayFromTarget(_bodypart.transform);
                }
            }

            private void UpdateInReady(float deltaTime)
            {
                switch (_animationPlayer.State)
                {
                    case EAnimationState.Failed:
                    case EAnimationState.Canceled:
                        CancelAbility();
                        return;
                    case EAnimationState.BlendOut:
                    case EAnimationState.Finish:
                        OnSpawnAirBlast();

                        if (_wasReleased)
                        {
                            TransitionState(EState.Shot);
                        }
                        else
                        {
                            TransitionState(EState.Charge);
                        }
                        
                        return;
                    default:
                        break;
                }
            }
            private void EndReady()
            {
            }


            private void OnCharge()
            {
                if(_wasReleased)
                {
                    TransitionState(EState.Shot);
                }
                else
                {
                    OnTurn();
                    _chargingTimer.OnTimer(_ability._maxChargeTime);
                    _animationPlayer.Animator.SetFloat(_chargeMotionTimeHash, 0f);
                }
            }
            private void UpdateInCharge(float deltaTime)
            {
                _chargingTimer.UpdateTimer(deltaTime);

                float normalizedTime = _chargingTimer.NormalizedTime;

                _animationPlayer.Animator.SetFloat(_chargeMotionTimeHash, normalizedTime);
                _chargeable.SetStrength(normalizedTime);
            }
            private void EndCharge()
            {
                _chargingTimer.EndTimer();
                EndTurn();
             
                if(_chargeable is not null)
                    _chargeable.FinishCharging();
            }


            private void OnShot()
            {
                _animationPlayer.Play(_shotAnimationHash, _ability._shotAnimFadeInTime, fixedTransition: _ability._shotAnimFixedTransition);
                _animationPlayer.AnimationEvents = _animationEvents;
            }

            private void OnShotAirBlast()
            {
                GameplayEffect[] applyGameplayEffects;

                switch (_chargeable.CurrentChargeLevel)
                {
                    case 0:
                        applyGameplayEffects = _ability._applyGameplayEffectsOnHitToOther_01;
                        break;
                    case 1:
                        applyGameplayEffects = _ability._applyGameplayEffectsOnHitToOther_02;
                        break;
                    default:
                        applyGameplayEffects = _ability._applyGameplayEffectsOnHitToOther_03;
                        break;
                }

                _spawnedActor.SetApplyGameplayEffectToOther(applyGameplayEffects);
                _spawnedActor.transform.SetParent(null, true);

                FGameplayCue shotCue;
                
                if(_chargeable is not null)
                {
                    if (_chargeable.CurrentChargeLevel > _ability._shotCues.LastIndex())
                        shotCue = _ability._shotCues.Last();
                    else
                        shotCue = _ability._shotCues.ElementAt(_chargeable.CurrentChargeLevel);
                }
                else
                {
                    shotCue = _ability._shotCues.FirstOrDefault();
                }

                if (shotCue.Cue)
                {
                    _bodypart = _bodySystem.GetBodyPart(_ability._spawnPoint);

                    shotCue.PlayFromTarget(_bodypart.transform);
                }

                _chargeable = null;

                var projectile = _spawnedActor.gameObject.GetComponent<IProjectileActor>();
                projectile.OnProjectile();
            }

            private void UpdateInShot(float deltaTime)
            {
                switch (_animationPlayer.State)
                {
                    case EAnimationState.Failed:
                    case EAnimationState.Canceled:
                        CancelAbility();
                        return;
                    case EAnimationState.BlendOut:
                    case EAnimationState.Finish:
                        TryFinishAbility();
                        return;
                    default:
                        break;
                }
            }
            private void EndShot()
            {

            }


            

            private void _animationPlayer_OnNotify(string eventName)
            {
                if (!IsPlaying)
                    return;

                switch (eventName)
                {
                    case "Spawn":
                        OnSpawnAirBlast();
                        break;

                    case "Shot":
                        OnShotAirBlast();
                        break;

                    default:
                        break;
                }
            }

        }
    }
}
