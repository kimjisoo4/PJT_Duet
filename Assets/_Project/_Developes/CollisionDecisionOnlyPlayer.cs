using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/Collision Decisions/new Only Player", fileName = "CollisionDecision_OnlyPlayer")]
    public class CollisionDecisionOnlyPlayer : IgnoreColliderDecision
    {
        public override bool Decision(Collider other)
        {
            if(other.TryGetActor(out IActor actor))
            {
                if(actor.gameObject.TryGetController(out IControllerSystem controllerSystem))
                {
                    return controllerSystem.IsPlayer;
                }
            }

            return false;
        }
    }
}
