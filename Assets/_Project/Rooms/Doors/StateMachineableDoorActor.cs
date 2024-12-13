using UnityEngine;

namespace PF.PJT.Duet
{
    [AddComponentMenu("Duet/Door/State Machineable Door Actor")]
    public class StateMachineableDoorActor : DoorActor
    {
        [Header(" [ State Machineable Door Actor ]")]

        [Header(" State Machine ")]
        [SerializeField] private DoorActorState.StateMachine _stateMachine;
        [SerializeField] private DoorActorState _openTransitionState;
        [SerializeField] private DoorActorState _openState;
        [SerializeField] private DoorActorState _closeTransitionState;
        [SerializeField] private DoorActorState _closeState;

        public void FInishedState(DoorActorState doorState)
        {
            if (_stateMachine.CurrentState != doorState)
                return;

            if (doorState == _openTransitionState)
            {
                _stateMachine.TrySetState(_openState);
                FinisheOpen();
            }
            else if (doorState == _closeTransitionState)
            {
                _stateMachine.TrySetState(_closeState);
                FinisheClose();
            }
        }

        public override bool CanOpen()
        {
            return base.CanOpen() && _stateMachine.CanSetState(_openTransitionState);
        }
        public override bool CanClose()
        {
            return base.CanClose() && _stateMachine.CanSetState(_closeTransitionState);
        }

        protected override void OnOpen()
        {
            _stateMachine.ForceSetState(_openTransitionState);
        }
        protected override void OnClose()
        {
            _stateMachine.ForceSetState(_closeTransitionState);
        }

    }
}
