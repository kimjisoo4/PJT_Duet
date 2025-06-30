using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class CheckToTitleUI : BaseToolkitUI
    {
        public delegate void CheckToTitleUIStateEventHandler(CheckToTitleUI checkExitGameUI);

        [Header(" [ Check ExitGame UI Controller ] ")]
        [SerializeField] private GameObject _acceptButtonActor;
        [SerializeField] private GameObject _rejectButtonActor;

        private IUIToolkitSubmitEventListener _acceptSubmit;
        private IUIToolkitCancelEventListener _acceptCancel;

        private IUIToolkitSubmitEventListener _rejectSubmit;

        public event CheckToTitleUIStateEventHandler OnAccepted;
        public event CheckToTitleUIStateEventHandler OnRejected;

        protected override void Awake()
        {
            base.Awake();

            _acceptSubmit = _acceptButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
            _acceptCancel = _acceptButtonActor.GetComponent<IUIToolkitCancelEventListener>();

            _rejectSubmit = _rejectButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            RemoveAllBinding();

            OnAccepted = null;
            OnRejected = null;
        }

        private void RemoveAllBinding()
        {
            if (_acceptSubmit is not null)
                _acceptSubmit.OnSubmited -= _acceptSubmit_OnSubmited;
            if (_acceptCancel is not null)
                _acceptCancel.OnCanceled -= _acceptCancel_OnCanceled;

            if (_rejectSubmit is not null)
                _rejectSubmit.OnSubmited -= _rejectSubmit_OnSubmited;
        }
        protected override void OnActivateEnd()
        {
            base.OnActivateEnd();

            _acceptSubmit.OnSubmited += _acceptSubmit_OnSubmited;
            _acceptCancel.OnCanceled += _acceptCancel_OnCanceled;

            _rejectSubmit.OnSubmited += _rejectSubmit_OnSubmited;
            _rejectSubmit.Target.Focus();
        }

        protected override void OnDeactivateStart()
        {
            base.OnDeactivateStart();
            
            RemoveAllBinding();
        }

        public void Accept()
        {
            Log(nameof(OnAccepted));

            OnAccepted?.Invoke(this);
        }
        public void Reject()
        {
            Log(nameof(OnRejected));

            OnRejected?.Invoke(this);
        }

        private void _rejectSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            Reject();
        }

        private void _acceptCancel_OnCanceled(IUIToolkitEventListener uitookiEventListener)
        {
            _rejectSubmit.Target.Focus();
        }

        private void _acceptSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            Accept();
        }
    }
}
