using PF.PJT.Duet.UISystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    public class GameFlowGameoverState : GameFlowState
    {
        [Header(" [ Gameover State ] ")]
        [SerializeField] private ResultController _resultController;

        protected override void EnterState()
        {
            base.EnterState();

            _resultController.OpenGameoverUI();
        }
    }
}
