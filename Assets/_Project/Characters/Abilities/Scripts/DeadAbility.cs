using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/State/new Dead Ability", fileName = "GA_PawnAbiltiy_Dead")]
    public class DeadAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            private readonly AnimationPlayer _animationPlayer;
            private readonly int ANIM_DEAD_ID = Animator.StringToHash("Dead");

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _animationPlayer = gameObject.GetComponentInChildren<AnimationPlayer>(true);
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _animationPlayer.Play(ANIM_DEAD_ID);
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                AbilitySystem.TryRemoveAbility(Ability);
            }
        }
    }
}
