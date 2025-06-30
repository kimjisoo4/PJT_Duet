using UnityEngine;

namespace PF.PJT.Duet.UISystem
{

    public class Mainmenu_MenuUIState : BaseUIFlowState
    {
        [Header(" [ Menu UI State ] ")]
        [SerializeField] private MenuController _menuController;

        [Header(" Next State ")]
        [SerializeField] private BaseUIFlowState _nextStateForGameSetting;


        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_menuController)
            {
                _menuController.OnSettingOpen -= MenuController_OnSettingOpen;
            }
        }

        protected override void EnterState()
        {
            base.EnterState();

            _menuController.OnSettingOpen += MenuController_OnSettingOpen;
         
            _menuController.Activate();
        }

        protected override void ExitState()
        {
            base.ExitState();

            if(_menuController)
            {
                _menuController.OnSettingOpen -= MenuController_OnSettingOpen;

                _menuController.Deactivate();
            }
        }

        private void MenuController_OnSettingOpen(MenuController menuController)
        {
            UIFlowController.StateMachine.TrySetState(_nextStateForGameSetting);
        }
    }
}
