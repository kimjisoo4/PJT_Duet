using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{


    public class StageFlowSystemController : BaseMonoBehaviour
    {
        [Header(" [ Stage Flow System Controller ] ")]
        [SerializeField] private GameFlowSystemComponent _gameFlowSystem;

        [Header(" States ")]
        [SerializeField] private GameFlowState _idleState;
        [SerializeField] private GameFlowState _battleInRoomState;
        [SerializeField] private GameFlowState _stageRewordState;
        [Header(" Events ")]
        [Header(" Listener ")]
        [SerializeField] private GameEvent _onStartedRoom;
        [SerializeField] private GameEvent _onFinishedRoom;
        [SerializeField] private GameEvent _onRequestReword;

        private void Awake()
        {
            _onStartedRoom.OnTriggerEvent += _onStartedRoom_OnTriggerEvent;
            _onFinishedRoom.OnTriggerEvent += _onFinishedRoom_OnTriggerEvent;

            _onRequestReword.OnTriggerEvent += _onRequestReword_OnTriggerEvent;
        }

        

        private void OnDestroy()
        {
            _onStartedRoom.OnTriggerEvent -= _onStartedRoom_OnTriggerEvent;
            _onFinishedRoom.OnTriggerEvent -= _onFinishedRoom_OnTriggerEvent;

            _onRequestReword.OnTriggerEvent -= _onRequestReword_OnTriggerEvent;
        }

        private void _onStartedRoom_OnTriggerEvent()
        {
            _gameFlowSystem.TrySetState(_battleInRoomState);
        }

        private void _onFinishedRoom_OnTriggerEvent()
        {
            _gameFlowSystem.TrySetState(_stageRewordState);
        }

        private void _onRequestReword_OnTriggerEvent()
        {
            _gameFlowSystem.TrySetState(_idleState);
        }
    }
}
