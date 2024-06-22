using System.Collections;
using UnityEngine;
using StudioScor.Utilities;
using System.Diagnostics;

namespace StudioScor.StateMachine
{
    [System.Serializable]
    public class Transition
    {
#if UNITY_EDITOR
        [SerializeField][SReadOnly] private string _Name;
#endif
        [SerializeField] private Decision[] _Decisions;
        [SerializeField] private State _True;
        [SerializeField] private State _False;

        [Conditional("UNITY_EDITOR")]
        public void EDITOR_SetTransitionName()
        {
#if UNITY_EDITOR
            string transitionName = "[T] ";

            if (_True)
            {
                transitionName += _True.Name;
            }

            transitionName += " | ";

            if (_False)
            {
                transitionName += _False.Name;
            }

            transitionName += " [F]";

            _Name = transitionName;
#endif
        }

        public void EnterTransition(StateMachineComponent stateMachine)
        {
            foreach (var decision in _Decisions)
            {
                decision.EnterDecide(stateMachine);
            }
        }
        public void ExitTransition(StateMachineComponent stateMachine)
        {
            foreach (var decision in _Decisions)
            {
                decision.ExitDecide(stateMachine);
            }
        }

        public void CheckTransition(StateMachineComponent stateMachine)
        {
            foreach (var decision in _Decisions)
            {
                if (!decision.Decide(stateMachine))
                {
                    stateMachine.TransitionToState(_False);

                    return;
                }
            }

            stateMachine.TransitionToState(_True);
        }
    }

}