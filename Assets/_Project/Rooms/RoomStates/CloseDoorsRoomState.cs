using UnityEngine;


namespace PF.PJT.Duet
{
    public class CloseDoorsRoomState : RoomState
    {
        [Header(" [ Close Doors State ] ")]
        [SerializeField] private GameObject[] _doorActors;

        private IDoor[] _doors;
        private int _remainClosedCount;

        private void Awake()
        {
            _doors = new IDoor[_doorActors.Length];

            for (int i = 0; i < _doorActors.Length; i++)
            {
                var doorActor = _doorActors[i];

                _doors[i] = doorActor.GetComponent<IDoor>();
            }
        }

        protected override void EnterState()
        {
            base.EnterState();

            CloseAllDoor();
        }

        private void CloseAllDoor()
        {
            foreach (var door in _doors)
            {
                door.OnStartedCloseDoor += Door_OnStaredCloseDoor;
                door.OnFinishedCloseDoor += Door_OnFinishedCloseDoor;

                door.TryClose();
            }
        }

        private void Door_OnStaredCloseDoor(IDoor door)
        {
            door.OnStartedCloseDoor -= Door_OnStaredCloseDoor;

            _remainClosedCount++;
        }

        private void Door_OnFinishedCloseDoor(IDoor door)
        {
            door.OnFinishedCloseDoor -= Door_OnFinishedCloseDoor;

            _remainClosedCount--;

            if (_remainClosedCount <= 0)
            {
                RoomController.ForceNextState(this);
            }
        }

    }

}
