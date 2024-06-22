using UnityEngine;

namespace StudioScor.StateMachine
{
    [CreateAssetMenu( menuName = "StudioScor/StateMachine/Actions/new Log", fileName = "Do_Log")]
    public class DoActionDebugLog : Action
    {
        [SerializeField] private string _EnterText = "Enter Action";
        [SerializeField] private string _UpdateText = "Update Action";
        [SerializeField] private string _ExitText = "Exit Action";

        public override void EnterAction(StateMachineComponent stateMachine)
        {
            Log(_EnterText);
        }

        public override void ExitAction(StateMachineComponent stateMachine)
        {
            Log(_UpdateText);
        }

        public override void UpdateAction(StateMachineComponent stateMachine)
        {
            Log(_ExitText);
        }
    }
}