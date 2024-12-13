using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    [AddComponentMenu("Duet/Door/Door Actor")]
    public class DoorActor : BaseMonoBehaviour, IDoorActor
    {
        [Header(" [ Door Actor ] ")]
        [SerializeField] private bool _isStartedOpen = true;

        [Header(" Open ")]
        [SerializeField] private ToggleableUnityEvent _onFailedOpenDoor;
        [SerializeField] private ToggleableUnityEvent _onStartedOpenDoor;
        [SerializeField] private ToggleableUnityEvent _onFinishedOpenDoor;

        [Header(" Close ")]
        [SerializeField] private ToggleableUnityEvent _onFailedCloseDoor;
        [SerializeField] private ToggleableUnityEvent _onStartedCloseDoor;
        [SerializeField] private ToggleableUnityEvent _onFinishedCloseDoor;


        private bool _wasInit = false;

        private bool _isOpened;
        public bool IsOpened => _isOpened;

        public event IDoorActor.ChangeDoorStateEvent OnFailedOpenDoor;
        public event IDoorActor.ChangeDoorStateEvent OnStartedOpenDoor;
        public event IDoorActor.ChangeDoorStateEvent OnFinishedOpenDoor;
        public event IDoorActor.ChangeDoorStateEvent OnStartedCloseDoor;
        public event IDoorActor.ChangeDoorStateEvent OnFinishedCloseDoor;
        public event IDoorActor.ChangeDoorStateEvent OnFailedCloseDoor;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            OnInit();

            if (_isStartedOpen)
            {
                ForceOpen();
            }
            else
            {
                ForceClose();
            }
        }

        protected virtual void OnInit()
        {

        }

        

        public bool TryOpen()
        {
            if (!CanOpen())
            {
                FailedOpen();

                return false;
            }

            ForceOpen();

            return true;
        }
        public bool TryClose()
        {
            if (!CanClose())
            {
                FailedClose();

                return false;
            }

            ForceClose();

            return true;
        }

        public virtual bool CanOpen()
        {
            return !IsOpened;
        }
        public virtual bool CanClose()
        {
            return IsOpened;
        }
        public void ForceOpen()
        {
            _isOpened = true;

            Invoke_OnStartedOpenDoor();

            OnOpen();
        }
        public void ForceClose()
        {
            _isOpened = false;

            Invoke_OnStartedCloseDoor();

            OnClose();
        }
        protected void FailedOpen()
        {
            OnFailedOpen();

            Invoke_OnFailedOpenDoor();
        }
        protected void FailedClose()
        {
            OnFailedClose();

            Invoke_OnFailedCloseDoor();
        }
        protected void FinisheOpen()
        {
            EndOpen();

            Invoke_OnFinishedOpenDoor();
        }
        protected void FinisheClose()
        {
            EndClose();

            Invoke_OnFinishedCloseDoor();
        }

        protected virtual void OnOpen()
        {
            FinisheOpen();
        }
        protected virtual void OnClose()
        {
            FinisheClose();
        }

        protected virtual void EndOpen()
        {
        }
        protected virtual void EndClose()
        {
        }

        protected virtual void OnFailedOpen()
        {
        }
        protected virtual void OnFailedClose()
        {
        }



        private void Invoke_OnStartedOpenDoor()
        {
            Log(nameof(OnStartedOpenDoor));

            _onStartedOpenDoor.Invoke();

            OnStartedOpenDoor?.Invoke(this);
        }
        private void Invoke_OnFinishedOpenDoor()
        {
            Log(nameof(OnFinishedOpenDoor));

            _onFinishedOpenDoor.Invoke();

            OnFinishedOpenDoor?.Invoke(this);
        }
        private void Invoke_OnFailedOpenDoor()
        {
            Log(nameof(OnFailedOpenDoor));

            _onFailedOpenDoor.Invoke();

            OnFailedOpenDoor?.Invoke(this);
        }
        private void Invoke_OnStartedCloseDoor()
        {
            Log(nameof(OnStartedCloseDoor));

            _onStartedCloseDoor.Invoke();

            OnStartedCloseDoor?.Invoke(this);
        }
        private void Invoke_OnFinishedCloseDoor()
        {
            Log(nameof(OnFinishedCloseDoor));

            _onFinishedCloseDoor.Invoke();

            OnFinishedCloseDoor?.Invoke(this);
        }
        private void Invoke_OnFailedCloseDoor()
        {
            Log(nameof(OnFailedCloseDoor));

            _onFailedCloseDoor.Invoke();

            OnFailedCloseDoor?.Invoke(this);
        }
    }
}
