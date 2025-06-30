using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [AddComponentMenu("Duet/GameFlow/State/Battle State")]
    public class GameFlowBattleState : GameFlowState
    {
        [Header(" [ Battle In Room State  ] ")]
        [SerializeField] private GameEvent _onRoomFinished;

        private void Awake()
        {
            _onRoomFinished.OnTriggerEvent += OnRoomFinished_OnTriggerEvent;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            _onRoomFinished.OnTriggerEvent -= OnRoomFinished_OnTriggerEvent;
        }

        private void OnRoomFinished_OnTriggerEvent()
        {
            ComplateState();
        }
    }
}
