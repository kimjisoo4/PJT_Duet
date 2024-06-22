using UnityEngine;
using System.Collections;
using UnityEditor;
using StudioScor.Utilities;

using System.Diagnostics;

namespace StudioScor.StateMachine
{
    public abstract class Action : BaseScriptableObject
    {
        public abstract void EnterAction(StateMachineComponent stateMachine);
        public abstract void UpdateAction(StateMachineComponent stateMachine);
        public abstract void ExitAction(StateMachineComponent stateMachine);


        [Conditional("UNITY_EDITOR")]
        public virtual void DrawGizmos(StateMachineComponent stateMachine)
        {

        }
        [Conditional("UNITY_EDITOR")]
        public virtual void DrawGizmosSelected(StateMachineComponent stateMachine)
        {

        }
    }
}