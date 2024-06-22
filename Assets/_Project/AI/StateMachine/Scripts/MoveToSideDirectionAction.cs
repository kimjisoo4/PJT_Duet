using StudioScor.StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Action/new Move To Side Direction Action", fileName = "Action_MoveToSideDirection")]
    public class MoveToSideDirectionAction : Action
    {
        public enum EDirection
        {
            Right,
            Left,
            Random,
        }

        [Header(" [ Move To Direction Action ] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private EDirection _direction;
        [SerializeField][Range(0f, 1f)] private float _strength = 1f;

        private Dictionary<StateMachineComponent, bool> _randDirections;
        protected override void OnReset()
        {
            base.OnReset();

            _randDirections = null;
        }

        public override void EnterAction(StateMachineComponent stateMachine)
        {
            if(_direction == EDirection.Random)
            {
                if (_randDirections is null)
                    _randDirections = new();

                bool isRight = Random.Range(0, 2) == 0;

                _randDirections.Add(stateMachine, isRight);
            }
        }

        public override void ExitAction(StateMachineComponent stateMachine)
        {
            if (_direction == EDirection.Random && _randDirections is not null)
            {
                _randDirections.Remove(stateMachine);
            }

            if (!_selfKey.TryGetValue(stateMachine, out var controllerSystem))
                return;

            controllerSystem.SetMoveDirection(controllerSystem.MoveDirection, 0f);
        }

        public override void UpdateAction(StateMachineComponent stateMachine)
        {
            if (!_selfKey.TryGetValue(stateMachine, out var controllerSystem))
                return;

            Vector3 sideDirection = Vector3.right;

            switch (_direction)
            {
                case EDirection.Right:
                    sideDirection = Vector3.right;
                    break;
                case EDirection.Left:
                    sideDirection = Vector3.left;
                    break;
                case EDirection.Random:
                    sideDirection = _randDirections[stateMachine] ? Vector3.right : Vector3.left;
                    break;
                default:
                    break;
            }

            Vector3 direction = controllerSystem.Pawn.transform.TransformDirection(sideDirection);

            controllerSystem.SetMoveDirection(direction, _strength);

        }
    }
}