using System.Collections.Generic;
using UnityEngine;

namespace StudioScor.Utilities
{
    public interface IAnimNotifyStateEvent
    {
        public void Enter(AnimNotifyComponent animNotifyComponent);
        public void Exit(AnimNotifyComponent animNotifyComponent);
    }
    public interface IAnimNotifyEvent
    {
        public void Enter(AnimNotifyComponent animNotifyComponent);
    }

    public abstract class AnimNotifyEvent : BaseScriptableObject, IAnimNotifyEvent
    {
        public abstract void Enter(AnimNotifyComponent animNotifyComponent);
    }
    public abstract class AnimNotifyStateEvent : BaseScriptableObject, IAnimNotifyStateEvent
    {
        public abstract void Enter(AnimNotifyComponent animNotifyComponent);
        public abstract void Exit(AnimNotifyComponent animNotifyComponent);
    }
    [RequireComponent(typeof(Animator))]
    public class AnimNotifyComponent : BaseMonoBehaviour
    {
        [Header(" [ Anim Notify Component ] ")]
        [SerializeField] private GameObject _owner;
        [SerializeField] private Animator _animator;

        public GameObject Owner => _owner;
        public Animator Animator => _animator;
        public GameObject Model => _animator.gameObject;

        private int _animationID;
        private float _offset;
        private int _eventCount;

        private readonly List<IAnimNotifyStateEvent> _animNotifyStates = new();

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_animator)
                _animator = gameObject.GetComponent<Animator>();
#endif
        }

        private void Start()
        {
            var currentState = _animator.GetCurrentAnimatorStateInfo(0);

            _animationID = currentState.shortNameHash;
        }
        private void Update()
        {
            var currentState = _animator.GetCurrentAnimatorStateInfo(0);
            var nextState = _animator.GetNextAnimatorStateInfo(0);

            if (nextState.shortNameHash != 0 && _animationID != nextState.shortNameHash)
            {
                _animationID = nextState.shortNameHash;
                _offset = 0f;

                ClearAllNotifyState();

                _eventCount = 0;
            }
            else if(!Animator.IsInTransition(0) && currentState.normalizedTime - _offset > 1)
            {
                _offset += 1;
                
                ClearAllNotifyState();

                _eventCount = 0;
            }
        }
        public void AnimNotify(Object eventData)
        {
            if (!eventData)
                return;

            if (eventData is IAnimNotifyEvent animNotifyState)
            {
                animNotifyState.Enter(this);
            }
        }

        private void ClearAllNotifyState()
        {
            int removeIndex = _animNotifyStates.Count - _eventCount;

            if (removeIndex <= 0)
                return;

            for(int i = 0; i < removeIndex; i++)
            {
                var animNotifyState = _animNotifyStates[i];

                animNotifyState.Exit(this);
            }

            _animNotifyStates.RemoveRange(0, removeIndex);
        }
        public void AnimNotifyStateEnter(Object eventData)
        {
            if (!eventData)
                return;

            if(eventData is IAnimNotifyStateEvent animNotifyState)
            {
                _eventCount++;

                _animNotifyStates.Add(animNotifyState);

                animNotifyState.Enter(this);
            }
            
        }
        public void AnimNotifyStateExit(Object eventData)
        {
            if (!eventData)
                return;

            if (eventData is IAnimNotifyStateEvent animNotifyState)
            {
                if (_animNotifyStates.Remove(animNotifyState))
                {
                    animNotifyState.Exit(this);
                }
            }
        }
    }
}