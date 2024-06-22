using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/State/new Dilation Ability", fileName = "GA_PawnAbiltiy_Dilation")]
    public class DilationAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            private readonly IDilationSystem _dilationSystem;
            private readonly Animator _animator;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _dilationSystem = gameObject.GetDilationSystem();
                _animator = gameObject.GetComponentInChildren<Animator>(true);
            }

            protected override void OnGrantAbility()
            {
                base.OnGrantAbility();

                _dilationSystem.OnChangedDilation += _dilationSystem_OnChangedDilation;
            }
            protected override void OnRemoveAbility()
            {
                base.OnRemoveAbility();

                if(_dilationSystem is not null)
                {
                    _dilationSystem.OnChangedDilation -= _dilationSystem_OnChangedDilation;
                }
            }


            protected override void EnterAbility()
            {
                base.EnterAbility();

                _dilationSystem.SetDilation(0f);
            }
            protected override void ExitAbility()
            {
                base.ExitAbility();

                _dilationSystem.ResetDilation();
            }
            private void _dilationSystem_OnChangedDilation(IDilationSystem dilation, float currentDilation, float prevDilation)
            {
                _animator.speed = currentDilation;
            }

        }
    }
}
