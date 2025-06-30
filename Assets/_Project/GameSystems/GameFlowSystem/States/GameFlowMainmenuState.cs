using PF.PJT.Duet.UISystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{

    [AddComponentMenu("Duet/GameFlow/State/Mainmenu State")]
    public class GameFlowMainmenuState : GameFlowState
    {
        [Header(" [ Mainmenu State ] ")]
        [SerializeField] private MainmenuUIFlowController _mainmenuController;

        protected override void EnterState()
        {
            base.EnterState();

            _mainmenuController.Activate();
        }
        protected override void ExitState()
        {
            base.ExitState();

            _mainmenuController.Deactivate();
        }
    }
}
