using StudioScor.StateMachine;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Action/new Move To Direction Action", fileName = "Action_MoveToDirection")]
    public class MoveToDirectionAction : Action
    {
        [Header(" [ Move To Direction Action ] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private bool _useRandDirection = false;
        [SerializeField][SCondition(nameof(_useRandDirection))] private Vector3 _direction = Vector3.forward;
        [SerializeField][Range(0f, 1f)] private float _strength = 1f;

        private Dictionary<StateMachineComponent, Vector3> _randDirection;

        public override void EnterAction(StateMachineComponent stateMachine)
        {
            if(_useRandDirection)
            {
                if (_randDirection is null)
                    _randDirection = new();

                Vector2 direction = Random.insideUnitCircle;

                _randDirection.Add(stateMachine, new Vector3(0, direction.y, direction.x));
            }
        }

        public override void ExitAction(StateMachineComponent stateMachine)
        {
            if(_useRandDirection && _randDirection is not null)
            {
                _randDirection.Remove(stateMachine);
            }

            if (!_selfKey.TryGetValue(stateMachine, out var controllerSystem))
                return;

            controllerSystem.SetMoveDirection(controllerSystem.MoveDirection, 0f);
        }

        public override void UpdateAction(StateMachineComponent stateMachine)
        {
            if (!_selfKey.TryGetValue(stateMachine, out var controllerSystem))
                return;

            Vector3 inputDirection = _useRandDirection ? _randDirection[stateMachine] : _direction;

            Vector3 direction = controllerSystem.Pawn.transform.TransformDirection(inputDirection);

            controllerSystem.SetMoveDirection(direction, _strength);

        }
    }
}