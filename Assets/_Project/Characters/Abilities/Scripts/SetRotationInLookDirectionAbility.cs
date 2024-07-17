using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.RotationSystem;
using StudioScor.PlayerSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Rotation/new Set Rotation In Look Direction Ability", fileName = "GA_PawnAbiltiy_SetRotationInLookDirection")]
    public class SetRotationInLookDirectionAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec, IUpdateableAbilitySpec
        {
            private readonly IPawnSystem _pawnSystem;
            private readonly IRotationSystem _rotationSystem;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _pawnSystem = gameObject.GetPawnSystem();
                _rotationSystem = gameObject.GetRotationSystem();
            }

            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
                {
                    _rotationSystem.SetLookDirection(_pawnSystem.LookDirection);
                }
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _rotationSystem.TransitionRotationType(ERotationType.Direction);
                _rotationSystem.SetLookDirection(_pawnSystem.LookDirection);
            }

            protected override void ExitAbility()
            {
                _rotationSystem.SetLookDirection(Vector3.zero);

                base.ExitAbility();
            }
        }
    }
}
