using UnityEngine;
using System.Collections;
using StudioScor.Utilities;

namespace StudioScor.StateMachine
{
    public abstract class Decision : BaseScriptableObject
    {
        [SerializeField] protected bool _IsInverse = false;

        public virtual void EnterDecide(StateMachineComponent stateMachine) { }
        public bool Decide(StateMachineComponent stateMachine)
        {
            return CheckDecide(stateMachine) != _IsInverse;
        }
        public virtual void ExitDecide(StateMachineComponent stateMachine) { }

        public abstract bool CheckDecide(StateMachineComponent stateMachine);
    }
}