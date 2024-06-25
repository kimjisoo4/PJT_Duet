using StudioScor.AbilitySystem;
using StudioScor.StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.Controller.Enemy
{
    [CreateAssetMenu(menuName = "Project/Duet/StateMachine/Action/new Activate Ability Action", fileName = "Action_ActivateAbility_")]
    public class ActivateAbilityAction : Action
    {
        [Header(" [ Activate Ability Action ] ")]
        [SerializeField] private BlackboardKey_Controller _selfKey;
        [SerializeField] private Ability _ability;

        private readonly Dictionary<StateMachineComponent, IAbilitySystem> _abilitySystemDatas = new();

        protected override void OnReset()
        {
            base.OnReset();

            _abilitySystemDatas.Clear();
        }

        public override void EnterAction(StateMachineComponent stateMachine)
        {
            var controller = _selfKey.GetValue(stateMachine);
            var abilitySystem = controller.Pawn.gameObject.GetAbilitySystem();

            _abilitySystemDatas.Add(stateMachine, abilitySystem);

            abilitySystem.TryActivateAbility(_ability);
        }

        public override void ExitAction(StateMachineComponent stateMachine)
        {
            if (_abilitySystemDatas.TryGetValue(stateMachine, out IAbilitySystem abilitySystem))
            {
                abilitySystem.ReleasedAbility(_ability);
            }

            _abilitySystemDatas.Remove(stateMachine);
        }

        public override void UpdateAction(StateMachineComponent stateMachine)
        {
            if(_abilitySystemDatas.TryGetValue(stateMachine, out IAbilitySystem abilitySystem))
            {
                abilitySystem.TryActivateAbility(_ability);
            }
        }
    }
}