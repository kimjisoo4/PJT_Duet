using StudioScor.PlayerSystem;
using StudioScor.StateMachine;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{

    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Action/new Set Turn Direction State", fileName = "Action_SetTurnDirectionState")]
    public class SetTurnDirectionState : Action
    {
        [Header(" [ Set Turn Direction State ] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private ETurnDirectionState _turnDirectionState;

        public override void EnterAction(StateMachineComponent stateMachine)
        {
            if (!_selfKey.TryGetValue(stateMachine, out IControllerSystem controller))
                return;

            controller.SetTurnDirectionState(_turnDirectionState);
        }

        public override void ExitAction(StateMachineComponent stateMachine)
        {
        }

        public override void UpdateAction(StateMachineComponent stateMachine)
        {
        }
    }
}