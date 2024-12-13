using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{

    public class GameFlowSystemComponent : BaseMonoBehaviour
    {
        [Header(" [ Game Flow System ] ")]
        [SerializeField] private GameFlowState.StateMachine _stateMachine;
        public GameFlowState.StateMachine StateMachine => _stateMachine;


        private void Start()
        {
            _stateMachine.Start();
        }

        public void TrySetState(GameFlowState gameFlowState)
        {
            _stateMachine.TrySetState(gameFlowState);
        }

        public void ForceSetState(GameFlowState gameFlowState)
        {
            _stateMachine.ForceSetState(gameFlowState);
        }
    }
}
