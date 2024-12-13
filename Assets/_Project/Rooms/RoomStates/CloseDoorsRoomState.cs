using UnityEngine;


namespace PF.PJT.Duet
{
    public class CloseDoorsRoomState : RoomState
    {
        [Header(" [ Close Doors State ] ")]
        [SerializeField] private GameObject[] _doorActors;

        private IDoorActor[] _doors;
        private int _remainClosedCount;

        private void Awake()
        {
            _doors = new IDoorActor[_doorActors.Length];

            for (int i = 0; i < _doorActors.Length; i++)
            {
                var doorActor = _doorActors[i];

                _doors[i] = doorActor.GetComponent<IDoorActor>();
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

        private void Door_OnStaredCloseDoor(IDoorActor door)
        {
            door.OnStartedCloseDoor -= Door_OnStaredCloseDoor;

            _remainClosedCount++;
        }

        private void Door_OnFinishedCloseDoor(IDoorActor door)
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
