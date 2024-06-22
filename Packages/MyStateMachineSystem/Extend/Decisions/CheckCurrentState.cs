using UnityEngine;
using System.Collections.Generic;

namespace StudioScor.StateMachine
{
    public abstract class CheckCurrentState : Decision
    {
        [SerializeField] private List<State> _States;
        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            return _States.Contains(stateMachine.CurrentState);
        }
    }
}
