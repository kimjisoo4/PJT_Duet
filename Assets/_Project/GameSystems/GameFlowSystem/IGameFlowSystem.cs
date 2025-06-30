using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    public interface IGameFlowSystem
    {
        public GameObject gameObject { get; }

        public GameFlowState.StateMachine StateMachine { get; }

        public bool TrySetState(GameFlowState gameFlowState);
    }
}
