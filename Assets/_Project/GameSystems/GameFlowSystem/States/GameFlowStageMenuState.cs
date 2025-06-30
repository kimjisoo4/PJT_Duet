using PF.PJT.Duet.UISystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [AddComponentMenu("Duet/GameFlow/State/Ingame Menu State")]
    public class GameFlowStageMenuState : GameFlowState
    {
        [Header(" [ Stage Menu State ] ")]
        [SerializeField] private StageMenuUIFlowController _stageMenuController;
        
        private void Awake()
        {
            _stageMenuController.OnContinue += StageMenuController_OnContinue;
            _stageMenuController.OnStageExit += StageMenuController_OnStageExit;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_stageMenuController)
            {
                _stageMenuController.OnContinue -= StageMenuController_OnContinue;
                _stageMenuController.OnStageExit -= StageMenuController_OnStageExit;
            }
        }
        protected override void EnterState()
        {
            base.EnterState();

            _stageMenuController.Activate();
        }

        protected override void ExitState()
        {
            base.ExitState();

            _stageMenuController.Deactivate();
        }

        private void StageMenuController_OnStageExit(StageMenuUIFlowController stageMenuController)
        {
            ComplateState();
        }

        private void StageMenuController_OnContinue(StageMenuUIFlowController stageMenuController)
        {
            ComplateState();
        }
    }
}
