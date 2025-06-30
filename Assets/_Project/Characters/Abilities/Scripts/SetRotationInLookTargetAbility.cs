using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.RotationSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Rotation/new Set Rotation In Look Target Ability", fileName = "GA_PawnAbiltiy_SetRotationInLookTarget")]
    public class SetRotationInLookTargetAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            private readonly IRotationSystem _rotationSystem;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _rotationSystem = transform.GetRotationSystem();
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _rotationSystem.TransitionRotationType(ERotationType.Target);
            }
        }
    }
}
