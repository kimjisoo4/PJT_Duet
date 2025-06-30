using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    public class DevFlowSystemController : BaseMonoBehaviour
    {
        [Header(" [ Dev Flow System Controller ] ")]
        [SerializeField] private BaseGameFlowSystem _gameFlowSystem;

        [Header(" Create Character  ")]
        [SerializeField] private GameFlowState _createCharacterState;
        [SerializeField] private GameEvent _onActivatedCreateCharacter;

        [Header(" Room ")]
        [SerializeField] private GameFlowState _roomState;
        [SerializeField] private GameEvent _onStartedRoom;

        [Header(" Dev Logs ")]
        [SerializeField] private GameEvent _onUsedDevLog;
        [SerializeField] private GameEvent _onUnusedDevLog;
        [SerializeField] private SOBoolVariable _useDevLogVariable;


        private void Awake()
        {
            _gameFlowSystem.StateMachine.OnChangedState += StateMachine_OnChangedState;

            _useDevLogVariable.Load();
            _useDevLogVariable.OnValueChanged += _useDevLogVariable_OnChangedValue;

            _onUsedDevLog.OnTriggerEvent += _onUsedDevLog_OnTriggerEvent;
            _onUnusedDevLog.OnTriggerEvent += _onUnusedDevLog_OnTriggerEvent;

            _onStartedRoom.OnTriggerEvent += _onStartedRoom_OnTriggerEvent;

            _onActivatedCreateCharacter.OnTriggerEvent += _onActivatedCreateCharacter_OnTriggerEvent;
        }

        private void OnDestroy()
        {
            _gameFlowSystem.StateMachine.OnChangedState -= StateMachine_OnChangedState;

            _useDevLogVariable.OnValueChanged -= _useDevLogVariable_OnChangedValue;

            _onUsedDevLog.OnTriggerEvent -= _onUsedDevLog_OnTriggerEvent;
            _onUnusedDevLog.OnTriggerEvent -= _onUnusedDevLog_OnTriggerEvent;

            _onStartedRoom.OnTriggerEvent -= _onStartedRoom_OnTriggerEvent;

            _onActivatedCreateCharacter.OnTriggerEvent -= _onActivatedCreateCharacter_OnTriggerEvent;
        }
        

        private void _onUsedDevLog_OnTriggerEvent()
        {
            _useDevLogVariable.Value = true;
            _useDevLogVariable.Save();
        }

        private void _onUnusedDevLog_OnTriggerEvent()
        {
            _useDevLogVariable.Value = false;
            _useDevLogVariable.Save();
        }

        private void _onActivatedCreateCharacter_OnTriggerEvent()
        {
            if (!_useDevLogVariable.Value)
                return;

            _gameFlowSystem.StateMachine.TrySetState(_createCharacterState);
        }
        private void _onStartedRoom_OnTriggerEvent()
        {
            if (!_useDevLogVariable.Value)
                return;

            _gameFlowSystem.StateMachine.TrySetState(_roomState);
        }

        private void _useDevLogVariable_OnChangedValue(SOVariable<bool> variable, bool currentValue, bool prevValue)
        {
            if (currentValue)
            {
                _gameFlowSystem.StateMachine.Start();
            }
            else
            {
                _gameFlowSystem.StateMachine.End();
                Time.timeScale = 1;
            }
        }

        private void StateMachine_OnChangedState(FiniteStateMachineSystem<GameFlowState> stateMachine, GameFlowState currentState, GameFlowState prevState)
        {
            if (currentState == stateMachine.DefaultState)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
            }
        }
    }
}
