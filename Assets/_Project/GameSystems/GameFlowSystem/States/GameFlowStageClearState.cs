using PF.PJT.Duet.UISystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    public class GameFlowStageClearState : GameFlowState
    {
        [Header(" [ Stage Clear State ] ")]
        [SerializeField] private ResultController _resultController;

        protected override void EnterState()
        {
            base.EnterState();

            _resultController.OpenStageClearUI();
        }
    }
}
