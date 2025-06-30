using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    public class BaseGameFlowSystem : BaseMonoBehaviour, IGameFlowSystem
    {
        [Header(" [ Base Game Flow System ] ")]
        [SerializeField] private GameFlowState.StateMachine _stateMachine;
        public GameFlowState.StateMachine StateMachine => _stateMachine;

        public bool TrySetState(GameFlowState gameFlowState)
        {
            return _stateMachine.TrySetState(gameFlowState);
        }
    }
}
