using System;
using UnityEngine;

namespace PF.PJT.Duet.UISystem
{

    public class StageMenuUIFlowController : BaseUIFlowController
    {
        public delegate void StageMenuControllerEventHandler(StageMenuUIFlowController stageMenuController);

        [Header(" [ Stage Menu Controller ] ")]
        [SerializeField] private IngameMenuController _ingameMenuController;
        [SerializeField] private SettingController _settingController;

        [Header(" States ")]
        [SerializeField] private BaseUIFlowState _ingameMenuState;
        [SerializeField] private BaseUIFlowState _settingUIState;

        public event StageMenuControllerEventHandler OnContinue;
        public event StageMenuControllerEventHandler OnStageExit;

        private void Awake()
        {
            _ingameMenuController.OnContinue += MenuUIController_OnContinue;
            _ingameMenuController.OnGameExit += MenuUIController_OnGameExit;
        }

        protected override void OnDestory()
        {
            base.OnDestory();

            if (_ingameMenuController)
            {
                _ingameMenuController.OnContinue -= MenuUIController_OnContinue;
                _ingameMenuController.OnGameExit -= MenuUIController_OnGameExit;
            }

            OnContinue = null;
            OnStageExit = null;
        }

        public void Continue()
        {
            Log(nameof(Continue));

            RaiseOnContinue();
        }
        public void ExitGame()
        {
            Log(nameof(ExitGame));

            RaiseOnStageExit();
        }


        // Event Binding
        private void MenuUIController_OnContinue(IngameMenuController ingameMenuController)
        {
            Continue();
        }
        private void MenuUIController_OnGameExit(IngameMenuController ingameMenuController)
        {
            ExitGame();
        }

        // Event Invoker
        private void RaiseOnContinue()
        {
            Log(nameof(OnContinue));

            OnContinue?.Invoke(this);
        }
        private void RaiseOnStageExit()
        {
            Log(nameof(OnStageExit));

            OnStageExit?.Invoke(this);
        }
    }
}
