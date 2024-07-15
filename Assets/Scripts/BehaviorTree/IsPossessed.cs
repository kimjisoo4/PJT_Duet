using BehaviorDesigner.Runtime.Tasks;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class IsPossessed : PlayerSystemConditional
    {
        public override TaskStatus OnUpdate()
        {
            return IsPossessed ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}

