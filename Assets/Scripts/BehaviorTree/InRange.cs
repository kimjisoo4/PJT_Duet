using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace StudioScor.PlayerSystem.BehaviorTree
{

    public class InRange : Conditional
    {
        [Header(" [ In Range ] ")]
        [SerializeField][RequiredField] private SharedTransform _from;
        [SerializeField][RequiredField] private SharedTransform _to;
        [SerializeField] private SharedFloat _distance = 5f;

        public override TaskStatus OnUpdate()
        {
            var lhs = _from.Value;
            var rhs = _to.Value;

            float distance = _distance.Value;

            return Vector3.Distance(lhs.position, rhs.position) <= distance ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}

