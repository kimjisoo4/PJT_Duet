using UnityEngine;


namespace PF.PJT.Duet
{
    public class OpenDoorsRoomState : RoomState
    {
        [Header(" [ Open Doors State ] ")]
        [SerializeField] private GameObject[] _doorActors;

        private IDoorActor[] _doors;
        private int _remainOpenCount;

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
        private void Door_OnStaredOpenDoor(IDoorActor door)
        {
            door.OnStartedOpenDoor -= Door_OnStaredOpenDoor;

            _remainOpenCount++;
        }

        private void Door_OnFinishedOpenDoor(IDoorActor door)
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
