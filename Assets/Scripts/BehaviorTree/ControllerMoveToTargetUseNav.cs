using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.AI;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class ControllerMoveToTargetUseNav : PlayerSystemAction
    {
        [Header(" [ Controller Move To Target Use Nav] ")]
        [SerializeField][RequiredField] private SharedTransform _targetKey;
        [SerializeField] private SharedFloat _reachDistance = 2f;
        [SerializeField] private SharedFloat _randDistance = 1f;

        [SerializeField] private SharedFloat _navUpdateInterval = 0.5f;

        private float _distance;
        private float _remainNavInterval;
        private int _pathIndex = 0;
        private NavMeshAgent _navmeshAgent;
        private readonly NavMeshPath _path = new();

        public override void OnStart()
        {
            base.OnStart();

            if (!IsPossessed)
                return;

            _navmeshAgent = Pawn.gameObject.GetComponent<NavMeshAgent>();

            _distance = _reachDistance.Value;
            _remainNavInterval = -1f;

            float randDistance = _randDistance.Value;

            if (randDistance > 0f)
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

            if(_remainNavInterval <= 0f)
            {/*
                Debug.Log(" Update Path Interval ");
*/
                _remainNavInterval = _navUpdateInterval.Value;
                _pathIndex = 1;
                NavMesh.CalculatePath(Pawn.transform.position, target.position, _navmeshAgent.areaMask, _path);
            }
            else
            {
                _remainNavInterval -= Time.deltaTime;
            }

            Vector3 direction;
/*
            Debug.Log($"Path State - {_path.status} || Path Length - {_path.corners.Length}");
*/
            if (_path.status != NavMeshPathStatus.PathComplete || _path.corners.Length <= 2)
            {
                direction = Pawn.transform.HorizontalDirection(target, false);
            }
            else
            {
                Vector3 targetPosition = _path.corners[_pathIndex];

                if (Pawn.transform.HorizontalDistance(targetPosition) <= 0.1f)
                {
                    _pathIndex++;

                    targetPosition = _path.corners[_pathIndex];
                }

                direction = Pawn.transform.HorizontalDirection(targetPosition, false);
            }

            float distance = Pawn.transform.HorizontalDistance(target.transform.position);

            if (distance <= _distance)
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

