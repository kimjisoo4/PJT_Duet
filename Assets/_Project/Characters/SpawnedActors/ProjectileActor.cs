using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface IProjectileActor
    {
        public void OnProjectile();
    }

    public class ProjectileActor : BaseMonoBehaviour, ISpawnedActorToAbility, IProjectileActor
    {
        [Header(" [ Air Blast Actor ] ")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Timer _lifeTimeTimer;

        [Header(" Explosion ")]
        [SerializeField] private SimplePoolContainer _explosionPool;

        [Header(" FX ")]
        [SerializeField] private FGameplayCue[] _projectileCues;

        [Header(" Trace ")]
        [SerializeField] private Variable_LayerMask _traceLayer;
        [SerializeField] private float[] _changeChargeLevelTraceRadius;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue[] _chargingCues;
        [SerializeField] private FGameplayCue[] _changeChargeLevelCues;

        [Header(" Gameplay Effects ")]
        [SerializeField] private GameplayEffect[] _addedApplyGameplayEffectsOnHitToOther;
        
        private GameplayEffect[] _applyGameplayEffectsOnHitToOther;

        public GameObject Owner { get; private set; }

        private IPawnSystem _pawnSystem;
        private IAbilitySpec _abilitySpec;
        private IProjectile _projectile;
        private IChargeable _chargeable;
        private ISphereCast _sphereCast;

        private Cue _projectileFX;
        private Cue _charingFX;

        private void Awake()
        {
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
            if(_projectile is not null)
            {
                _projectile.EndProjectile();
            }
            if(_chargeable is not null)
            {
                _chargeable.CancelCharging();
            }
            if (_sphereCast is not null)
            {
                _sphereCast.EndTrace();
            }
            if(_lifeTimeTimer is not null)
            {
                _lifeTimeTimer.CancelTimer();
            }
            if(_projectileFX is not null)
            {
                _projectileFX.Stop();
                _projectileFX = null;
            }
            if (_charingFX is not null)
            {
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

        public void SetOwner(GameObject newOwner, IAbilitySpec abilitySpec)
        {
            Owner = newOwner;
            _abilitySpec = abilitySpec;

            _pawnSystem = Owner.GetPawnSystem();
        }
        public void SetApplyGameplayEffectToOther(GameplayEffect[] gameplayEffects)
        {
            _addedApplyGameplayEffectsOnHitToOther = gameplayEffects;
        }


        public void OnProjectile()
        {
            _sphereCast.TraceLayer = _traceLayer.Value;
            _sphereCast.OnTrace();

            if (_projectileFX is not null)
            {
                _projectileFX.OnEndedCue += _projectileFX_OnEndedCue;
            }

            _lifeTimeTimer.OnTimer();
            _projectile.OnProjectile();
        }

        private void _chargeable_OnStartedCharge(IChargeable chargeable)
        {
            SetChangedChargeTraceRadius(0);
            PlayChangedChargeProjectileFX(0);
            PlayChangedChargeImpactFX(0);
            PlayChangedChargeChangingFX(0);
        }

        private void SetChangedChargeTraceRadius(int changeLevel)
        {
            float radius;

            if (changeLevel > _changeChargeLevelTraceRadius.LastIndex())
                radius = _changeChargeLevelTraceRadius.Last();
            else
                radius = _changeChargeLevelTraceRadius.ElementAt(changeLevel);

            _sphereCast.TraceRadius = radius;
        }
        private void PlayChangedChargeChangingFX(int chargeLevel)
        {
            if (_charingFX is not null)
            {
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
        private void PlayChangedChargeImpactFX(int chargeLevel)
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
        private void PlayChangedChargeProjectileFX(int chargeLevel)
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
        private void EndProjectile()
        {
            if (_projectileFX is not null)
            {
                _projectileFX.Stop();
            }

            _lifeTimeTimer.CancelTimer();
            _sphereCast.EndTrace();
        }
        private IEnumerator DelayedDisable()
        {
            yield return null;

            gameObject.SetActive(false);
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

            bool isHit = false;

            Vector3 direction = _sphereCast.StartPosition.Direction(_sphereCast.EndPosition);

            for (int i = 0; i < hitCount; i++)
            {
                var hit = hitResult[i];

                Log($"HIT :: {hit.transform.name}");

                if (hit.transform.TryGetPawnSystem(out IPawnSystem hitPawn) && hitPawn.Controller.CheckAffiliation(_pawnSystem.Controller) != EAffiliation.Hostile)
                    continue;

                isHit = true;

                if (_applyGameplayEffectsOnHitToOther is null || _applyGameplayEffectsOnHitToOther.Length == 0)
                    break;

                if (hit.transform.TryGetGameplayEffectSystem(out var hitEffectSystem))
                {
                    foreach (var effect in _applyGameplayEffectsOnHitToOther)
                    {
                        if (effect is TakeDamageEffect takeDamageEffect)
                        {
                            TakeDamageEffect.Element element = TakeDamageEffect.Element.Get(hit.point, hit.normal, hit.collider, direction, gameObject, Owner);

                            hitEffectSystem.TryTakeEffect(takeDamageEffect, Owner, _abilitySpec.Level, element);

                            element.Release();
                        }
                        else if (effect is TakeRadialKnockbackEffect takeRadialKnockbackEffect)
                        {
                            var radialData = new TakeRadialKnockbackEffect.FElement(transform.position);

                            if (hitEffectSystem.TryTakeEffect(takeRadialKnockbackEffect, gameObject, _abilitySpec.Level, radialData).isActivate)
                            {
                                isHit = true;
                            }
                        }
                        else
                        {
                            hitEffectSystem.TryTakeEffect(effect, gameObject, _abilitySpec.Level, null);
                        }
                    }
                }
            }

            if (!isHit)
                return;

            SpawnExplosion();

            EndProjectile();
        }

        private void SpawnExplosion()
        {
            var explosionActor = _explosionPool.Get();

            explosionActor.SetPositionAndRotation(transform.position, transform.rotation);
            explosionActor.gameObject.SetActive(true);

            var spawned = explosionActor.GetComponent<ISpawnedActorToAbility>();
            spawned.SetOwner(Owner, _abilitySpec);
            spawned.SetApplyGameplayEffectToOther(_addedApplyGameplayEffectsOnHitToOther);

            var spawnedChargeable = spawned.gameObject.GetComponent<IChargeable>();
            spawnedChargeable.SetChargeLevel(_chargeable.CurrentChargeLevel);

            var spawnedTrace = spawned.gameObject.GetComponent<IExplosionActor>();
            spawnedTrace.OnExplosion();
        }

        private void _chargeable_OnChangedChargeLevel(IChargeable chargeable, int currentLevel, int prevLevel)
        {
            if (prevLevel >= currentLevel)
                return;

            SetChangedChargeTraceRadius(currentLevel);
            PlayChangedChargeProjectileFX(currentLevel);
            PlayChangedChargeImpactFX(currentLevel);
            PlayChangedChargeChangingFX(currentLevel);
        }
        private void _chargeable_OnEndedCharge(IChargeable chargeable)
        {
            if (_charingFX is not null)
            {
                _charingFX.Stop();
                _charingFX = null;
            }
        }

        private void _timer_OnFinishedTimer(ITimer timer)
        {
            EndProjectile();
        }

        private void _projectileFX_OnEndedCue(Cue cue)
        {
            if(gameObject.activeInHierarchy)
                StartCoroutine(DelayedDisable());
        }
    }
}
