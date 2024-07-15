using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using StudioScor.Utilities;
using UnityEngine;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class ControllerMoveToTarget : PlayerSystemAction
    {
        [Header(" [ Controller Move To Target ] ")]
        [SerializeField][RequiredField] private SharedTransform _targetKey;
        [SerializeField] private SharedFloat _reachDistance = 2f;
        [SerializeField] private SharedFloat _randDistance = 1f;

        private float _distance;
        public override void OnStart()
        {
            base.OnStart();

            _distance = _reachDistance.Value;

            float randDistance = _randDistance.Value;
            
            if(randDistance > 0f)
            {
                _distance += Random.Range(0f, randDistance);
            }
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
            float reachDistance = _reachDistance.Value;

            Vector3 direction = Pawn.transform.HorizontalDirection(target, false);
            float distance = direction.magnitude;

            if (distance <= reachDistance)
            {
                return TaskStatus.Success;
            }
            else
            {
                distance = Mathf.Clamp01(distance);
                direction.Normalize();

                ControllerSystem.SetMoveDirection(direction, distance);

                return TaskStatus.Running;
            }
        }
    }
}

