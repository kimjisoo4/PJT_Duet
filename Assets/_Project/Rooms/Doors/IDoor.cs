namespace PF.PJT.Duet
{
    public interface IDoor
    {
        public delegate void ChangeDoorStateEvent(IDoor door);

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
