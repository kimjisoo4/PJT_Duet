using StudioScor.StateMachine;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{

    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Action/new Move To Target Action", fileName = "Action_MoveToTarget")]
    public class MoveToTargetAction : Action
    {
        [Header(" [ Move To Target Action ] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private BlackBoardKey_Transform _targetKey;

        public override void EnterAction(StateMachineComponent stateMachine)
        {
            
        }

        public override void ExitAction(StateMachineComponent stateMachine)
        {
            if (!_selfKey.TryGetValue(stateMachine, out var controllerSystem))
                return;

            controllerSystem.SetMoveDirection(controllerSystem.MoveDirection, 0f);
        }

        public override void UpdateAction(StateMachineComponent stateMachine)
        {
            if (!_selfKey.TryGetValue(stateMachine, out var controllerSystem))
                return;
            
            if (!_targetKey.TryGetValue(stateMachine, out var target))
                return;

            Vector3 direction = controllerSystem.Pawn.transform.Direction(target);
            float strength = Mathf.Max(1f, direction.magnitude);

            controllerSystem.SetMoveDirection(direction, strength);

        }
    }
}