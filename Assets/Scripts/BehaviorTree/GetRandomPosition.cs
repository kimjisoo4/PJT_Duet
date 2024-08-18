using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class GetRandomPosition : Action
    {
        [Header(" [ Get Random Position ] ")]
        [SerializeField] private SharedTransform _self;

        [Header(" Position ")]
        [SerializeField] private bool _useNavMesh = true;
        [SerializeField] private SharedVector3 _origin;
        [SerializeField] private SharedTransform _target;
        [SerializeField] private SharedFloat _radius = 5f;
        [SerializeField] private SharedFloat _randRadius = 2f;

        [Header(" Result ")]
        [SerializeField][RequiredField] private SharedVector3 _result;

        public override TaskStatus OnUpdate()
        {
            Vector3 startPosition = _origin.Value;

            if(_target.IsNone)
            {
                startPosition = _target.Value.position;
            }

            Vector2 randPosition = Random.insideUnitCircle;
            float distance = _radius.Value;

            if (_randRadius.Value > 0)
            {
                distance += Random.Range(0f, _randRadius.Value);
            }

            randPosition = randPosition * distance;

            Vector3 position = startPosition + new Vector3(randPosition.x, 0, randPosition.y);

            if(_useNavMesh)
            {
                if(NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas))
                {
                    position = hit.position;
                }
                else
                {
                    return TaskStatus.Failure;
                }
            }

            _result.SetValue(position);

            return TaskStatus.Success;
        }
    }
}

