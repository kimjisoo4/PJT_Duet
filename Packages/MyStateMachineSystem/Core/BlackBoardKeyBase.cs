using StudioScor.Utilities;

namespace StudioScor.StateMachine
{
    public abstract class BlackBoardKeyBase : BaseScriptableObject
    {
        public abstract bool HasValue(StateMachineComponent stateMachine);
        public abstract void Create(StateMachineComponent stateMachine);
        public abstract void Clear(StateMachineComponent stateMachine);
        public abstract void Remove(StateMachineComponent stateMachine);
    }
}