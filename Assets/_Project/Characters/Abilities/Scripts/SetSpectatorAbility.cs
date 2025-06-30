using PF.PJT.Duet.Define;
using StudioScor.AbilitySystem;
using UnityEngine;
using UnityEngine.AI;

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
            private readonly NavMeshAgent _navmeshAgent;

            public Spec(Ability ability, IAbilitySystem abilitySystem, int level) : base(ability, abilitySystem, level)
            {
                _navmeshAgent = gameObject.GetComponent<NavMeshAgent>();
            }
            protected override void EnterAbility()
            {
                base.EnterAbility();

                gameObject.layer = ProjectDefine.Layer.INVISIBILITY;

                _navmeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
                _navmeshAgent.avoidancePriority = 99;
            }

            protected override void ExitAbility()
            {
                base.ExitAbility();

                gameObject.layer = ProjectDefine.Layer.CHARACTER;

                _navmeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                _navmeshAgent.avoidancePriority = 0;
            }
        }
    }
}
