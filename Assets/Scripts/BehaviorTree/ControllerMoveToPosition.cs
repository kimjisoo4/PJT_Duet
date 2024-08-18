using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using StudioScor.Utilities;
using UnityEngine;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class ControllerMoveToPosition : PlayerSystemAction
    {
        [Header(" [ Controller Move To Position ] ")]
        [SerializeField] private SharedVector3 _targetKey;
        [SerializeField] private SharedFloat _reachDistance = 2f;
        [SerializeField] private SharedFloat _randDistance = 1f;
        [SerializeField][Range(0f, 1f)] private SharedFloat _speedRate = 0.5f;
        [SerializeField][Range(0f, 1f)] private SharedFloat _randSpeedRate = 0.5f;

        private float _speed;
        private float _distance;
        public override void OnStart()
        {
            base.OnStart();

            _distance = _reachDistance.Value;

            float randDistance = _randDistance.Value;

            if (randDistance > 0f)
            {
                _distance += Random.Range(0f, randDistance);
            }

            _speed = _speedRate.Value;

            float randSpeed = _randSpeedRate.Value;

            if (randSpeed > 0f)
            {
                _speed += Random.Range(0f, randSpeed);
            }

            _speed = Mathf.Clamp01(_speed);
        }
        public override void OnEnd()
        {
            base.OnEnd();

            ControllerSystem.SetMoveDirection(Vector3.zero, 0f);
        }
        public override TaskStatus OnUpdate()
        {
            if (_targetKey.IsNone)
                return TaskStatus.Failure;

            if (!IsPossessed)
                return TaskStatus.Failure;

            var target = _targetKey.Value;

            Vector3 direction = Pawn.transform.HorizontalDirection(target, false);
            float distance = direction.magnitude;

            if (distance <= _distance)
            {
                return TaskStatus.Success;
            }
            else
            {
                distance = Mathf.Min(distance, _speed);
                direction.Normalize();

                ControllerSystem.SetMoveDirection(direction, distance);

                return TaskStatus.Running;
            }
        }
    }
}

