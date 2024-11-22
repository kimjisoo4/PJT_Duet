using StudioScor.Utilities;
using UnityEngine;


namespace PF.PJT.Duet
{
    public class SelectRewordRoomState : RoomState
    {
        [Header(" [ Select Reword State ] ")]

        [Header(" Events ")]
        [Header(" Post ")]
        [SerializeField] private GameEvent _onRequestRewordSystem;

        [Header(" Listen ")]
        [SerializeField] private GameEvent _onStratedRewordSystem;
        [SerializeField] private GameEvent _onFinishedRewordSystem;

        private bool _wasStartedRewordSystem;

        protected override void EnterState()
        {
            base.EnterState();

            _wasStartedRewordSystem = false;

            _onStratedRewordSystem.OnTriggerEvent += _onStratedRewordSystem_OnTriggerEvent;
            _onFinishedRewordSystem.OnTriggerEvent += _onFinishedRewordSystem_OnTriggerEvent;

            _onRequestRewordSystem.Invoke();
        }

        private void _onStratedRewordSystem_OnTriggerEvent()
        {
            _wasStartedRewordSystem = true;
        }
        private void _onFinishedRewordSystem_OnTriggerEvent()
        {
            if(_wasStartedRewordSystem)
            {
                RoomController.TryNextState(this);
            }
        }
    }
}
