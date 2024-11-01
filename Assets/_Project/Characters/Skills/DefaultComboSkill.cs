using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.GameplayEffectSystem;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnSkill/new Default Combo Skill", fileName = "GA_Skill_", order = -1000000)]
    public class DefaultComboSkill : CharacterSkill
    {
        [Header(" [ Punch Combo Skill ] ")]
        [Header(" Actions ")]
        [SerializeField] private Ability[] _groundActions;

        [Header(" Gameplay Effects ")]
        [SerializeField] private CoolTimeEffect _coolTimeEffect;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec
        {
            protected new readonly DefaultComboSkill _ability;

            private readonly IGameplayEffectSystem _gameplayEffectSystem;

            private int _comboCount = 0;

            private IAbilitySpec[] _groundSpecs;

            private IAbilitySpec[] _playedSpecs;

            private IAbilitySpec PrevPlayedSpec => _playedSpecs.ElementAtOrDefault(_comboCount - 1);
            private IAbilitySpec CurrentPlayedSpec => _playedSpecs.ElementAtOrDefault(_comboCount);
            private IAbilitySpec NextPlayedSpec => _playedSpecs.ElementAtOrDefault(_comboCount + 1);


            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as DefaultComboSkill;

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

                    if (_abilitySystem.TryGrantAbility(action, Level, out IAbilitySpec abilitySpec))
                    {
                        _groundSpecs[i] = abilitySpec;

                        abilitySpec.OnCanceledAbility += AbilitySpec_OnCanceledAbility;
                        abilitySpec.OnFinishedAbility += AbilitySpec_OnFinishedAbility;
                    }
                }
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                int length = _groundSpecs.Length;

                for (int i = 0; i < length; i++)
                {
                    var abilitySpec = _groundSpecs[i];

                    if (_abilitySystem.RemoveAbility(abilitySpec.Ability))
                    {
                        abilitySpec.OnCanceledAbility -= AbilitySpec_OnCanceledAbility;
                        abilitySpec.OnFinishedAbility -= AbilitySpec_OnFinishedAbility;
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
