using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using StudioScor.Utilities;
using UnityEngine;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    public class ControllerMoveDirection : PlayerSystemAction
    {
        public enum EDirection
        {
            World,
            Local,
            Target,
        }

        [Header(" [ Controller Move Direction ] ")]
        [SerializeField] private SharedFloat _duration = 5f;
        [SerializeField] private SharedFloat _randDuration = 1f;

        [SerializeField] private SharedVector3 _direction = Vector3.forward;
        [SerializeField] private SharedFloat _strength = 1f;
        [SerializeField] private SharedFloat _randStrangth = 0.5f;
        [SerializeField] private EDirection _directionSpace = EDirection.Local;
        [SerializeField] private SharedTransform _targetKey;

        private bool _isForever;
        private float _remainTime = 0f;
        private float _moveStrength;

        public override void OnStart()
        {
            base.OnStart();

            _moveStrength = _strength.Value;

            float randStrength = _randStrangth.Value;

            if(randStrength > 0)
            {
                _moveStrength += Random.Range(0f, randStrength);
            }

            _moveStrength = Mathf.Clamp01(_moveStrength);

            _remainTime = _duration.Value;
            _isForever = _remainTime < 0;

            if(!_isForever)
            {
                float randDuration = _randDuration.Value;

                if(randDuration > 0)
                {
                    _remainTime += Random.Range(0f, randDuration);
                }
            }
        }
        public override void OnEnd()
        {
            base.OnEnd();

            ControllerSystem.SetMoveDirection(Vector3.zero, 0f);
        }
        public override TaskStatus OnUpdate()
        {
            Vector3 direction = _direction.Value;

            switch (_directionSpace)
            {
                case EDirection.World:
                    break;
                case EDirection.Local:
                    direction = direction.TurnDirectionFromY(Pawn.transform);
                    break;
                case EDirection.Target:
                    Quaternion rotation = Quaternion.LookRotation(Pawn.transform.HorizontalDirection(_targetKey.Value), Vector3.up);

                    direction = Quaternion.Euler(0, rotation.eulerAngles.y, 0) * direction;
                    break;
                default:
                    break;
            }

            ControllerSystem.SetMoveDirection(direction, _moveStrength);

            if (_isForever)
            {
                return TaskStatus.Running;
            }
            else
            {
                _remainTime -= Time.deltaTime;

                if (_remainTime <= 0f)
                    return TaskStatus.Success;
                else
                    return TaskStatus.Running;
            }
        }
    }
}

