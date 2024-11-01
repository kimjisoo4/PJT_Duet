using StudioScor.AbilitySystem;
using StudioScor.MovementSystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/State/new Check In Air Ability", fileName = "GA_PawnAbiltiy_CheckInAir")]
    public class CheckInAirAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            private readonly IMovementSystem _movementSystem;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _movementSystem = gameObject.GetMovementSystem();
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _movementSystem.OnJumped += _movementSystem_OnJumped;
                _movementSystem.OnLanded += _movementSystem_OnLanded;
            }

            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if (_movementSystem is not null)
                {
                    _movementSystem.OnJumped -= _movementSystem_OnJumped;
                    _movementSystem.OnLanded -= _movementSystem_OnLanded;
                }
            }

            private void _movementSystem_OnJumped(IMovementSystem movementSystem)
            {
                TryActiveAbility();
            }
            private void _movementSystem_OnLanded(IMovementSystem movementSystem)
            {
                TryFinishAbility();
            }
        }
    }
}
