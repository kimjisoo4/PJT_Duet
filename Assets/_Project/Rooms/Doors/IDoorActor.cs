using UnityEngine;

namespace PF.PJT.Duet
{
    public interface IDoorActor
    {
        public delegate void ChangeDoorStateEvent(IDoorActor door);

        public GameObject gameObject { get; }
        public Transform transform { get; }

        public bool IsOpened { get; }

        public bool CanOpen();
        public bool CanClose();
        public bool TryOpen();
        public bool TryClose();
        public void ForceOpen();
        public void ForceClose();

        public event ChangeDoorStateEvent OnStartedOpenDoor;
        public event ChangeDoorStateEvent OnFinishedOpenDoor;
        public event ChangeDoorStateEvent OnFailedOpenDoor;

        public event ChangeDoorStateEvent OnStartedCloseDoor;
        public event ChangeDoorStateEvent OnFinishedCloseDoor;
        public event ChangeDoorStateEvent OnFailedCloseDoor;
    }

}
