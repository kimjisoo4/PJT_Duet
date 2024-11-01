using StudioScor.MovementSystem;
using StudioScor.AbilitySystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Movement/new Stair Move Modifier Ability", fileName = "GA_PawnAbiltiy_Movement_StairMoveModifier")]
    public class StairMoveModifierAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            private readonly IMovementSystem _movementSystem;
            private readonly StairModifier _stairMove;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _movementSystem = gameObject.GetMovementSystem();

                var movementModuleSystem = gameObject.GetMovementModuleSystem();

                _stairMove = new StairModifier(_movementSystem, movementModuleSystem);

            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _stairMove.EnableModifier();
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _stairMove.DisableModifier();
            }
        }
    }
}
