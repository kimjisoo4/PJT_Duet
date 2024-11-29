using StudioScor.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PF.PJT.Duet
{
    public class RoomController : BaseMonoBehaviour
    {
        [Header(" [ Enemy Room Controller ] ")]
        [SerializeField] private FiniteStateMachineSystem<RoomState> _stateMachine;
        [SerializeField] private RoomState[] _roomStates;

        [Header(" Events ")]
        [SerializeField] private ToggleableUnityEvent _onStartedRoom;
        [SerializeField] private ToggleableUnityEvent _onFinishedRoom;

        private void Start()
        {
            _stateMachine.Start();
        }

        public void ForceNextState(RoomState roomState)
        {
            Log(nameof(ForceNextState));    

            if (_stateMachine.CurrentState != roomState)
                return;

            int index = _roomStates.IndexOf(roomState);
            var nextStaet = _roomStates.ElementAtOrDefault(index + 1);

            if (nextStaet)
            {
                if (index == 0)
                {
                    Inovke_OnStartedRoom();
                }

                _stateMachine.ForceSetState(nextStaet);
            }
            else if (index == _roomStates.LastIndex())
            {
                _stateMachine.End();

                Invoke_OnFInishedRoom();
            }
            else
            {
                return;
            }
        }

        public bool TryNextState(RoomState roomState)
        {
            Log(nameof(TryNextState));

            if (_stateMachine.CurrentState != roomState)
                return false;

            int index = _roomStates.IndexOf(roomState);
            var nextStaet = _roomStates.ElementAtOrDefault(index + 1);

            if(nextStaet)
            {
                if (_stateMachine.CanSetState(nextStaet))
                {
                    if (index == 0)
                    {
                        Inovke_OnStartedRoom();
                    }

                    _stateMachine.ForceSetState(nextStaet);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if(index == _roomStates.LastIndex())
            {
                _stateMachine.End();

                Invoke_OnFInishedRoom();

                return false;
            }
            else
            {
                return false;
            }
        }

        private void Inovke_OnStartedRoom()
        {
            _onStartedRoom.Invoke();
        }
        private void Invoke_OnFInishedRoom()
        {
            _onFinishedRoom.Invoke();
        }
    }
}
