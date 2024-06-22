using UnityEngine;
using StudioScor.Utilities;
using System;
using System.Collections;

namespace StudioScor.StateMachine
{
    public class StateMachineComponent : BaseMonoBehaviour
    {
        #region Event
        public delegate void ChangedStateHandler(StateMachineComponent stateMachine, State currentState, State prevState);
        #endregion

        [Header(" [ State Machine ] ")]
        [Tooltip(" Start/Default State.")]
        [SerializeField] private BlackBoard _BlackBoard;
        [SerializeField] private State _DefaultState;

        [Tooltip(" Remain Current State. ")]
        [SerializeField][ SReadOnlyWhenPlaying] private State _RemainState;
        
        [Tooltip(" Transition Default State.")]
        [SerializeField][ SReadOnlyWhenPlaying] private State _ResetState;

        [SerializeField] private bool _useAutoPlaying = true;

        private bool _isPlaying = false;
        private State _CurrentState;
        private float _StateTimeElapsed = 0;
        private float _DeltaTime;
        private float _FixedDeltaTime;

        public bool IsPlaying => _isPlaying;

        public State CurrentState => _CurrentState;
        public float DeltaTime => _DeltaTime;
        public float FixedDeltaTime => _FixedDeltaTime;
        public float StateElapsed => _StateTimeElapsed;

        public BlackBoard BlackBoard => _BlackBoard;

        public event ChangedStateHandler OnChangedState;

#if UNITY_EDITOR
        [Header(" [ Debug ] ")]
        public Vector3 DebugOffset = new Vector3(0, 2, 0);
        public float DebugRadius = 0.5f;
        private GUIStyle _GuiStyle;
        private Camera _Camera;

        private void OnDrawGizmos()
        {
            if (!_CurrentState || !gameObject.activeInHierarchy)
                return;

            Gizmos.color = _CurrentState.Color;
            Gizmos.DrawWireSphere(transform.TransformPoint(DebugOffset), DebugRadius);

            _CurrentState.DrawGizmos(this);
        }
        private void OnDrawGizmosSelected()
        {
            if (!_CurrentState || !gameObject.activeInHierarchy)
                return;

            Gizmos.color = _CurrentState.Color;
            Gizmos.DrawSphere(transform.TransformPoint(DebugOffset), DebugRadius);

            _CurrentState.DrawGizmosSelected(this);
        }

        private void OnGUI()
        {
            if (!UseDebug)
                return;

            if (_CurrentState is null)
                return;

            if(_GuiStyle is null)
            {
                _GuiStyle = new();

                _GuiStyle.normal.textColor = Color.white;
                _GuiStyle.alignment = TextAnchor.MiddleCenter;
                _GuiStyle.fontStyle = FontStyle.Bold;
            }

            if(!_Camera)
            {
                _Camera = Camera.main;

                return;
            }

            Vector3 worldPosition = _Camera.WorldToScreenPoint(transform.TransformPoint(DebugOffset));
            Rect rect = new Rect(worldPosition.x, Screen.height - worldPosition.y, 0f, 0f);
            
            GUI.Label(rect, _CurrentState.Name, _GuiStyle);
        }
#endif

        private void Awake()
        {
            SetupStateMachine();
        }

        private void OnDestroy()
        {
            if (_CurrentState != null)
            {
                _CurrentState.ExitState(this);
            }

            _BlackBoard.Remove(this);
        }
        private void OnEnable()
        {
            if(_useAutoPlaying)
            {
                OnStateMachine();
            }
        }
        private void OnDisable()
        {
            if(_useAutoPlaying)
            {
                EndStateMachine();
            }
        }

        private void SetupStateMachine()
        {
            _BlackBoard.Create(this);

            OnSetup();
        }
        public void ResetStateMachine()
        {
            _BlackBoard.Clear(this);

            OnReset();
        }

        public void OnStateMachine()
        {
            if (_isPlaying)
                return;

            _isPlaying = true;

            ResetStateMachine();

            TransitionToState(_DefaultState);
        }
        public void EndStateMachine()
        {
            if (!_isPlaying)
                return;

            _isPlaying = false;
        }

        protected virtual void OnSetup() { }
        protected virtual void OnReset() { }


        private void Update()
        {
            if (!IsPlaying)
                return;

            _DeltaTime = Time.deltaTime;
            _StateTimeElapsed += _DeltaTime;

            _CurrentState.UpdateState(this);
        }
        private void FixedUpdate()
        {
            if (!IsPlaying)
                return;

            _FixedDeltaTime = Time.fixedDeltaTime;

            _CurrentState.PhysicsUpdateState(this);
        }

        public void TransitionToDefaultState()
        {
            TransitionToState(_DefaultState);
        }

        public void TransitionToState(State nextState)
        {
            if (nextState == null)
                return;

            if (nextState == _RemainState)
                return;
            
            if (nextState == _ResetState)
            {
                if (_CurrentState == _DefaultState)
                {
                    return;
                }

                Transition(_DefaultState);
            }
            else
            {
                Transition(nextState);

            }        
        }
        private void Transition(State nextState)
        {
            if (_CurrentState != null)
            {
                _CurrentState.ExitState(this);
            }

            var prevState = _CurrentState;

            _StateTimeElapsed = 0f;

            _CurrentState = nextState;

            _CurrentState.EnterState(this);

            Callback_OnChangedState(prevState);
        }

        #region Callback
        protected void Callback_OnChangedState(State prevState)
        {
            Log($"On Changed State - Current [ {(_CurrentState ? _CurrentState.Name : "Empty")} ] Prev [ {(prevState ? prevState.Name : "Empty" )} ]");

            OnChangedState?.Invoke(this, _CurrentState, prevState);
        }
        #endregion
    }
}