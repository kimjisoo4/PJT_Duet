using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{

    public class MenuUI : BaseToolkitUI
    {
        public delegate void MenuUIStateEventHandler(MenuUI menuUIController);

        [Header(" [ Menu UI ] ")]
        [SerializeField] private GameObject _gameStartButtonActor;
        [SerializeField] private GameObject _settingButtonActor;
        [SerializeField] private GameObject _exitButtonActor;

        private IUIToolkitSubmitEventListener _gameStartSubmit;
        private IUIToolkitCancelEventListener _gameStartCancel;
        private IUIToolkitSubmitEventListener _settingSubmit;
        private IUIToolkitCancelEventListener _settingCancel;
        private IUIToolkitSubmitEventListener _exitSubmit;

        public VisualElement GameStartElement => _gameStartSubmit.Target;
        public VisualElement SettingElement => _settingSubmit.Target;
        public VisualElement GameExitElemnt => _exitSubmit.Target;


        public event MenuUIStateEventHandler OnGameStartButtonPressed;
        public event MenuUIStateEventHandler OnSettingButtonPressed;
        public event MenuUIStateEventHandler OnExitButtonPressed;

        protected override void Awake()
        {
            base.Awake();

            _gameStartSubmit = _gameStartButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
            _gameStartCancel = _gameStartButtonActor.GetComponent<IUIToolkitCancelEventListener>();

            _settingSubmit = _settingButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
            _settingCancel = _settingButtonActor.GetComponent<IUIToolkitCancelEventListener>();

            _exitSubmit = _exitButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RemoveAllBinding();

            OnGameStartButtonPressed = null;
            OnSettingButtonPressed = null;
            OnExitButtonPressed = null;
        }

        private void RemoveAllBinding()
        {
            if (_gameStartSubmit is not null)
                _gameStartSubmit.OnSubmited -= GameStartSubmit_OnSubmited;
            if (_gameStartCancel is not null)
                _gameStartCancel.OnCanceled -= GameStartCancel_OnCanceled;

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

            _gameStartSubmit.OnSubmited += GameStartSubmit_OnSubmited;
            _gameStartCancel.OnCanceled += GameStartCancel_OnCanceled;

            _settingSubmit.OnSubmited += SettingSubmit_OnSubmited;
            _settingCancel.OnCanceled += SettingCancel_OnCanceled;

            _exitSubmit.OnSubmited += ExitSubmit_OnSubmited;

            _gameStartSubmit.Target.Focus();
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

        private void GameStartSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            PressGameStartButton();
        }
        private void GameStartCancel_OnCanceled(IUIToolkitEventListener uitookiEventListener)
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
            Log(nameof(OnGameStartButtonPressed));

            OnGameStartButtonPressed?.Invoke(this);
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
