using StudioScor.AbilitySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public class ExplosiveArrowExplosionActor : BaseMonoBehaviour, ISpawnedActorByAbility
    {
        [Header(" [ Explosive Arrow Explosion Actor ] ")]
        [SerializeField] private PooledObject _pooledObject;

        [Header(" Trace ")]
        [SerializeField] private float _traceRadius = 2f;
        [SerializeField][Range(0f, 180f)] private float _traceAngle = 30f;
        [SerializeField] private Variable_LayerMask _traceLayer;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _explosionCue;

        [Header(" Life Time ")]
        [SerializeField][Min(0f)] private float _lifeTime = 2f;

        private IGameplayTagSystem _gameplayTagSystem;
        private GameObject _owner;
        private IAbilitySpec _abilitySpec;
        private ITakeDamageAbility _spawnHitActorAbility;

        private IOverlapSphere _overlapSphere;
        private ITimer _timer;
        private Cue _explosionFX;

        private bool _isPlaying = false;

        public IGameplayTagSystem GameplayTagSystem => _gameplayTagSystem;

        private void OnValidate()
        {
            if (TryGetComponent(out IOverlapSphere overlapSphere))
            { 
                overlapSphere.TraceRadius = _traceRadius;
            }
        }
        void Awake()
        {
            _gameplayTagSystem = gameObject.GetGameplayTagSystem();
            _overlapSphere = GetComponent<IOverlapSphere>();
            _timer = GetComponent<ITimer>();

            _timer.OnEndedTimer += _traceTimer_OnEndedTimer;
        }
        private void OnDestroy()
        {
            if (_timer is not null)
            {
                _timer.EndTimer();
                _timer.OnEndedTimer -= _traceTimer_OnEndedTimer;
                _timer = null;
            }

            if (_explosionFX is not null)
            {
                _explosionFX.OnEndedCue -= _explosionCue_OnEndedCue;
                _explosionFX = null;
            }
        }
        private void OnDisable()
        {
            if (_timer is not null)
            {
                _timer.EndTimer();
            }
            if (_overlapSphere is not null)
            {
                _overlapSphere.EndTrace();
            }
            if (_explosionFX is not null)
            {
                _explosionFX.Stop();
                _explosionFX = null;
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
            _overlapSphere.EndTrace();
            _timer.EndTimer();

            if (_explosionFX is null)
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
            _timer.OnTimer(_lifeTime);

            _overlapSphere.AddIgnoreTransform(_owner.transform);
            _overlapSphere.TraceLayer = _traceLayer.Value;
            _overlapSphere.TraceRadius = _traceRadius;
            _overlapSphere.OnTrace();

            _explosionFX = _explosionCue.PlayFromTarget(transform);
            _explosionFX.OnEndedCue += _explosionCue_OnEndedCue;
        }

        private void UpdateTrace()
        {
            (int hitCount, RaycastHit[] hitResult) = _overlapSphere.UpdateTrace();

            if (hitCount == 0)
                return;

            Log($"{nameof(UpdateTrace)} :: Hit Count - {hitCount}");

            int removeIndex = 0;

            for(int i = 0; i < hitCount; i++)
            {
                var hit = hitResult[i];

                float angle = transform.AngleOnForward(hit.point);

                if (!angle.InRange(-_traceAngle, _traceAngle))
                {
                    removeIndex++;
                }
                else if(removeIndex > 0)
                {
                    hitResult[i - removeIndex] = hit;
                }
            }

            hitCount -= removeIndex;

            _spawnHitActorAbility.OnHit(this, hitResult, hitCount);
        }

        private void _traceTimer_OnEndedTimer(ITimer timer)
        {
            Finish();
        }

        private void _explosionCue_OnEndedCue(Cue cue)
        {
            if (IsApplicationQuit)
                return;

            _pooledObject.Release();
            gameObject.SetActive(false);

            _explosionFX = null;
        }
    }
}
