using UnityEngine;

namespace StudioScor.StateMachine
{
    [CreateAssetMenu(menuName ="StudioScor/StateMachine/Decisions/new CheckHasValue", fileName ="Decision_HasValue_")]
    public class CheckHasValue : Decision
    {
        [SerializeField] private BlackBoardKeyBase _key;

        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            return _key.HasValue(stateMachine);
        }
    }
}
