using StudioScor.AbilitySystem;
using StudioScor.GameplayTagSystem;
using StudioScor.StatSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Guard Skill", fileName = "GA_Skill_Guard")]
    public class GuardSkill : CharacterSkill
    {
        [Header(" [ Guard Skill ] ")]
        [Header(" Animation")]
        [SerializeField] private string _animationName = "GuardHit";
        [SerializeField][Range(0f, 1f)] private float _fadeTime = 0.2f;
        [SerializeField][Range(0f, 1f)] private float _iffsetTime = 0f;

        [SerializeField] private FGameplayTags _guardHitGrantTags;
        [Header(" Stat ")]
        [SerializeField] private FStatModifier _moveSpeedStat;

        [Header(" Just Guard  ")]
        [SerializeField] private float _justGuardTime = 0.2f;
        [SerializeField] private CharacterSkill _justGuardSkill;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec, IUpdateableAbilitySpec
        {
            protected new readonly GuardSkill _ability;

            private readonly ICharacter _character;
            private readonly AnimationPlayer _animationPlayer;
            private readonly IStatSystem _statSystem;
            private readonly IDamageableSystem _damageableSystem;

            private readonly Timer _justGuardTimer = new();
            private readonly Stat _moveSpeed;
            private readonly StatModifier _moveSpeedModifier;

            private readonly int ANIM_GUARD_BLOCK;

            private readonly AnimationPlayer.Events _animationEvents;
            private bool _wasPlayedGuardHit;
            private IReceiveDamageSkill _receiveDamageSkill;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as GuardSkill;

                _character = gameObject.GetComponent<ICharacter>();
                _statSystem = gameObject.GetStatSystem();
                _damageableSystem = gameObject.GetDamageableSystem();
                _animationPlayer = _character.Model.GetComponent<AnimationPlayer>();

                _moveSpeed = _statSystem.GetOrCreateValue(_ability._moveSpeedStat.StatTag);
                _moveSpeedModifier = new StatModifier(_ability._moveSpeedStat.StatModifier);

                ANIM_GUARD_BLOCK = Animator.StringToHash(_ability._animationName);

                _animationEvents = new();
                _animationEvents.OnStartedBlendOut += _animationEvents_OnStartedBlendOut;
            }


            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _damageableSystem.OnTakePointDamage += _damageableSystem_OnTakePointDamage;

                if(AbilitySystem.TryGrantAbility(_ability._justGuardSkill, Level, out IAbilitySpec abilitySpec))
                {
                    _receiveDamageSkill = abilitySpec as IReceiveDamageSkill;
                }
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                _damageableSystem.OnTakePointDamage -= _damageableSystem_OnTakePointDamage;

                if (AbilitySystem.RemoveAbility(_ability._justGuardSkill))
                {
                    _receiveDamageSkill = null;
                }
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();
                
                _moveSpeed.AddModifier(_moveSpeedModifier);

                _justGuardTimer.OnTimer(_ability._justGuardTime);
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();
                
                _moveSpeed.RemoveModifier(_moveSpeedModifier);

                _justGuardTimer.EndTimer();

                if(_wasPlayedGuardHit)
                {
                    _wasPlayedGuardHit = false;

                    GameplayTagSystem.RemoveGameplayTags(_ability._guardHitGrantTags);
                }
            }

            protected override void OnReleaseAbility()
            {
                base.OnReleaseAbility();

                TryFinishAbility();
            }

            public void UpdateAbility(float deltaTime)
            {
                if (!IsPlaying)
                    return;

                _justGuardTimer.UpdateTimer(deltaTime);
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }


            private void _damageableSystem_OnTakePointDamage(IDamageableSystem damageable, DamageInfoData damageInfo)
            {
                if (!IsPlaying)
                    return;

                if(_justGuardTimer.IsPlaying && _receiveDamageSkill.TryActiveReceiveDamageSkill(damageInfo))
                {
                    _justGuardTimer.EndTimer();

                    TryFinishAbility();
                }
                else
                {
                    _animationPlayer.Play(ANIM_GUARD_BLOCK, _ability._fadeTime, _ability._iffsetTime);
                    _animationPlayer.AnimationEvents = _animationEvents;

                    if(!_wasPlayedGuardHit)
                    {
                        _wasPlayedGuardHit = true;

                        GameplayTagSystem.AddGameplayTags(_ability._guardHitGrantTags);
                    }

                }
            }


            private void _animationEvents_OnStartedBlendOut()
            {
                if(_wasPlayedGuardHit)
                {
                    _wasPlayedGuardHit = false;

                    GameplayTagSystem.RemoveGameplayTags(_ability._guardHitGrantTags);
                }
            }
        }
    }
}
