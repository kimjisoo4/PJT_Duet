using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System;
using System.Linq;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface IExplosionActor
    {
        public void OnExplosion();
    }

    public class ExplosionActor : BaseMonoBehaviour, ISpawnedActorToAbility, IExplosionActor
    {
        [Header(" [ Explosion Actor ] ")]
        [SerializeField] private Timer _traceTimer;

        [Header(" Trace ")]
        [SerializeField] private float[] _chargeTraceRadiuses;
        [SerializeField] private Variable_LayerMask _traceLayer;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue[] _explosionCues;

        [Header(" Gameplay Effects ")]
        [SerializeField] private TakeDamageEffect[] _takeDamages;
        [SerializeField] private TakeRadialKnockbackEffect[] _takeRadialKnockbacks;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnHitToOther;
        [SerializeField] private GameplayEffect[] _applyGameplayEffectsOnSuccessedHit;

        public GameObject Owner { get; private set; }
        private IAbilitySpec _abilitySpec;
        private IPawnSystem _pawnSystem;

        private Cue _explosionCue;
        private ISphereCast _sphereCast;
        private IChargeable _chargeable;

        void Awake()
        {
            _sphereCast = GetComponent<ISphereCast>();
            _chargeable = GetComponent<IChargeable>();

            _traceTimer.OnEndedTimer += _traceTimer_OnEndedTimer;
            _sphereCast.OnStartedRaycast += _sphereCast_OnStartedRaycast;
        }
        private void OnDestroy()
        {
            if(_traceTimer is not null)
            {
                _traceTimer.EndTimer();
                _traceTimer.OnEndedTimer -= _traceTimer_OnEndedTimer;
            }
            
            if(_sphereCast is not null)
            {
                _sphereCast.OnStartedRaycast -= _sphereCast_OnStartedRaycast;
            }

            if(_explosionCue is not null)
            {
                _explosionCue.OnEndedCue -= _explosionCue_OnEndedCue;
            }
        }
        private void OnDisable()
        {
            if (_traceTimer is not null)
            {
                _traceTimer.EndTimer();
            }
            if (_sphereCast is not null)
            {
                _sphereCast.EndTrace();
            }
            if(_explosionCue is not null)
            {
                _explosionCue.Stop();
                _explosionCue = null;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _traceTimer.UpdateTimer(deltaTime);
        }
        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            UpdateTrace();
        }
        public void SetOwner(GameObject newOwner, IAbilitySpec abilitySpec)
        {
            Owner = newOwner;
            _abilitySpec = abilitySpec;

            _pawnSystem = Owner.GetPawnSystem();
        }
        public void OnExplosion()
        {
            _sphereCast.TraceLayer = _traceLayer.Value;
            int level = _chargeable.CurrentChargeLevel;

            SetTraceRadius(level);
            PlayExplosionCue(level);

            _traceTimer.OnTimer();
            _sphereCast.OnTrace();
        }

        private void SetTraceRadius(int chargeLevel)
        {
            float radius;

            if (chargeLevel > _chargeTraceRadiuses.LastIndex())
                radius = _chargeTraceRadiuses.Last();
            else
                radius = _chargeTraceRadiuses.ElementAt(chargeLevel);

            _sphereCast.TraceRadius = radius;
        }
        private void PlayExplosionCue(int chargeLevel)
        {
            FGameplayCue cue;

            if (chargeLevel > _explosionCues.LastIndex())
                cue = _explosionCues.Last();
            else
                cue = _explosionCues.ElementAt(chargeLevel);

            _explosionCue = cue.PlayFromTarget(transform);

            _explosionCue.OnEndedCue += _explosionCue_OnEndedCue;
        }

        private void _explosionCue_OnEndedCue(Cue cue)
        {
            gameObject.SetActive(false);

            _explosionCue = null;
        }

        private void UpdateTrace()
        {
            (int hitCount, RaycastHit[] hitResult) = _sphereCast.UpdateTrace();

            bool isHit = false;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = hitResult[i];

                Log($"HIT :: {hit.transform.name}");

                if (hit.transform.TryGetPawnSystem(out IPawnSystem hitPawn) && hitPawn.Controller.CheckAffiliation(_pawnSystem.Controller) != EAffiliation.Hostile)
                    continue;

                bool isEachHit = false;

                    Vector3 direction = transform.Direction(hit.point);

                if (hit.transform.TryGetGameplayEffectSystem(out var hitEffectSystem))
                {
                    if (_takeDamages is not null && _takeDamages.Length > 0)
                    {
                        TakeDamageEffect.FElement element = new TakeDamageEffect.FElement(hit.point, hit.normal, hit.collider, direction, gameObject, Owner);
                        var takeDamage = _takeDamages.ElementAtOrDefault(_chargeable.CurrentChargeLevel);

                        if (!takeDamage)
                        {
                            takeDamage = _takeDamages.Last();
                        }

                        if (hitEffectSystem.TryTakeEffect(takeDamage, Owner, _abilitySpec.Level, element).isActivate)
                        {
                            isHit = true;
                            isEachHit = true;
                        }
                    }
                    if (_takeRadialKnockbacks is not null && _takeRadialKnockbacks.Length > 0)
                    {
                        var radialData = new TakeRadialKnockbackEffect.FElement(transform.position);
                        var takeRadialKnockback = _takeRadialKnockbacks.ElementAtOrDefault(_chargeable.CurrentChargeLevel);

                        if (!takeRadialKnockback)
                            takeRadialKnockback = _takeRadialKnockbacks.Last();

                        if (hitEffectSystem.TryTakeEffect(takeRadialKnockback, gameObject, _abilitySpec.Level, radialData).isActivate)
                        {
                            isHit = true;
                            isEachHit = true;
                        }

                    }

                    for (int effectIndex = 0; effectIndex < _applyGameplayEffectsOnHitToOther.Length; effectIndex++)
                    {
                        var effect = _applyGameplayEffectsOnHitToOther[effectIndex];

                        if (hitEffectSystem.TryTakeEffect(effect, gameObject, _abilitySpec.Level, null).isActivate)
                        {
                            isHit = true;
                            isEachHit = true;
                        }

                    }

                    if (isEachHit)
                    {
                        // 각각 맞았을 때,
                    }
                }
            }

            if (isHit)
            {
                // 이번에 맞았을 때,
            }
        }
        private void _sphereCast_OnStartedRaycast(IRaycast raycast)
        {
           
        }
        private void _traceTimer_OnEndedTimer(ITimer timer)
        {
            _sphereCast.EndTrace();
        }
        
    }
}
