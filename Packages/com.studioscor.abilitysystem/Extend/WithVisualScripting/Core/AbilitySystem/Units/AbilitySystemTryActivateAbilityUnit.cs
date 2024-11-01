
#if SCOR_ENABLE_VISUALSCRIPTING
using Unity.VisualScripting;
using UnityEngine;

namespace StudioScor.AbilitySystem.VisualScripting
{


    [UnitTitle("Try Activate Ability")]
    [UnitSubtitle("AbilitySystem Unit")]
    public class AbilitySystemTryActivateAbilityUnit : AbilitySystemFlowUnit
    {
        [DoNotSerialize]
        [PortLabel("Ability")]
        [PortLabelHidden]
        public ValueInput Ability { get; private set; }

        [DoNotSerialize]
        [PortLabel("isActivate")]
        [PortLabelHidden]
        public ValueOutput IsActivate { get; private set; }

        [DoNotSerialize]
        [PortLabel("AbilitySpec")]
        [PortLabelHidden]
        public ValueOutput AbilitySpec { get; private set; }

        protected override void Definition()
        {
            base.Definition();

            Ability = ValueInput<Ability>(nameof(Ability), null);

            AbilitySpec = ValueOutput<IAbilitySpec>(nameof(Ability));
            IsActivate = ValueOutput<bool>(nameof(IsActivate));

            Requirement(Target, AbilitySpec);
            Requirement(Target, IsActivate);

            Requirement(Ability, Enter);
            Requirement(Ability, AbilitySpec);
            Requirement(Ability, IsActivate);
        }

        protected override ControlOutput EnterUnit(Flow flow)
        {
            var abilitySystem = flow.GetValue<IAbilitySystem>(Target);
            var ability = flow.GetValue<Ability>(Ability);

            var result = abilitySystem.TryActivateAbility(ability, out IAbilitySpec abilitySpec);

            flow.SetValue(AbilitySpec, abilitySpec);
            flow.SetValue(IsActivate, result);

            return Exit;
        }
    }
}

#endif