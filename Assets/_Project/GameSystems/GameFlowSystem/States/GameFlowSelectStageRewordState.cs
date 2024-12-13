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

            _stageRewordSystem.OnInactivated += _stageRewordSystem_OnInactivated;
            _stageRewordSystem.OnSelectedReword += _stageRewordSystem_OnSelectedReword;
            _stageRewordSystem.Init();
            _stageRewordSystem.Activate();
        }

        protected override void ExitState()
        {
            base.ExitState();

            if(_stageRewordSystem)
            {
                _stageRewordSystem.OnInactivated -= _stageRewordSystem_OnInactivated;
            }
        }

        private void OnSelectedReword(ItemSO selectItem)
        {
            _remainCount--;

            if(_remainCount <= 0)
            {
                _stageRewordSystem.Inactivate();
            }
        }

        private void _stageRewordSystem_OnInactivated(StageRewordSystem stageRewordSystem)
        {
            TryNextState();
        }

        private void _stageRewordSystem_OnSelectedReword(StageRewordSystem stageRewordSystem, ItemSO selectItem)
        {
            OnSelectedReword(selectItem);
        }
    }
}
