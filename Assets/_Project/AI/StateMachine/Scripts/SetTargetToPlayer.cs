using StudioScor.PlayerSystem;
using StudioScor.StateMachine;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Action/new SetTargetToPlayer", fileName = "Action_SetTargetToPlayer")]
    public class SetTargetToPlayer : Action
    {
        [Header(" [ Set Target To Player ]")]
        [SerializeField] private BlackBoardKey_Transform _targetKey;
        [SerializeField] private PlayerManager _playerManager;

        public override void EnterAction(StateMachineComponent stateMachine)
        {
            if(_playerManager.HasPlayerPawn)
            {
                _targetKey.SetValue(stateMachine, _playerManager.PlayerPawn.transform);
            }
        }
        public override void UpdateAction(StateMachineComponent stateMachine)
        {
        }
        public override void ExitAction(StateMachineComponent stateMachine)
        {
        }
    }
}