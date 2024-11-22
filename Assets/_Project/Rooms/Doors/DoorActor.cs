using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace PF.PJT.Duet
{
    public class DoorActor : BaseMonoBehaviour, IDoor
    {
        [Header(" [ Door Actor ] ")]
        [SerializeField] private bool _isStartedOpen = true;

        [Header(" Events ")]
        [SerializeField] private bool _useUnityEvent;
        
        [Header(" Open ")]
        [SerializeField] private UnityEvent _onFailedOpenDoor;
        [SerializeField] private UnityEvent _onStartedOpenDoor;
        [SerializeField] private UnityEvent _onFinishedOpenDoor;
        
        [Header(" Close ")]
        [SerializeField] private UnityEvent _onFailedCloseDoor;
        [SerializeField] private UnityEvent _onStartedCloseDoor;
        [SerializeField] private UnityEvent _onFinishedCloseDoor;

        private bool _isOpened;
        public bool IsOpened => _isOpened;

        public event IDoor.ChangeDoorStateEvent OnFailedOpenDoor;
        public event IDoor.ChangeDoorStateEvent OnStartedOpenDoor;
        public event IDoor.ChangeDoorStateEvent OnFinishedOpenDoor;
        public event IDoor.ChangeDoorStateEvent OnStartedCloseDoor;
        public event IDoor.ChangeDoorStateEvent OnFinishedCloseDoor;
        public event IDoor.ChangeDoorStateEvent OnFailedCloseDoor;

        private void Awake()
        {
            if(_isStartedOpen)
            {
                ForceOpen();
            }
            else
            {
                ForceClose();
            }
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

            if (_useUnityEvent)
                _onStartedOpenDoor?.Invoke();

            OnStartedOpenDoor?.Invoke(this);
        }
        private void Invoke_OnFinishedOpenDoor()
        {
            Log(nameof(OnFinishedOpenDoor));

            if (_useUnityEvent)
                _onFinishedOpenDoor?.Invoke();

            OnFinishedOpenDoor?.Invoke(this);
        }
        private void Invoke_OnFailedOpenDoor()
        {
            Log(nameof(OnFailedOpenDoor));

            if (_useUnityEvent)
                _onFailedOpenDoor?.Invoke();

            OnFailedOpenDoor?.Invoke(this);
        }
        private void Invoke_OnStartedCloseDoor()
        {
            Log(nameof(OnStartedCloseDoor));

            if (_useUnityEvent)
                _onStartedCloseDoor?.Invoke();

            OnStartedCloseDoor?.Invoke(this);
        }
        private void Invoke_OnFinishedCloseDoor()
        {
            Log(nameof(OnFinishedCloseDoor));

            if (_useUnityEvent)
                _onFinishedCloseDoor?.Invoke();

            OnFinishedCloseDoor?.Invoke(this);
        }
        private void Invoke_OnFailedCloseDoor()
        {
            Log(nameof(OnFailedCloseDoor));

            if (_useUnityEvent)
                _onFailedCloseDoor?.Invoke();

            OnFailedCloseDoor?.Invoke(this);
        }
    }

}
