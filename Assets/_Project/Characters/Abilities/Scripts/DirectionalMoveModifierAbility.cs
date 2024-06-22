using UnityEngine;
using StudioScor.MovementSystem;
using StudioScor.AbilitySystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Movement/new Directional Move Modifier Ability", fileName = "GA_PawnAbiltiy_DirectionalMoveModifier")]
    public class DirectionalMoveModifierAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            private readonly IMovementSystem _movementSystem;
            private readonly DirectionalAccelerationModifier _accelerateMove;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _movementSystem = gameObject.GetMovementSystem();

                var movementModuleSystem = gameObject.GetMovementModuleSystem();

                _accelerateMove = new DirectionalAccelerationModifier(_movementSystem, movementModuleSystem);
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                _accelerateMove.EnableModifier();

                if (_movementSystem.MoveStrength > 0f)
                {
                    _accelerateMove.SetCurrentSpeed(_movementSystem.PrevSpeed);
                }
                else
                {
                    _accelerateMove.SetCurrentSpeed(0f);
                }
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _accelerateMove.DisableModifier();
            }
        }
    }
}
