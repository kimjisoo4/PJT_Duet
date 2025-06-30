using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{


    [AddComponentMenu("Duet/GameFlow/State/Select Stage Reword State")]
    public class GameFlowSelectStageRewordState : GameFlowState
    {
        [Header(" [ Select Stage Reword State ] ")]
        [SerializeField] private StageRewordSystem _stageRewordSystem;
        [SerializeField] private int _selectRewordCount = 1;

        private int _remainCount;

        protected override void EnterState()
        {
            base.EnterState();

            _remainCount = _selectRewordCount;

            _stageRewordSystem.OnDeactivated += StageRewordSystem_OnDeactivated;
            _stageRewordSystem.OnSelectedReword += StageRewordSystem_OnSelectedReword;
            _stageRewordSystem.Init();
            _stageRewordSystem.Activate();
        }

        protected override void ExitState()
        {
            base.ExitState();

            if(_stageRewordSystem)
            {
                _stageRewordSystem.OnDeactivated -= StageRewordSystem_OnDeactivated;
            }
        }

        private void OnSelectedReword(ItemSO selectItem)
        {
            _remainCount--;

            if(_remainCount <= 0)
            {
                _stageRewordSystem.Deactivate();
            }
        }

        private void StageRewordSystem_OnDeactivated(StageRewordSystem stageRewordSystem)
        {
            ComplateState();
        }

        private void StageRewordSystem_OnSelectedReword(StageRewordSystem stageRewordSystem, ItemSO selectItem)
        {
            OnSelectedReword(selectItem);
        }
    }
}
