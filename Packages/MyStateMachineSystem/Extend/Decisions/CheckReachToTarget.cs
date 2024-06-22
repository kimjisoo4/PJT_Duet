using UnityEngine;
using StudioScor.Utilities;
using StudioScor.StateMachine;

namespace Portfolio.Monster
{
    [CreateAssetMenu(menuName = "StudioScor/StateMachine/Decisions/new Check Reach To Target", fileName = "Check_ReachToTarget")]
    public class CheckReachToTarget : Decision
    {
        [Header(" [ Do Action TurnTo Target] ")]
        [SerializeField] private bool _UseData = false;
        [SerializeField][SCondition(nameof(_UseData))] private TransformContainer _Self;
        [SerializeField] private TransformContainer _Target;
        [SerializeField] private float _Distance = 1f;

        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            if (!_Target.TryGetValue(stateMachine, out Transform target))
                return false;

            Transform self;

            if (_UseData)
            {
                if(!_Self.TryGetValue(stateMachine, out self))
                {
                    return false;
                }
            }
            else
            {
                self = stateMachine.transform;
            }

            return (self.HorizontalDistance(target) < _Distance);
        }
    }

}
