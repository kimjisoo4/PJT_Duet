using StudioScor.AbilitySystem;
using StudioScor.StatSystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    [CreateAssetMenu(menuName = "Project/Duet/Reword Ability/new Attack Point Modifier Reword", fileName = "GA_Reword_AttackPointModifier")]
    public class AttackPointModifierReword : RewordAbility
    {
        [Header(" [ Simple Stat Modifier Reword ] ")]
        [SerializeField] private StatTag _statTag;
        [SerializeField] private EStatModifierType _modifierType;
        [SerializeField] private float _value;
        [SerializeField] private int _order;
        public override string Description
        {
            get
            {
                var stat = _statTag as IDisplayName;
                float value = _modifierType == EStatModifierType.Add ? _value : _value * 100f;

                return string.Format(base.Description, stat.Name, $"<b>{value:F0}</b>");
            }
        }

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec
        {
            protected new readonly AttackPointModifierReword _ability;
            private readonly IStatSystem _statSystem;

            private Stat _stat;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as AttackPointModifierReword;

                _statSystem = gameObject.GetStatSystem();
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                ForceActiveAbility();
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _stat = _statSystem.GetOrCreateValue(_ability._statTag);
                var modifier = new StatModifier(_ability._value, _ability._modifierType, _ability._order, this);

                _stat.AddModifier(modifier);
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _stat.RemoveAllModifiersFromSource(this);
                _stat = null;
            }
        }
    }
}
