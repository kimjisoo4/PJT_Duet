using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.GameplayTagSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class ArrowProjectile : BaseMonoBehaviour, ISpawnedActorByAbility
    {
        [Header(" [ Arrow Projectile ] ")]
        [SerializeField] private GameObject _actor;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private PooledObject _pooledObject;

        [Header(" Projectile ")]
        [SerializeField][Min(0f)] private float _startSpeed = 20f;
        [SerializeField][Min(0f)] private float _targetSpeed = 5f;
        [SerializeField][Min(0f)] private float _accelerationSpeed = 20f;
        [SerializeField][Min(0f)] private float _decelerationSpeed = 20f;

        [Header(" Trace ")]
        [SerializeField][Min(0f)] private float _traceRadius = 1f;
        [SerializeField] private Variable_LayerMask _traceLayer;

        [Header(" VFX ")]
        [SerializeField] private TrailRenderer _trail;

        [Header(" Life Time ")]
        [SerializeField] private float _lifeTime = 0.5f;

        private IGameplayTagSystem _gameplayTagSystem;

        private GameObject _owner;
        private IAbilitySpec _abilitySpec;
        private ITakeDamageAbility _takeDamageAbility;

        private IProjectile _projectile;
        private ISphereCast _sphereCast;
        private ITimer _timer;

        private bool _isPlaying = false;
        private bool _wasFinished = false;
        private float _remainTime;

        public IGameplayTagSystem GameplayTagSystem => _gameplayTagSystem;

        private void Awake()
        {
            _gameplayTagSystem = gameObject.GetGameplayTagSystem();

            _projectile = GetComponent<IProjectile>();
            _sphereCast = GetComponent<ISphereCast>();
            _timer = GetComponent<ITimer>();

            _timer.OnEndedTimer += _timer_OnEndedTimer;
        }
        private void OnDestroy()
        {
            if(_timer is not null)
            {
                _timer.OnEndedTimer += _timer_OnEndedTimer;
            }
        }
        private void OnDisable()
        {
            transform.localScale = Vector3.one;
        }

        public void Activate(GameObject newOwner, IAbilitySpec abilitySpec, IEnumerable<IGameplayTag> ownedTags = null)
        {
            _owner = newOwner;
            _abilitySpec = abilitySpec;

            _takeDamageAbility = _abilitySpec as ITakeDamageAbility;

            _gameplayTagSystem.AddOwnedTags(ownedTags);

            _actor.SetActive(true);
            _wasFinished = false;
        }

        public void Inactivate()
        {
            _isPlaying = false;
            _wasFinished = true;

            _owner = null;
            _takeDamageAbility = null;

            _gameplayTagSystem.ClearAllGameplayTags();

            _timer.EndTimer();
            _projectile.EndProjectile();
            _sphereCast.EndTrace();

            _actor.SetActive(false);

            _trail.emitting = false;
            _remainTime = _trail.time;
        }

        public void Play()
        {
            if (_isPlaying)
                return;

            _isPlaying = true;

            OnProjectile();
        }

        public void Finish()
        {
            if (!_isPlaying)
                return;

            Inactivate();
        }

        public void OnProjectile()
        {
            transform.parent = null;

            _projectile.StartSpeed = _startSpeed;
            _projectile.TargetSpeed = _targetSpeed;
            _projectile.Acceleration = _accelerationSpeed;
            _projectile.Deceleration = _decelerationSpeed;

            _projectile.OnProjectile();

            _sphereCast.TraceLayer = _traceLayer.Value;
            _sphereCast.TraceRadius = _traceRadius;
            _sphereCast.AddIgnoreTransform(_owner.transform);

            _sphereCast.OnTrace();

            _timer.OnTimer(_lifeTime);

            _trail.emitting = true;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            UpdateTimer(deltaTime);

            if(_wasFinished)
            {
                _remainTime -= deltaTime;

                if(_remainTime < 0f)
                {
                    _pooledObject.Release();
                }
            }
        }
        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            UpdateProjectile(deltaTime);
            UpdateTrace(deltaTime);
        }

        private void UpdateTimer(float deltaTime)
        {
            if (!_timer.IsPlaying)
                return;

            _timer.UpdateTimer(deltaTime);
        }

        private void UpdateProjectile(float deltaTime)
        {
            if (!_projectile.IsPlaying)
                return;

            var projectile = _projectile.UpdateProjectile(deltaTime);

            _rigidbody.MovePosition(_rigidbody.position + projectile.velocity);
            _rigidbody.MoveRotation(projectile.rotation);
        }

        private void UpdateTrace(float deltaTime)
        {
            if (!_sphereCast.IsPlaying)
                return;

            var hitResults = _sphereCast.UpdateTrace();

            if (hitResults.hitCount == 0)
                return;

            _takeDamageAbility.OnHit(this, hitResults.raycastHits, hitResults.hitCount);
        }

        private void _timer_OnEndedTimer(ITimer timer)
        {
            Finish();  
        }
    }
}
