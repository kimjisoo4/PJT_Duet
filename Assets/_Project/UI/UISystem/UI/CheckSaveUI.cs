using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class CheckSaveUI : BaseToolkitUI
    {
        public delegate void CheckSaveUIEventHandler(CheckSaveUI checkSaveUI);

        [Header(" [ Check Save UI Controller ] ")]
        [SerializeField] private GameObject _acceptButtonActor;
        [SerializeField] private GameObject _rejectButtonActor;

        private IUIToolkitSubmitEventListener _acceptSubmit;
        private IUIToolkitCancelEventListener _acceptCancel;

        private IUIToolkitSubmitEventListener _rejectSubmit;

        public event CheckSaveUIEventHandler OnAccepted;
        public event CheckSaveUIEventHandler OnRejected;

        
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
                _acceptSubmit.OnSubmited -= AcceptSubmit_OnSubmited;
            if (_acceptCancel is not null)
                _acceptCancel.OnCanceled -= AcceptCancel_OnCanceled;

            if (_rejectSubmit is not null)
                _rejectSubmit.OnSubmited -= RejectSubmit_OnSubmited;
        }

        protected override void OnActivateEnd()
        {
            base.OnActivateEnd();

            _acceptSubmit.OnSubmited += AcceptSubmit_OnSubmited;
            _acceptCancel.OnCanceled += AcceptCancel_OnCanceled;

            _rejectSubmit.OnSubmited += RejectSubmit_OnSubmited;
            _acceptSubmit.Target.Focus();
        }

        protected override void OnDeactivateStart()
        {
            base.OnDeactivateStart();
            
            RemoveAllBinding();
        }

        public void Accept()
        {
            Inovke_OnAccepted();
        }
        public void Reject()
        {
            Inovke_OnRejected();
        }


        private void AcceptSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            Accept();
        }
        private void AcceptCancel_OnCanceled(IUIToolkitEventListener uitookiEventListener)
        {
            _rejectSubmit.Target.Focus();
        }

        private void RejectSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            Reject();
        }


        private void Inovke_OnAccepted()
        {
            Log(nameof(OnAccepted));

            OnAccepted?.Invoke(this);
        }
        private void Inovke_OnRejected()
        {
            Log(nameof(OnRejected));

            OnRejected?.Invoke(this);
        }
    }
}
