using StudioScor.StateMachine;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Decisions/new Check Reach Distance", fileName = "Decision_CheckReachDistance")]
    public class CheckReachDistance : Decision
    {
        [Header(" [ Check Reach Distance ] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private BlackBoardKey_Transform _targetKey;
        [SerializeField] private float _reachDistance = 5f;

        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            if (!_selfKey.TryGetValue(stateMachine, out var controller))
                return false;

            if (!_targetKey.TryGetValue(stateMachine, out var target))
                return false;


            float distance = Vector3.Distance(controller.Pawn.transform.position, target.transform.position);

            return distance <= _reachDistance;
        }
    }
}