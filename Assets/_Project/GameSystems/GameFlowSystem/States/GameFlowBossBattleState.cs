using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [AddComponentMenu("Duet/GameFlow/State/Boss Battle State")]
    public class GameFlowBossBattleState : GameFlowState
    {
        [Header(" [ Boss Battle State ] ")]
        [SerializeField] private GameEvent _onBossRoomFinished;

        private void Awake()
        {
            _onBossRoomFinished.OnTriggerEvent += OnBossRoomFinished_OnTriggerEvent;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _onBossRoomFinished.OnTriggerEvent -= OnBossRoomFinished_OnTriggerEvent;
        }

        private void OnBossRoomFinished_OnTriggerEvent()
        {
            ComplateState();
        }
    }
}
