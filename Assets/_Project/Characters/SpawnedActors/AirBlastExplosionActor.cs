using PF.PJT.Duet.Pawn.Effect;
using StudioScor.AbilitySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayEffectSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface IAirBlastExplosionActor
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }

        public void SetApplyGameplayEffects(GameplayEffect takeDamageEffect, GameplayEffect radialKnockbackEffect, GameplayEffect[] applyGameplayEffectToOther);
        public void OnExplosion(int chargeLevel);
    }

    public class AirBlastExplosionActor : BaseMonoBehaviour, ISpawnedActorByAbility
    {
        [Header(" [ Explosion Actor ] ")]
        [SerializeField] private PooledObject _pooledObject;

        [Header(" Trace ")]
        [SerializeField] private float[] _chargeTraceRadiuses;
        [SerializeField] private Variable_LayerMask _traceLayer;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue[] _explosionCues;

        private GameObject _owner;
        private IAbilitySpec _abilitySpec;
        private ITakeDamageAbility _spawnHitActorAbility;
        private IGameplayTagSystem _gameplayTagSystem;

        private IChargeable _chargeable;
        private ISphereCast _sphereCast;
        private ITimer _timer;
        private Cue _explosionCue;

        private bool _isPlaying = false;

        public IGameplayTagSystem GameplayTagSystem => _gameplayTagSystem;

        void Awake()
        {
            _gameplayTagSystem = gameObject.GetGameplayTagSystem();

            _chargeable = GetComponent<IChargeable>();
            _sphereCast = GetComponent<ISphereCast>();
            _timer = GetComponent<ITimer>();

            _timer.OnEndedTimer += _traceTimer_OnEndedTimer;
        }
        private void OnDestroy()
        {
            if(_timer is not null)
            {
                _timer.EndTimer();
                _timer.OnEndedTimer -= _traceTimer_OnEndedTimer;
                _timer = null;
            }

            if(_explosionCue is not null)
            {
                _explosionCue.OnEndedCue -= _explosionCue_OnEndedCue;
                _explosionCue = null;
            }
        }
        private void OnDisable()
        {
            if (_timer is not null)
            {
                _timer.EndTimer();
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

            _timer.UpdateTimer(deltaTime);
        }
        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

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
            _isPlaying = false;

            _owner = null;
            _abilitySpec = null;

            _gameplayTagSystem.ClearAllGameplayTags();

            _sphereCast.EndTrace();
            _timer.EndTimer();

            if (_explosionCue is null)
            {
                _pooledObject.Release();
                gameObject.SetActive(false);
            }
        }

        public void Play()
        {
            if (_isPlaying)
                return;

            _isPlaying = true;

            OnExplosion();
        }
        public void Finish()
        {
            if (!_isPlaying)
                return;

            Inactivate();
        }
        
        public void OnExplosion()
        {
            int level = _chargeable.CurrentChargeLevel;

            SetTraceRadius(level);

            _sphereCast.AddIgnoreTransform(_owner.transform);
            _sphereCast.TraceLayer = _traceLayer.Value;
            _sphereCast.OnTrace();
            
            _timer.OnTimer();

            PlayExplosionCue(level);
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

        private void UpdateTrace()
        {
            (int hitCount, RaycastHit[] hitResult) = _sphereCast.UpdateTrace();

            if (hitCount == 0)
                return;

            _spawnHitActorAbility.OnHit(this, hitResult, hitCount);
        }

        private void _traceTimer_OnEndedTimer(ITimer timer)
        {
            Finish();
        }

        private void _explosionCue_OnEndedCue(Cue cue)
        {
            if (!gameObject)
                return;

            _pooledObject.Release();
            gameObject.SetActive(false);

            _explosionCue = null;
        }
    }
}
