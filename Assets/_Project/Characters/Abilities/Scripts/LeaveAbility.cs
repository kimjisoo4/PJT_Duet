using PF.PJT.Duet.Define;
using StudioScor.AbilitySystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/State/new Leave Ability", fileName = "GA_PawnAbiltiy_Leave")]
    public class LeaveAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASAbilitySpec
        {
            private readonly ICharacter _character;
            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _character = gameObject.GetComponent<Character>();
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _character.Model.SetActive(false);
                gameObject.layer = ProjectDefine.Layer.INVISIBILITY;
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _character.Model.SetActive(true);
                gameObject.layer = ProjectDefine.Layer.CHARACTER;
            }
        }
    }
}
