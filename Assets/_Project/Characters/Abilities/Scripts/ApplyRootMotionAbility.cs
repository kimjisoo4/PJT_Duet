using UnityEngine;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using StudioScor.MovementSystem;
using StudioScor.RotationSystem;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/Animation/new Apply Root Motion Ability", fileName = "GA_PawnAbiltiy_ApplyRootMotion")]
    public class ApplyRootMotionAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec, IUpdateableAbilitySpec
        {
            private readonly IApplyRootMotion _applyRootMotion;
            private readonly IMovementSystem _movementSystem;
            private readonly IRotationSystem _rotationSystem;


            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _applyRootMotion = gameObject.GetComponentInChildren<IApplyRootMotion>();
                _movementSystem = gameObject.GetMovementSystem();
                _rotationSystem = gameObject.GetRotationSystem();

                _applyRootMotion.SetUseRootMotion(false);
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _applyRootMotion.SetUseRootMotion(true);
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _applyRootMotion.SetUseRootMotion(false);
            }
            public void UpdateAbility(float deltaTime)
            {
                if(IsPlaying)
                {
                    _movementSystem.MovePosition(_applyRootMotion.DeltaPosition);
                    _rotationSystem.AddRotation(_applyRootMotion.DeltaRotation);
                }
            }
            public void FixedUpdateAbility(float deltaTime)
            {
                return;
            }


        }
    }
}
