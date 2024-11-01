﻿using UnityEngine;
using UnityEngine.Events;

namespace StudioScor.Utilities
{

    [System.Serializable]
    public class BaseStateMonoEvent
    {
        public UnityEvent<IState> OnEnteredState;
        public UnityEvent<IState> OnExitedState;
    }

    public abstract class BaseStateMono : BaseMonoBehaviour, IState
    {
        [Header(" [ State MonoBehaviour ] ")]
        [SerializeField] private bool useUnityEvent = false;
        [SerializeField] protected BaseStateMonoEvent unityEvents = new();

        public event UnityAction<IState> OnEnteredState;
        public event UnityAction<IState> OnExitedState;

        private bool isPlaying;
        public bool IsPlaying => isPlaying;

        #region EDITOR ONLY

        protected virtual void Reset()
        {
#if UNITY_EDITOR
            enabled = false;
#endif
        }
        #endregion


        public virtual bool CanEnterState()
        {
            return !IsPlaying;
        }

        public virtual bool CanExitState()
        {
            return IsPlaying;
        }


        public void ForceEnterState()
        {
            isPlaying = true;

            enabled = true;

            EnterState();

            Callback_OnEnteredState();
        }

        public void ForceExitState()
        {
            isPlaying = false;

            ExitState();

            enabled = false;

            Callback_OnExitedState();
        }

        public bool TryEnterState()
        {
            if (!CanEnterState())
                return false;

            ForceEnterState();

            return true;
        }

        public bool TryExitState()
        {
            if (!CanExitState())
                return false;

            ForceExitState();

            return true;
        }

        protected virtual void EnterState()
        {

        }
        protected virtual void ExitState()
        {

        }

        private void Callback_OnEnteredState()
        {
            Log("Entered State");

            if (useUnityEvent)
            {
                unityEvents.OnEnteredState?.Invoke(this);
            }

            OnEnteredState?.Invoke(this);
        }
        private void Callback_OnExitedState()
        {
            Log("Exited State");

            if (useUnityEvent)
            {
                unityEvents.OnExitedState?.Invoke(this);
            }

            OnExitedState?.Invoke(this);
        }
    }

    public abstract class BaseStateClass : BaseClass, IState
    {       
        public event UnityAction<IState> OnEnteredState;
        public event UnityAction<IState> OnExitedState;

        protected bool _IsActivate;
        public bool IsActivate => _IsActivate;

        public BaseStateClass()
        {

        }

        public virtual bool TryEnterState()
        {
            if (!CanEnterState())
                return false;

            ForceEnterState();

            return true;
        }
        public virtual bool TryExitState()
        {
            if (!CanExitState())
                return false;

            ForceExitState();

            return true;
        }
        public virtual bool CanEnterState()
        {
            return !_IsActivate;
        }
        public virtual bool CanExitState()
        {
            return _IsActivate;
        }
        public void ForceEnterState()
        {
            if (_IsActivate)
                return;

            _IsActivate = true;

            EnterState();

            Invoke_OnEnteredState();
        }
        public void ForceExitState()
        {
            if (!_IsActivate)
                return;

            _IsActivate = false;

            ExitState();

            Invoke_OnExitedState();
        }

        protected abstract void EnterState();
        protected virtual void ExitState() { }

        private void Invoke_OnEnteredState()
        {
            Log($"{nameof(OnEnteredState)}");

            OnEnteredState?.Invoke(this);
        }
        private void Invoke_OnExitedState()
        {
            Log($"{nameof(OnExitedState)}");

            OnExitedState?.Invoke(this);
        }
    }
}