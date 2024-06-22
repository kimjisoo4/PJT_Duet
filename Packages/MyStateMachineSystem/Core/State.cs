using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using StudioScor.Utilities;

namespace StudioScor.StateMachine
{
    [CreateAssetMenu(menuName = "StudioScor/StateMachine/new State", fileName = "State_")]
    public class State : BaseScriptableObject
    {
        [Header(" [ State ] ")]
        public string Name = "New State";
        public Color Color = Color.white;

        [Header(" [ Actions ] ")]
        [SerializeField] private List<Action> _EarlyActions;
        [SerializeField] private List<Action> _ProcessActions;
        [SerializeField] private List<Action> _LateActions;

        [Header(" [ Physics Actions ] ")]
        [SerializeField] private List<Action> _PhysisActions;

        [Header(" [ Transitions ] ")]
        [SerializeField] private List<Transition> _Transitions;


        #region EDITOR ONLY
        private void OnValidate()
        {
#if UNITY_EDITOR
            foreach (Transition transition in _Transitions)
            {
                transition.EDITOR_SetTransitionName();
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawGizmos(StateMachineComponent stateMachine)
        {
#if UNITY_EDITOR
            foreach (Action action in _EarlyActions)
            {
                action.DrawGizmos(stateMachine);
            }
            foreach (Action action in _ProcessActions)
            {
                action.DrawGizmos(stateMachine);
            }
            foreach (Action action in _LateActions)
            {
                action.DrawGizmos(stateMachine);
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawGizmosSelected(StateMachineComponent stateMachine)
        {
#if UNITY_EDITOR
            foreach (Action action in _EarlyActions)
            {
                action.DrawGizmosSelected(stateMachine);
            }
            foreach (Action action in _ProcessActions)
            {
                action.DrawGizmosSelected(stateMachine);
            }
            foreach (Action action in _LateActions)
            {
                action.DrawGizmosSelected(stateMachine);
            }
#endif
        }
        #endregion

        public void EnterState(StateMachineComponent stateMachine)
        {
            EnterTransitions(stateMachine);

            EnterActions(stateMachine);
        }
        public void UpdateState(StateMachineComponent stateMachine)
        {
            DoActions(stateMachine);

            CheckTransition(stateMachine);
        }
        public void PhysicsUpdateState(StateMachineComponent stateMachine)
        {
            DoPhysicsActions(stateMachine);
        }

        public void ExitState(StateMachineComponent stateMachine)
        {
            ExitTransitions(stateMachine);

            ExitActions(stateMachine);
        }
        private void EnterTransitions(StateMachineComponent stateMachine)
        {
            foreach (Transition transition in _Transitions)
            {
                transition.EnterTransition(stateMachine);
            }
        }
            
        private void EnterActions(StateMachineComponent stateMachine)
        {
            foreach (Action action in _EarlyActions)
            {
                action.EnterAction(stateMachine);
            }
            foreach (Action action in _ProcessActions)
            {
                action.EnterAction(stateMachine);
            }
            foreach (Action action in _LateActions)
            {
                action.EnterAction(stateMachine);
            }
            foreach (Action action in _PhysisActions)
            {
                action.EnterAction(stateMachine);
            }
        }

        private void ExitTransitions(StateMachineComponent stateMachine)
        {
            foreach (Transition transition in _Transitions)
            {
                transition.ExitTransition(stateMachine);
            }
        }
        private void ExitActions(StateMachineComponent stateMachine)
        {
            foreach (Action action in _EarlyActions)
            {
                action.ExitAction(stateMachine);
            }
            foreach (Action action in _ProcessActions)
            {
                action.ExitAction(stateMachine);
            }
            foreach (Action action in _LateActions)
            {
                action.ExitAction(stateMachine);
            }
            foreach (Action action in _PhysisActions)
            {
                action.ExitAction(stateMachine);
            }
        }
        private void DoActions(StateMachineComponent stateMachine)
        {
            foreach (Action action in _EarlyActions)
            {
                action.UpdateAction(stateMachine);
            }
            foreach (Action action in _ProcessActions)
            {
                action.UpdateAction(stateMachine);
            }
            foreach (Action action in _LateActions)
            {
                action.UpdateAction(stateMachine);
            }
        }
        private void DoPhysicsActions(StateMachineComponent stateMachine)
        {
            foreach (Action action in _PhysisActions)
            {
                action.UpdateAction(stateMachine);
            }
        }

        private void CheckTransition(StateMachineComponent stateMachine)
        {
            foreach (Transition transition in _Transitions)
            {
                transition.CheckTransition(stateMachine);
            }
        }
    }

}
