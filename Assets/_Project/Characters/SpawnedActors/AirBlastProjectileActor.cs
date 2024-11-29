using StudioScor.AbilitySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public class AirBlastProjectileActor : BaseMonoBehaviour, ISpawnedActorByAbility
    {
        [Header(" [ Air Blast Actor ] ")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private PooledObject _pooledObject;
        [SerializeField] private Timer _lifeTimeTimer;

        [Header(" FX ")]
        [SerializeField] private FGameplayCue[] _projectileCues;

        [Header(" Trace ")]
        [SerializeField] private Variable_LayerMask _traceLayer;
        [SerializeField] private float[] _changeChargeLevelTraceRadius;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue[] _chargingCues;
        [SerializeField] private FGameplayCue[] _changeChargeLevelCues;

        private GameObject _owner;
        private IAbilitySpec _abilitySpec;
        private ITakeDamageAbility _spawnHitActorAbility;
        private IGameplayTagSystem _gameplayTagSystem;

        private IProjectile _projectile;
        private IChargeable _chargeable;
        private ISphereCast _sphereCast;

        private Cue _projectileFX;
        private Cue _charingFX;

        private bool _isPlaying = false;

        public IGameplayTagSystem GameplayTagSystem => _gameplayTagSystem;

        private void Awake()
        {
            _gameplayTagSystem = gameObject.GetGameplayTagSystem();
            _projectile = GetComponent<IProjectile>();
            _chargeable = GetComponent<IChargeable>();
            _sphereCast = GetComponent<ISphereCast>();

            _chargeable.OnStartedCharge += _chargeable_OnStartedCharge;
            _chargeable.OnEndedCharge += _chargeable_OnEndedCharge;
            _chargeable.OnChangedChargeLevel += _chargeable_OnChangedChargeLevel;

            _lifeTimeTimer.OnFinishedTimer += _timer_OnFinishedTimer;
        }

        private void OnDestroy()
        {
            if (_chargeable is not null)
            {
                _chargeable.OnStartedCharge -= _chargeable_OnStartedCharge;
                _chargeable.OnChangedChargeLevel -= _chargeable_OnChangedChargeLevel;
            }

            if (_lifeTimeTimer is not null)
            {
                _lifeTimeTimer.OnFinishedTimer -= _timer_OnFinishedTimer;
            }
        }

        private void OnDisable()
        {
            if (_projectile is not null)
            {
                _projectile.EndProjectile();
            }
            if (_chargeable is not null)
            {
                _chargeable.CancelCharging();
            }
            if (_sphereCast is not null)
            {
                _sphereCast.EndTrace();
            }
            if (_lifeTimeTimer is not null)
            {
                _lifeTimeTimer.CancelTimer();
            }
            if (_projectileFX is not null)
            {
                _projectileFX.Detach();
                _projectileFX.Stop();
                _projectileFX = null;
            }
            if (_charingFX is not null)
            {
                _charingFX.Detach();
                _charingFX.Stop();
                _charingFX = null;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _lifeTimeTimer.UpdateTimer(deltaTime);
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            UpdateProjectile(deltaTime);
            UpdateTrace();
        }

        public void Activate(GameObject newOwner, IAbilitySpec abilitySpec, IEnumerable<GameplayTag> ownedTags = null)
        {
            _owner = newOwner;
            _abilitySpec = abilitySpec;
            _spawnHitActorAbility = _abilitySpec as ITakeDamageAbility;

            _gameplayTagSystem.AddOwnedTags(ownedTags);
        }
        public void Inactivate()
        {
            Log($"{nameof(Inactivate)}");

            _isPlaying = false;

            _owner = null;
            _spawnHitActorAbility = null;

            _chargeable.FinishCharging();
            _gameplayTagSystem.ClearAllGameplayTags();
            _lifeTimeTimer.CancelTimer();
            _sphereCast.EndTrace();

            if(_projectileFX is not null)
            {
                _projectileFX.Detach();
                _projectileFX.Stop();
                _projectileFX = null;
            }
            if(_charingFX is not null)
            {
                _charingFX.Detach();
                _charingFX.Stop();
                _charingFX = null;
            }

            _pooledObject.Release();
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
            transform.SetParent(null, true);

            _sphereCast.AddIgnoreTransform(_owner.transform);
            _sphereCast.TraceLayer = _traceLayer.Value;
            _sphereCast.OnTrace();

            _lifeTimeTimer.OnTimer();
            _projectile.OnProjectile();
        }

        private void OnChangedChargeTraceRadius(int changeLevel)
        {
            float radius;

            if (changeLevel > _changeChargeLevelTraceRadius.LastIndex())
                radius = _changeChargeLevelTraceRadius.Last();
            else
                radius = _changeChargeLevelTraceRadius.ElementAt(changeLevel);

            _sphereCast.TraceRadius = radius;
        }
        private void OnChangedChargeChangingFX(int chargeLevel)
        {
            if (_charingFX is not null)
            {
                _charingFX.Detach();
                _charingFX.Stop();
                _charingFX = null;
            }

            FGameplayCue targetCue;

            if (chargeLevel > _chargingCues.LastIndex())
                targetCue = _chargingCues.Last();
            else
                targetCue = _chargingCues.ElementAt(chargeLevel);

            if (!targetCue.Cue)
                return;

            _charingFX = targetCue.PlayAttached(transform);
        }
        private void OnChangedChargeImpactFX(int chargeLevel)
        {
            FGameplayCue targetCue;

            if (chargeLevel > _changeChargeLevelCues.LastIndex())
                targetCue = _changeChargeLevelCues.Last();
            else
                targetCue = _changeChargeLevelCues.ElementAt(chargeLevel);

            if (!targetCue.Cue)
                return;

            targetCue.PlayFromTarget(transform);
        }
        private void OnChangedChargeProjectileFX(int chargeLevel)
        {
            if (_projectileFX is not null)
            {
                _projectileFX.Stop();
                _projectileFX = null;
            }

            FGameplayCue targetCue;

            if (chargeLevel > _projectileCues.LastIndex())
                targetCue = _projectileCues.Last();
            else
                targetCue = _projectileCues.ElementAt(chargeLevel);

            if (!targetCue.Cue)
                return;

            _projectileFX = targetCue.PlayAttached(transform);
        }
        

        private void UpdateProjectile(float deltaTime)
        {
            if (!_projectile.IsPlaying)
                return;

            var projectile = _projectile.UpdateProjectile(deltaTime);

            _rigidbody.MovePosition(_rigidbody.position + projectile.velocity);
            _rigidbody.MoveRotation(projectile.rotation);
        }
        
        private void UpdateTrace()
        {
            if (!_sphereCast.IsPlaying)
                return;

            (int hitCount, RaycastHit[] hitResult) = _sphereCast.UpdateTrace();

            if (hitCount == 0)
                return;

            _spawnHitActorAbility.OnHit(this, hitResult, hitCount);
        }
        
        private void _chargeable_OnStartedCharge(IChargeable chargeable)
        {
            OnChangedChargeTraceRadius(0);
            OnChangedChargeProjectileFX(0);
            OnChangedChargeImpactFX(0);
            OnChangedChargeChangingFX(0);
        }
        private void _chargeable_OnChangedChargeLevel(IChargeable chargeable, int currentLevel, int prevLevel)
        {
            if (prevLevel >= currentLevel)
                return;

            OnChangedChargeTraceRadius(currentLevel);
            OnChangedChargeProjectileFX(currentLevel);
            OnChangedChargeImpactFX(currentLevel);
            OnChangedChargeChangingFX(currentLevel);
        }
        private void _chargeable_OnEndedCharge(IChargeable chargeable)
        {
            if (_charingFX is not null)
            {
                _charingFX.Detach();
                _charingFX.Stop();
                _charingFX = null;
            }
        }

        private void _timer_OnFinishedTimer(ITimer timer)
        {
            Finish();
        }

        private void _projectileFX_OnEndedCue(Cue cue)
        {/*
            if( gameObject.activeInHierarchy)
            {
                _pooledObject.Release();
            }
*/
        }
        private void _charingFX_OnEndedCue(Cue cue)
        {/*
            if (_projectileFX is null)
            {
                _pooledObject.Release();
            }*/
        }
    }
}
