using StudioScor.AbilitySystem;
using StudioScor.StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Decisions/new Check Playing Ability", fileName = "Decision_IsPlayingAbility_")]
    public class CheckPlayingAbility : Decision
    {
        [Header(" [ Check Playing Ability ] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private Ability _ability;

        private readonly Dictionary<StateMachineComponent, IAbilitySystem> _abilitySystems = new();

        public override void EnterDecide(StateMachineComponent stateMachine)
        {
            base.EnterDecide(stateMachine);

            var controller = _selfKey.GetValue(stateMachine);

            var abilitySystem = controller.Pawn.gameObject.GetAbilitySystem();

            _abilitySystems.Add(stateMachine, abilitySystem);
        }

        public override void ExitDecide(StateMachineComponent stateMachine)
        {
            base.ExitDecide(stateMachine);

            _abilitySystems.Remove(stateMachine);
        }
        public override bool CheckDecide(StateMachineComponent stateMachine)
        {
            return _abilitySystems[stateMachine].IsPlayingAbility(_ability);
        }
    }
}