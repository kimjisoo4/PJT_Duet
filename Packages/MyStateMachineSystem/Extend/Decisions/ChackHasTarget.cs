using UnityEngine;
using StudioScor.StateMachine;

namespace Portfolio.Monster
{
    [CreateAssetMenu(menuName = "StudioScor/StateMachine/Decisions/new Check Has Target", fileName = "Check_HasTarget")]
    public class ChackHasTarget : Decision
    {
        [Header(" [ Do Action TurnTo Target] ")]
        [SerializeField] private TransformContainer _TransformData;

        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            return (_TransformData.TryGetValue(stateMachine, out Transform target) && target);
        }
    }

}
