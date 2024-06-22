using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.RotationSystem;
using StudioScor.PlayerSystem;
using StudioScor.GameplayTagSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Rotation/new Update Rotation Input Ability", fileName = "GA_PawnAbiltiy_UpdateRotationInput")]
    public class UpdateRotationInputAbility : GASAbility
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

            protected override void ExitAbility()   
            {
                base.ExitAbility();

                _rotationSystem.SetLookDirection(Vector3.zero);
            }
            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }

            public void UpdateAbility(float deltaTime)
            {
                if (!IsPlaying)
                    return;

                _rotationSystem.SetLookDirection(_pawnSystem.TurnDirection);
            }
        }
    }
}
