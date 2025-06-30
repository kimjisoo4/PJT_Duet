using StudioScor.BodySystem;
using StudioScor.GameplayTagSystem;
using StudioScor.MovementSystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace PF.PJT.Duet.Pawn
{
    public class OnFootstepData
    {
        public static OnFootstepData CreateFootstepData(Vector3 position, Vector3 normal)
        {
            if (_pool is null)
            {
                _pool = new ObjectPool<OnFootstepData>(Create);
            }

            var data = _pool.Get();

            data._position = position;
            data._normal = normal;

            return data;
        }

        private static OnFootstepData Create()
        {
            return new OnFootstepData();
        }
        public void Release()
        {
            _pool.Release(this);
        }

        private static IObjectPool<OnFootstepData> _pool;

        private Vector3 _position;
        private Vector3 _normal;
        public Vector3 Position => _position;
        public Vector3 Normal => _normal;
    }

    public class FootstepEventComponent : BaseMonoBehaviour
    {
        [Header(" [ Footstep Event Component ] ")]
        [SerializeField] private GameObject _owner;
        [SerializeField] private GameplayTag _footstepTriggerTag;

        [Space(5f)]
        [SerializeField] private float _footstepInterval = 0.2f;
        [SerializeField] private float _footstepSpeed = 1f;
        [SerializeField] private float _footstepHeight = 0.2f;

        private float _lastFootstepTime = -1f;
        private IGameplayTagSystem _gameplayTagSystem;
        private IBodySystem _bodySystem;
        private IMovementSystem _movementSystem;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!_owner)
            {
                if (gameObject.TryGetComponentInParentOrChildren(out IGameplayTagSystem gameplayTagSystem))
                {
                    _owner = gameplayTagSystem.gameObject;
                }
            }
#endif
        }

        private void Awake()
        {
            if (_owner)
            {
                _gameplayTagSystem = _owner.GetComponent<IGameplayTagSystem>();
            }
            else
            {
                if (gameObject.TryGetComponentInParentOrChildren(out _gameplayTagSystem))
                {
                    _owner = _gameplayTagSystem.gameObject;
                }
            }

            _bodySystem = _owner.GetBodySystem();
            _movementSystem = _owner.GetMovementSystem();
        }

        public void Footstep(Object data)
        {
            if (_movementSystem.PrevSpeed < _footstepSpeed)
                return;

            if (data is not BodyTag bodyTag)
                return;

            if (!_bodySystem.TryGetBodyPart(bodyTag, out IBodyPart bodypart))
                return;

            if (bodypart.transform.position.y > transform.position.y + _footstepHeight)
                return;

            float time = Time.time;

            if (time < _lastFootstepTime)
                return;

            _lastFootstepTime = time + _footstepInterval;

            Vector3 position = bodypart.transform.position;

            var footstepData = OnFootstepData.CreateFootstepData(position, _movementSystem.GroundNormal);

            _gameplayTagSystem.TriggerTag(_footstepTriggerTag, footstepData);

            footstepData.Release();
        }
    }
}
