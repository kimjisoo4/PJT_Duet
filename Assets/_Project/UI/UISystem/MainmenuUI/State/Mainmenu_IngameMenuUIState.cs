using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class Mainmenu_IngameMenuUIState : BaseUIFlowState
    {
        [Header(" [ Ingame Menu UI State ] ")]
        [SerializeField] private IngameMenuController _ingameMenuController;

        [Header(" Next State ")]
        [SerializeField] private BaseUIFlowState _nextStateForGameSetting;

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_ingameMenuController)
            {
                _ingameMenuController.OnSettingOpen -= IngameMenuController_OnSettingOpen;
            }
        }

        protected override void EnterState()
        {
            base.EnterState();
            
            _ingameMenuController.OnSettingOpen += IngameMenuController_OnSettingOpen;

            if (UIFlowController.StateMachine.PrevState == _nextStateForGameSetting)
            {
                _ingameMenuController.SetInteraction(true);
            }
            else
            {
                _ingameMenuController.Activate();
            }
        }

        protected override void ExitState()
        {
            base.ExitState();

            if (UIFlowController.StateMachine.NextState == _nextStateForGameSetting)
            {
                _ingameMenuController.SetInteraction(false);
            }
            else
            {
                _ingameMenuController.Deactivate();
            }
        }

        private void IngameMenuController_OnSettingOpen(IngameMenuController ingameMenuController)
        {
            UIFlowController.StateMachine.TrySetState(_nextStateForGameSetting);
        }
    }
}
