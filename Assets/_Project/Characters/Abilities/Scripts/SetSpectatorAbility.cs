﻿using PF.PJT.Duet.Define;
using StudioScor.AbilitySystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnAbility
{
    [CreateAssetMenu(menuName = "Project/Duet/PawnAbility/State/new Set Spectator Ability", fileName = "GA_PawnAbiltiy_SetSpectator")]
    public class SetSpectatorAbility : GASAbility
    {
        public override IAbilitySpec CreateSpec(IAbilitySystem abilitySystem, int level = 0)
        {
            return new Spec(this, abilitySystem, level);
        }

        public class Spec : GASPassiveAbilitySpec
        {
            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
            }

            protected override void EnterAbility()
            {
                base.EnterAbility();

                gameObject.layer = ProjectDefine.Layer.INVISIBILITY;
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                gameObject.layer = ProjectDefine.Layer.CHARACTER;
            }
        }
    }
}