using StudioScor.PlayerSystem;
using StudioScor.StateMachine;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Action/new Set Look Target", fileName = "Action_SetLookTarget")]
    public class SetLookTarget : Action
    {
        [Header(" [ Set Look Target] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private BlackBoardKey_Transform _targetKey;

        public override void EnterAction(StateMachineComponent stateMachine)
        {
            if (!_selfKey.TryGetValue(stateMachine, out IControllerSystem controller))
                return;


            if (!_targetKey.TryGetValue(stateMachine, out Transform target))
                return;

            controller.SetLookTarget(target);
        }

        public override void ExitAction(StateMachineComponent stateMachine)
        {
        }

        public override void UpdateAction(StateMachineComponent stateMachine)
        {
        }
    }
}