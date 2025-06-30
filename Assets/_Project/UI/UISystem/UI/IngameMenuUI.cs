using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{
    public class IngameMenuUI : BaseToolkitUI
    {
        public delegate void IngameMenuUIStateEventHandler(IngameMenuUI ingameMenuUI);

        [Header(" [ Ingame Menu UI ] ")]
        [SerializeField] private GameObject _continueButtonActor;
        [SerializeField] private GameObject _settingButtonActor;
        [SerializeField] private GameObject _exitButtonActor;

        private IUIToolkitSubmitEventListener _continueSubmit;
        private IUIToolkitCancelEventListener _continueCancel;
        private IUIToolkitSubmitEventListener _settingSubmit;
        private IUIToolkitCancelEventListener _settingCancel;
        private IUIToolkitSubmitEventListener _exitSubmit;

        public VisualElement GameStartElement => _continueSubmit.Target;
        public VisualElement SettingElement => _settingSubmit.Target;
        public VisualElement GameExitElemnt => _exitSubmit.Target;


        public event IngameMenuUIStateEventHandler OnContinueButtonPress;
        public event IngameMenuUIStateEventHandler OnSettingButtonPressed;
        public event IngameMenuUIStateEventHandler OnExitButtonPressed;

        protected override void Awake()
        {
            base.Awake();

            _continueSubmit = _continueButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
            _continueCancel = _continueButtonActor.GetComponent<IUIToolkitCancelEventListener>();

            _settingSubmit = _settingButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
            _settingCancel = _settingButtonActor.GetComponent<IUIToolkitCancelEventListener>();

            _exitSubmit = _exitButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RemoveAllBinding();

            OnContinueButtonPress = null;
            OnSettingButtonPressed = null;
            OnExitButtonPressed = null;
        }

        private void RemoveAllBinding()
        {
            if (_continueSubmit is not null)
                _continueSubmit.OnSubmited -= ContinueSubmit_OnSubmited;
            if (_continueCancel is not null)
                _continueCancel.OnCanceled -= ContinueCancel_OnCanceled;

            if (_settingSubmit is not null)
                _settingSubmit.OnSubmited -= SettingSubmit_OnSubmited;
            if (_settingCancel is not null)
                _settingCancel.OnCanceled -= SettingCancel_OnCanceled;

            if (_exitSubmit is not null)
                _exitSubmit.OnSubmited -= ExitSubmit_OnSubmited;
        }

        protected override void OnActivateEnd()
        {
            base.OnActivateEnd();

            _continueSubmit.OnSubmited += ContinueSubmit_OnSubmited;
            _continueCancel.OnCanceled += ContinueCancel_OnCanceled;

            _settingSubmit.OnSubmited += SettingSubmit_OnSubmited;
            _settingCancel.OnCanceled += SettingCancel_OnCanceled;

            _exitSubmit.OnSubmited += ExitSubmit_OnSubmited;

            _continueSubmit.Target.Focus();
        }
        protected override void OnDeactivateStart()
        {
            base.OnDeactivateStart();

            RemoveAllBinding();
        }

        public void PressGameStartButton()
        {
            Log(nameof(PressGameStartButton));

            Invoke_OnGameStartButtonPressed();
        }
        public void PressSettingButton()
        {
            Log(nameof(PressSettingButton));

            Invoke_OnSettingButtonPressed();
        }
        public void PressExitButton()
        {
            Log(nameof(PressExitButton));

            Invoke_OnExitButtonPressed();
        }

        private void ContinueSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            PressGameStartButton();
        }
        private void ContinueCancel_OnCanceled(IUIToolkitEventListener uitookiEventListener)
        {
            _exitSubmit.Target.Focus();
        }

        private void SettingSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            PressSettingButton();
        }
        private void SettingCancel_OnCanceled(IUIToolkitEventListener uitookiEventListener)
        {
            _exitSubmit.Target.Focus();
        }

        private void ExitSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            PressExitButton();
        }



        private void Invoke_OnGameStartButtonPressed()
        {
            Log(nameof(OnContinueButtonPress));

            OnContinueButtonPress?.Invoke(this);
        }
        private void Invoke_OnExitButtonPressed()
        {
            Log(nameof(OnExitButtonPressed));

            OnExitButtonPressed?.Invoke(this);
        }
        private void Invoke_OnSettingButtonPressed()
        {
            Log(nameof(OnSettingButtonPressed));

            OnSettingButtonPressed?.Invoke(this);
        }
    }
}
