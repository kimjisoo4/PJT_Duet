using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.GameplayEffectSystem;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Punch Combo Skill", fileName = "GA_Skill_PunchCombo")]
    public class PunchComboSkill : GASAbility, ISkill
    {
        [Header(" [ Punch Combo Skill ] ")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private ESkillType _skillType;

        [Header(" Actions ")]
        [SerializeField] private Ability[] _groundActions;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;

        public Sprite Icon => _icon;
        public ESkillType SkillType => _skillType;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec
        {
            protected new readonly PunchComboSkill _ability;

            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private int _comboCount = 0;

            private IAbilitySpec[] _groundSpecs;

            private IAbilitySpec[] _playedSpecs;

            private IAbilitySpec PrevPlayedSpec => _playedSpecs.ElementAtOrDefault(_comboCount - 1);
            private IAbilitySpec CurrentPlayedSpec => _playedSpecs.ElementAtOrDefault(_comboCount);
            private IAbilitySpec NextPlayedSpec => _playedSpecs.ElementAtOrDefault(_comboCount + 1);


            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as PunchComboSkill;

                _gameplayEffectSystem = gameObject.GetGameplayEffectSystem();
            }
            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                int length = _ability._groundActions.Length;
                _groundSpecs = new IAbilitySpec[length];

                for (int i = 0; i < length; i++)
                {
                    var action = _ability._groundActions[i];

                    var grantAbility = _abilitySystem.TryGrantAbility(action, Level);

                    if(grantAbility.isGrant)
                    {
                        _groundSpecs[i] = grantAbility.abilitySpec;

                        grantAbility.abilitySpec.OnCanceledAbility += AbilitySpec_OnCanceledAbility;
                        grantAbility.abilitySpec.OnFinishedAbility += AbilitySpec_OnFinishedAbility;
                    }
                }
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                int length = _groundSpecs.Length;

                for (int i = 0; i < length; i++)
                {
                    var action = _groundSpecs[i];

                    if (_abilitySystem.TryRemoveAbility(action.Ability))
                    {
                        action.OnCanceledAbility -= AbilitySpec_OnCanceledAbility;
                        action.OnFinishedAbility -= AbilitySpec_OnFinishedAbility;
                    }
                }

                _groundSpecs = null;
            }

            public override bool CanActiveAbility()
            {
                if (_ability._coolTimeEffect && _gameplayEffectSystem.HasEffect(_ability._coolTimeEffect))
                    return false;

                if (!base.CanActiveAbility())
                    return false;

                if (_groundSpecs is not null && _groundSpecs.Length > 0 && _groundSpecs[0].CanActiveAbility())
                {
                    _playedSpecs = _groundSpecs;
                }
                else
                {
                    return false;
                }

                return true;
            }

            public override bool CanReTriggerAbility()
            {
                if (!base.CanReTriggerAbility())
                    return false;

                if (NextPlayedSpec is null || !NextPlayedSpec.CanActiveAbility())
                    return false;

                return true;
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _comboCount = 0;
                CurrentPlayedSpec.ForceActiveAbility();
            }
            protected override void OnReTriggerAbility()
            {
                base.OnReTriggerAbility();

                _comboCount++;

                PrevPlayedSpec.CancelAbility();
                CurrentPlayedSpec.ForceActiveAbility();
            }

            protected override void OnCancelAbility()
            {
                base.OnCancelAbility();

                CurrentPlayedSpec.CancelAbility();
            }

            private void AbilitySpec_OnFinishedAbility(IAbilitySpec abilitySpec)
            {
                if (CurrentPlayedSpec != abilitySpec)
                    return;

                TryFinishAbility();
            }

            private void AbilitySpec_OnCanceledAbility(IAbilitySpec abilitySpec)
            {
                if (CurrentPlayedSpec != abilitySpec)
                    return;

                CancelAbility();
            }
        }
    }
}
