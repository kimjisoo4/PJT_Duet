using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class Mainmenu_SettingUIState : BaseUIFlowState
    {
        [Header(" [ Setting UI State ] ")]
        [SerializeField] private SettingController _settingController;

        [Header(" Next State ")]
        [SerializeField] private BaseUIFlowState _nextStateForSettingExit;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_settingController)
            {
                _settingController.OnSettingExit -= SettingController_OnSettingExit;
            }
        }
        protected override void EnterState()
        {
            base.EnterState();

            _settingController.OnSettingExit += SettingController_OnSettingExit;

            _settingController.Activate();
        }
        protected override void ExitState()
        {
            base.ExitState();
            
            if(_settingController)
            {
                _settingController.OnSettingExit -= SettingController_OnSettingExit;

                _settingController.Deactivate();
            }
        }

        private void SettingController_OnSettingExit(object sender, System.EventArgs e)
        {
            _settingController.OnDeactivateEnded += SettingController_OnDeactivateEnded;

            _settingController.Deactivate();
        }

        private void SettingController_OnDeactivateEnded(object sender, System.EventArgs e)
        {
            _settingController.OnDeactivateEnded -= SettingController_OnDeactivateEnded;

            UIFlowController.StateMachine.TrySetState(_nextStateForSettingExit);
        }
    }
}
