using PF.PJT.Duet.Pawn.Effect;
using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/Effect/new Create Super Armor Shield Skill", fileName = "GA_Skill_CreateSuperArmorShield")]
    public class CreateSuperArmorShieldSkill : CharacterSkill
    {
        [Header(" [ Create Super Armor Shield Skill ]")]
        [SerializeField] private string _animationName = "BattleCry";
        [SerializeField][Range(0f, 1f)] private float _blendInTime = 0.2f;

        [Header(" Status ")]
        [SerializeField] private StatusTag _shieldStatus;

        [Header(" Gameplay Tags ")]
        [SerializeField] private GameplayTag _triggerRestoreShield;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, ISkillState
        {
            private new readonly CreateSuperArmorShieldSkill _ability;
            private readonly IStatusSystem _statusSystem;

            private readonly ICharacter _character;
            private readonly IGameplayEffectSystem _gameplayEffectSystem;
            private readonly AnimationPlayer _animationPlayer;
            private readonly AnimationPlayer.Events _animationEvents = new();
            private readonly int _animationHash;

            private readonly Status _shieldStatus;
            
            private bool _wasPlayedAnim;
            private bool _skipAnimation;

            public bool SkipAnimation
            {
                get
                {
                    return _skipAnimation;
                }
                set
                {
                    Log($"{(_skipAnimation ? "Skip Animation" : "Play Animation")}");

                    _skipAnimation = value;
                }
            }


            private CoolTimeEffect.Spec _coolTimeSpec;
            public float CoolTime => _ability._coolTimeEffect ? _ability._coolTimeEffect.Duration : 0f;
            public float RemainCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 0f : _coolTimeSpec.RemainTime;
            public float NormalizedCoolTime => _coolTimeSpec is null || !_coolTimeSpec.IsActivate ? 1f : _coolTimeSpec.NormalizedTime;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as CreateSuperArmorShieldSkill;

                _character = gameObject.GetComponent<ICharacter>();
                _statusSystem = gameObject.GetStatusSystem();
                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
                _animationPlayer = _character.Model.GetComponent<AnimationPlayer>();

                _animationHash = Animator.StringToHash(_ability._animationName);
                _animationEvents.OnStarted += _animationEvents_OnStarted;
                _animationEvents.OnFailed += _animationEvents_OnFailed;
                _animationEvents.OnCanceled += _animationEvents_OnCanceled;
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;

                _shieldStatus = _statusSystem.GetOrCreateStatus(_ability._shieldStatus);
            }

            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if(_shieldStatus is null)
                {
                    _shieldStatus.OnChangedState -= _shieldStatus_OnChangedState;
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

                if (SkipAnimation)
                {
                    _shieldStatus.SetCurrentValue(_shieldStatus.MaxValue);

                    TryFinishAbility();
                }
                else
                {
                    _wasPlayedAnim = false;
                    _animationPlayer.Play(_animationHash, _ability._blendInTime);
                    _animationPlayer.AnimationEvents = _animationEvents;

                    GameplayTagSystem.OnTriggeredTag += GameplayTagSystem_OnTriggeredTag;
                }

                _shieldStatus.OnChangedState += _shieldStatus_OnChangedState;
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                GameplayTagSystem.OnTriggeredTag -= GameplayTagSystem_OnTriggeredTag;
            }

            private void _shieldStatus_OnChangedState(Status status, EStatusState currentState, EStatusState prevState)
            {
                if(currentState == EStatusState.Emptied)
                {
                    _shieldStatus.OnChangedState -= _shieldStatus_OnChangedState;

                    if (_ability._coolTimeEffect)
                    {
                        if (_gameplayEffectSystem.TryApplyGameplayEffect(_ability._coolTimeEffect, gameObject, Level, null, out var spec))
                        {
                            _coolTimeSpec = spec as CoolTimeEffect.Spec;
                            _coolTimeSpec.OnEndedEffect += _coolTimeSpec_OnEndedEffect;
                        }
                    }
                }
            }

            private void _coolTimeSpec_OnEndedEffect(IGameplayEffectSpec effectSpec)
            {
                effectSpec.OnEndedEffect -= _coolTimeSpec_OnEndedEffect;

                _coolTimeSpec = null;
            }

            private void GameplayTagSystem_OnTriggeredTag(IGameplayTagSystem gameplayTagSystem, GameplayTag gameplayTag, object data = null)
            {
                if (!IsPlaying)
                    return;

                if (_ability._triggerRestoreShield == gameplayTag)
                {
                    _shieldStatus.SetCurrentValue(_shieldStatus.MaxValue);
                }
            }

            private void _animationEvents_OnStarted()
            {
                _wasPlayedAnim = true;
            }

            private void _animationEvents_OnCanceled()
            {
                if(_wasPlayedAnim)
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
        }
    }
}
