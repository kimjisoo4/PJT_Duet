using StudioScor.PlayerSystem;
using StudioScor.StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{

    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Decisions/new Check Has Player Pawn", fileName = "Decision_CheckHasPlayerPawn")]
    public class CheckHasPlayerPawn : Decision
    {
        [Header(" [ Check Has Player Pawn ] ")]
        [SerializeField] private PlayerManager _playerManager;
        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            return _playerManager.HasPlayerPawn;
        }
    }
}