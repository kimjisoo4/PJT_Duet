using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class SettingController : BaseUIController
    {
        [Header(" [ Setting Controller ] ")]
        [SerializeField] private SettingSystem _settingSystem;
        [SerializeField] private SettingUI _settingUI;
        [SerializeField] private CheckSaveUI _checkSaveUI;

        public float SortingOrder => _settingUI.SortingOrder;

        public event EventHandler OnSettingExit;

        private void Awake()
        {
            _settingUI.OnActivateStarted += SettingUI_OnActivateStarted;
            _settingUI.OnActivateEnded += SettingUI_OnActivateEnded;
            _settingUI.OnDeactivateStarted += SettingUI_OnDeactivateStarted;
            _settingUI.OnDeactivateEnded += SettingUI_OnDeactivateEnded;
            _settingUI.OnResetButtonPressed += SettingUI_OnResetButtonPressed;
            _settingUI.OnApplyButtonPressed += SettingUI_OnApplyButtonPressed;
            _settingUI.OnExitButtonPressed += SettingUI_OnExitButtonPressed;

            _checkSaveUI.OnAccepted += CheckSaveUI_OnAccepted;
            _checkSaveUI.OnRejected += CheckSaveUI_OnRejected;
            _checkSaveUI.OnActivateEnded += CheckSaveUI_OnActivateEnded;
            _checkSaveUI.OnDeactivateEnded += CheckSaveUI_OnDeactivateEnded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_settingUI)
            {
                _settingUI.OnActivateStarted -= SettingUI_OnActivateStarted;
                _settingUI.OnActivateEnded -= SettingUI_OnActivateEnded;
                _settingUI.OnDeactivateStarted -= SettingUI_OnDeactivateStarted;
                _settingUI.OnDeactivateEnded -= SettingUI_OnDeactivateEnded;
                _settingUI.OnResetButtonPressed -= SettingUI_OnResetButtonPressed;
                _settingUI.OnApplyButtonPressed -= SettingUI_OnApplyButtonPressed;
                _settingUI.OnExitButtonPressed -= SettingUI_OnExitButtonPressed;
            }

            if(_checkSaveUI)
            {
                _checkSaveUI.OnAccepted -= CheckSaveUI_OnAccepted;
                _checkSaveUI.OnRejected -= CheckSaveUI_OnRejected;
                _checkSaveUI.OnActivateEnded -= CheckSaveUI_OnActivateEnded;
                _checkSaveUI.OnDeactivateEnded -= CheckSaveUI_OnDeactivateEnded;
            }

            OnSettingExit = null;
        }

        protected override void OnActivate()
        {
            _settingUI.Activate();

            FocusUI = _settingUI;
        }
        protected override void OnDeactivate()
        {
            _settingUI.Deactivate();
            _checkSaveUI.Deactivate();
        }
        public override void SetSortingOrder(float newOrder)
        {
            if (_settingUI.SortingOrder.SafeEquals(newOrder))
                return;

            var gap = _checkSaveUI.SortingOrder - _settingUI.SortingOrder;

            _settingUI.SetSortingOrder(newOrder);
            _checkSaveUI.SetSortingOrder(newOrder + gap);
        }

        public override void ResetSortingOrder()
        {
            _settingUI.ResetSortingOrder();
            _checkSaveUI.ResetSortingOrder();
        }

        public void OnApply()
        {
            Log(nameof(OnApply));

            _settingSystem.Save();
        }
        public void OnReset()
        {
            Log(nameof(OnReset));

            _settingSystem.ResetSetting();
        }
        public void OnExit()
        {
            Log(nameof(OnExit));

            RaiseOnSettingExit();
        }

        private void OpenCheckSave()
        {
            Log(nameof(OpenCheckSave));

            _settingUI.SetInteraction(false);

            FocusUI = null;

            _checkSaveUI.Activate();
        }
        private void CloseCheckSave()
        {
            Log(nameof(CloseCheckSave));

            _settingUI.SetInteraction(true);
            FocusUI = null;

            _checkSaveUI.Deactivate();
        }
        public void AcceptSetting()
        {
            Log(nameof(AcceptSetting));

            _settingSystem.Save();

            CloseCheckSave();

            OnExit();
        }
        public void RejectSetting()
        {
            Log(nameof(RejectSetting));

            _settingSystem.Load();
         
            CloseCheckSave();

            OnExit();
        }


        private void SettingUI_OnActivateStarted(object sender, EventArgs e)
        {
            RaiseOnActivateStarted();
        }
        private void SettingUI_OnActivateEnded(object sender, EventArgs e)
        {
            RaiseOnActivateEnded();
        }
        private void SettingUI_OnDeactivateStarted(object sender, EventArgs e)
        {
            RaiseOnDeactivateStarted();
        }
        private void SettingUI_OnDeactivateEnded(object sender, EventArgs e)
        {
            RaiseOnDeactivateEnded();
        }
        private void SettingUI_OnResetButtonPressed(SettingUI settingUI)
        {
            OnReset();
        }
        private void SettingUI_OnApplyButtonPressed(SettingUI settingUI)
        {
            OnApply();
        }
        private void SettingUI_OnExitButtonPressed(SettingUI settingUI)
        {
            if (_settingSystem.IsValueChanged)
            {
                OpenCheckSave();
            }
            else
            {
                OnExit();
            }
        }

        private void CheckSaveUI_OnActivateEnded(object sender, EventArgs e)
        {
            FocusUI = _checkSaveUI;
        }
        private void CheckSaveUI_OnDeactivateEnded(object sender, EventArgs e)
        {
            FocusUI = _settingUI;
        }
        private void CheckSaveUI_OnAccepted(CheckSaveUI checkSaveUI)
        {
            AcceptSetting();
        }
        private void CheckSaveUI_OnRejected(CheckSaveUI checkSaveUI)
        {
            RejectSetting();
        }


        private void RaiseOnSettingExit()
        {
            Log(nameof(OnSettingExit));

            OnSettingExit?.Invoke(this, EventArgs.Empty);
        }
    }
}
