using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{
    public class SettingUI : BaseToolkitUI
    {
        public delegate void SettingUIEventHandler(SettingUI settingUI);

        [Header(" [ Setting UI ] ")]
        [SerializeField] private string _firstSelectElementName = "";

        [Header(" Buttons ")]
        [SerializeField] private GameObject _resetButtonActor;
        [SerializeField] private GameObject _applyButtonActor;
        [SerializeField] private GameObject _exitButtonActor;

        private VisualElement _firstSelectElement;

        private IUIToolkitSubmitEventListener _resetSubmit;
        private IUIToolkitCancelEventListener _resetCancel;

        private IUIToolkitSubmitEventListener _applySubmit;
        private IUIToolkitCancelEventListener _applyCancel;

        private IUIToolkitSubmitEventListener _exitSubmit;

        public event SettingUIEventHandler OnResetButtonPressed;
        public event SettingUIEventHandler OnApplyButtonPressed;
        public event SettingUIEventHandler OnExitButtonPressed;
        
        protected override void Awake()
        {
            base.Awake();

            _resetSubmit = _resetButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
            _resetCancel = _resetButtonActor.GetComponent<IUIToolkitCancelEventListener>();

            _applySubmit = _applyButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
            _applyCancel = _applyButtonActor.GetComponent<IUIToolkitCancelEventListener>();
            
            _exitSubmit = _exitButtonActor.GetComponent<IUIToolkitSubmitEventListener>();

            SetEnableBindings(false);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            RemoveAllBinding();

            OnResetButtonPressed = null;
            OnApplyButtonPressed = null;
            OnExitButtonPressed = null;
        }

        private void RemoveAllBinding()
        {
            if (_resetSubmit is not null)
                _resetSubmit.OnSubmited -= ResetSubmit_OnSubmited;

            if (_resetCancel is not null)
                _resetCancel.OnCanceled -= ResetCancel_OnCanceled;

            if (_applySubmit is not null)
                _applySubmit.OnSubmited -= ApplySubmit_OnSubmited;

            if (_applyCancel is not null)
                _applyCancel.OnCanceled -= ApplyCancel_OnCanceled;

            if (_exitSubmit is not null)
                _exitSubmit.OnSubmited -= ExitSubmit_OnSubmited;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _firstSelectElement = Root.Q<VisualElement>(_firstSelectElementName);
        }
        protected override void OnDisable()
        {
            base.OnDisable();

            _firstSelectElement = null;
        }

        protected override void OnActivateStart()
        {
            base.OnActivateStart();

            SetEnableBindings(true);
        }

        protected override void OnActivateEnd()
        {
            base.OnActivateEnd();

            _resetSubmit.OnSubmited += ResetSubmit_OnSubmited;
            _resetCancel.OnCanceled += ResetCancel_OnCanceled;
            _applySubmit.OnSubmited += ApplySubmit_OnSubmited;
            _applyCancel.OnCanceled += ApplyCancel_OnCanceled;
            _exitSubmit.OnSubmited += ExitSubmit_OnSubmited;

            _firstSelectElement.Focus();
        }

        protected override void OnDeactivateStart()
        {
            base.OnDeactivateStart();

            RemoveAllBinding();

            SetEnableBindings(false);
        }

        private void SetEnableBindings(bool enabled)
        {
            var bindings = GetComponentsInChildren<BaseBindingComponent>(true);

            for (int i = 0; i < bindings.Length; i++)
            {
                var binding = bindings[i];

                binding.enabled = enabled;
            }
        }

        public void PressResetButton()
        {
            Log(nameof(PressResetButton));

            Invoke_OnResetButtonPressed();
        }
        public void PressApplyButton()
        {
            Log(nameof(PressApplyButton));

            Invoke_OnApplyButtonPressed();
        }
        public void PressExitButton()
        {
            Log(nameof(PressExitButton));

            Invoke_OnExitButtonPressed();
        }

        public void FocusExitButton()
        {
            _exitSubmit.Target.Focus();
        }


        private void ResetSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            if(IsPlaying)
                PressResetButton();
        }
        private void ResetCancel_OnCanceled(IUIToolkitEventListener uitookiEventListener)
        {
            if (IsPlaying)
                FocusExitButton();
        }
        private void ApplySubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            if (IsPlaying)
                PressApplyButton();
        }
        private void ApplyCancel_OnCanceled(IUIToolkitEventListener uitookiEventListener)
        {
            if (IsPlaying)
                FocusExitButton();
        }

        private void ExitSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            if (IsPlaying)
                PressExitButton();
        }


        private void Invoke_OnResetButtonPressed()
        {
            Log(nameof(OnResetButtonPressed));

            OnResetButtonPressed?.Invoke(this);
        }   
        private void Invoke_OnApplyButtonPressed()
        {
            Log(nameof(OnApplyButtonPressed));

            OnApplyButtonPressed?.Invoke(this);
        }
        private void Invoke_OnExitButtonPressed()
        {
            Log(nameof(OnExitButtonPressed));

            OnExitButtonPressed?.Invoke(this);
        }
    }
}
