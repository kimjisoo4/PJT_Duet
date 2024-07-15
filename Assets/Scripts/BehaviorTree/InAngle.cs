using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using StudioScor.Utilities;
using UnityEngine;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class InAngle : Conditional
    {
        [Header(" [ In Angle ] ")]
        [SerializeField][RequiredField] private SharedTransform _from;
        [SerializeField][RequiredField] private SharedTransform _to;
        [SerializeField] private SharedFloat _angle = 45f;

        public override TaskStatus OnUpdate()
        {
            var lhs = _from.Value;
            var rhs = _to.Value;

            float angle = _angle.Value;

            return lhs.AngleOnForward(rhs) <= angle ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}

