using StudioScor.AbilitySystem;
using StudioScor.GameplayCueSystem;
using StudioScor.GameplayTagSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public class ArrowRainArea : BaseMonoBehaviour, ISpawnedActorByAbility
    {
        [Header(" [ Arrow Rain Area ] ")]
        [SerializeField] private PooledObject _pooledObject;
        [SerializeField] private GameObject _triggerAreaActor;

        [Header(" Life Time ")]
        [SerializeField][Min(0f)] private float _duration = 5f;

        [Header(" Gameplay Cue ")]
        [SerializeField] private FGameplayCue _areaLoopFX;

        private IGameplayTagSystem _gameplayTagSystem;
        private readonly ITimer _timer = new Timer();
        private ITriggerArea _triggerArea;

        private Cue _areaLoopCue;
        private GameObject _owner;
        private IAbilitySpec _abilitySpec;
        private IAreaAbility _areaAbility;

        public IGameplayTagSystem GameplayTagSystem => _gameplayTagSystem;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_pooledObject)
            {
                _pooledObject = GetComponent<PooledObject>();
            }

            if(!_triggerAreaActor)
            {
                if(gameObject.TryGetComponentInChildren(out ITriggerArea triggerArea))
                {
                    _triggerAreaActor = triggerArea.gameObject;
                }
            }
#endif
        }
        private void Awake()
        {
            _gameplayTagSystem = gameObject.GetGameplayTagSystem();

            _triggerArea = _triggerAreaActor.GetComponent<ITriggerArea>();

            _triggerArea.OnEnteredTrigger += _triggerArea_OnEnteredTrigger;
            _triggerArea.OnExitedTrigger += _triggerArea_OnExitedTrigger;

            _timer.OnEndedTimer += _timer_OnEndedTimer;
        }

        private void OnDestroy()
        {
            if(_triggerArea is not null)
            {
                _triggerArea.OnEnteredTrigger -= _triggerArea_OnEnteredTrigger;
                _triggerArea.OnExitedTrigger -= _triggerArea_OnExitedTrigger;

                _triggerArea = null;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            _timer.UpdateTimer(deltaTime);
        }

        public void Activate(GameObject newOwner, IAbilitySpec spawnAbility, IEnumerable<GameplayTag> ownedTags = null)
        {
            _owner = newOwner;
            _abilitySpec = spawnAbility;
            _areaAbility = _abilitySpec as IAreaAbility;

            _gameplayTagSystem.AddOwnedTags(ownedTags);
        }

        public void Play()
        {
            _timer.OnTimer(_duration);

            _areaLoopCue = _areaLoopFX.PlayAttached(transform);
            _areaLoopCue.OnEndedCue += _areaLoopCue_OnEndedCue;

            _triggerArea.gameObject.SetActive(true);
        }

        public void Inactivate()
        {
            _gameplayTagSystem.ClearAllGameplayTags();

            _timer.EndTimer();
            _areaLoopCue.Stop();
            _triggerArea.gameObject.SetActive(false);
        }

        private void EnterActor(GameObject enterActor)
        {
            if (!enterActor.TryGetActor(out IActor actor))
                return;

            if (actor.gameObject == _owner)
                return;

            _areaAbility.EnterArea(this, actor.gameObject);
        }
        private void ExitActor(GameObject exitActor)
        {
            if (!exitActor.TryGetActor(out IActor actor))
                return;

            if (actor.gameObject == _owner)
                return;

            _areaAbility.ExitArea(this, actor.gameObject);
        }

        private void _triggerArea_OnEnteredTrigger(ITriggerArea triggerArea, Collider collider)
        {
            EnterActor(collider.gameObject);
        }
        private void _triggerArea_OnExitedTrigger(ITriggerArea triggerArea, Collider collider)
        {
            ExitActor(collider.gameObject);
        }
        private void _timer_OnEndedTimer(ITimer timer)
        {
            Inactivate();
        }
        private void _areaLoopCue_OnEndedCue(Cue cue)
        {
            _pooledObject.Release();
        }
    }
}
