using StudioScor.AbilitySystem;
using StudioScor.MovementSystem;
using StudioScor.StatSystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Movement/new Gravity Move Modifier Ability", fileName = "GA_PawnAbiltiy_GravityMoveModifier")]
    public class GravityMoveModifierAbility : GASAbility
    {
        [Header(" [ Gravity Move Modifier Ability ] ")]
        [SerializeField] private StatTag _gravityTag;

        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            protected new readonly GravityMoveModifierAbility _ability;
            private readonly IMovementSystem _movementSystem;
            private readonly GravityModifier _gravityMove;
            private readonly Stat _gravityStat;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _ability = ability as GravityMoveModifierAbility;

                _movementSystem = gameObject.GetMovementSystem();


                var statSystem = gameObject.GetStatSystem();

                _gravityStat = statSystem.GetOrCreateValue(_ability._gravityTag);

                var movementModuleSystem = gameObject.GetMovementModuleSystem();

                _gravityMove = new GravityModifier(_movementSystem, movementModuleSystem);

            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _gravityMove.SetGravity(_gravityStat.Value);
                _gravityStat.OnChangedValue += _moveSpeedStat_OnChangedValue;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if (_gravityStat is not null)
                {
                    _gravityStat.OnChangedValue -= _moveSpeedStat_OnChangedValue;
                }
            }

            private void _moveSpeedStat_OnChangedValue(Stat stat, float currentValue, float prevValue)
            {
                _gravityMove.SetGravity(currentValue);
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _gravityMove.EnableModifier();
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _gravityMove.DisableModifier();
            }
        }
    }
}
