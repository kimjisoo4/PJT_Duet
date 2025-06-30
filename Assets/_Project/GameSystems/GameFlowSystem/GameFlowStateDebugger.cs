using StudioScor.Utilities;
using TMPro;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    public class GameFlowStateDebugger : BaseMonoBehaviour
    {
        [Header(" [ Game Flow State Debugger ] ")]
        [SerializeField] private BaseGameFlowSystem _gameFlowSystem;
        [SerializeField] private TMP_Text _stateField;


        private void Awake()
        {
            if(_gameFlowSystem.StateMachine.IsPlaying)
            {
                _stateField.text = _gameFlowSystem.StateMachine.CurrentState.ToString();
            }
            
            _gameFlowSystem.StateMachine.OnChangedState += StateMachine_OnChangedState;
        }

        private void OnDestroy()
        {
            _gameFlowSystem.StateMachine.OnChangedState -= StateMachine_OnChangedState;
        }

        private void StateMachine_OnChangedState(FiniteStateMachineSystem<GameFlowState> stateMachine, GameFlowState currentState, GameFlowState prevState)
        {
            _stateField.text = currentState.ToString();
        }
    }
}
