using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.PlayerSystem;
using StudioScor.MovementSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Movement/new Update Movement Input Ability", fileName = "GA_PawnAbiltiy_UpdateMovementInput")]
    public class UpdateMovementInputAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec, IUpdateableAbilitySpec
        {
            private readonly IPawnSystem _pawnSystem;
            private readonly IMovementSystem _movementSystem;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _pawnSystem = gameObject.GetPawnSystem();
                _movementSystem = gameObject.GetMovementSystem();
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _movementSystem.SetMoveDirection(_pawnSystem.MoveDirection, 0f);
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void UpdateAbility(float deltaTime)
            {
                if (!IsPlaying)
                    return;

                _movementSystem.SetMoveDirection(_pawnSystem.MoveDirection, _pawnSystem.MoveStrength);
            }
        }
    }
}
