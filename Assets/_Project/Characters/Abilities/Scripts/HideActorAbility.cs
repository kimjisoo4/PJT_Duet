using StudioScor.AbilitySystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{

    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/State/new Hide Actor Ability", fileName = "GA_PawnAbiltiy_HideActor")]
    public class HideActorAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            private readonly ICharacter _character;
            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _character = gameObject.GetComponent<ICharacter>();
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                _character.Model.SetActive(false);
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                _character.Model.SetActive(true);
            }
        }
    }
}
