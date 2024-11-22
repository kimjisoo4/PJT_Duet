using UnityEngine;


namespace PF.PJT.Duet
{
    public class OpenDoorsRoomState : RoomState
    {
        [Header(" [ Open Doors State ] ")]
        [SerializeField] private GameObject[] _doorActors;

        private IDoor[] _doors;
        private int _remainOpenCount;

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

            OpenAllDoor();
        }

        private void OpenAllDoor()
        {
            foreach (var door in _doors)
            {
                door.OnStartedOpenDoor += Door_OnStaredOpenDoor;
                door.OnFinishedOpenDoor += Door_OnFinishedOpenDoor;

                door.TryOpen();
            }
        }
        private void Door_OnStaredOpenDoor(IDoor door)
        {
            door.OnStartedOpenDoor -= Door_OnStaredOpenDoor;

            _remainOpenCount++;
        }

        private void Door_OnFinishedOpenDoor(IDoor door)
        {
            door.OnFinishedOpenDoor -= Door_OnFinishedOpenDoor;

            _remainOpenCount--;

            if(_remainOpenCount <= 0)
            {
                RoomController.ForceNextState(this);
            }
        }

       
    }

}
