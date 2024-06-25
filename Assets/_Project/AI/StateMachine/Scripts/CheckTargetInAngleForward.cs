using StudioScor.StateMachine;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Decisions/new Check Target In Angle Forward ", fileName = "Decision_TargetInAngleForward")]
    public class CheckTargetInAngleForward : Decision
    {
        [Header(" [ Check InAngle Forward] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private BlackBoardKey_Transform _targetKey;
        [SerializeField][Range(0f, 180f)] private float _angle = 30f;


        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            var controller = _selfKey.GetValue(stateMachine);
            
            if (controller is null || !controller.IsPossess)
                return false;
            
            var target = _targetKey.GetValue(stateMachine);

            if (!target)
                return false;

            float angle = controller.Pawn.transform.AngleOnForward(target);

            return angle.InRange(-_angle, _angle);
        }
    }
}