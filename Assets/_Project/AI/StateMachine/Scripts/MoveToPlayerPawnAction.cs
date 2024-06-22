using StudioScor.PlayerSystem;
using StudioScor.StateMachine;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Action/new Move To Player Pawn Action", fileName = "Action_MoveToPlayerPawn")]
    public class MoveToPlayerPawnAction : Action
    {
        [Header(" [ Move To PlayerPawn Action ] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private PlayerManager _playerManager;

        public override void EnterAction(StateMachineComponent stateMachine)
        {

        }

        public override void ExitAction(StateMachineComponent stateMachine)
        {
            if (!_selfKey.TryGetValue(stateMachine, out var controllerSystem))
                return;

            controllerSystem.SetMoveDirection(controllerSystem.MoveDirection, 0f);
        }

        public override void UpdateAction(StateMachineComponent stateMachine)
        {
            if (!_selfKey.TryGetValue(stateMachine, out var controllerSystem))
                return;

            if (!_playerManager.HasPlayerPawn)
                return;

            Vector3 direction = controllerSystem.Pawn.transform.Direction(_playerManager.PlayerPawn.transform);
            float strength = Mathf.Max(1f, direction.magnitude);

            controllerSystem.SetMoveDirection(direction, strength);

        }
    }
}