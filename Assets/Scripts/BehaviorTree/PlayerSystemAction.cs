using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    [TaskCategory("StudioScor/PlayerSystem")]
    public abstract class PlayerSystemAction : Action
    {
        private IControllerSystem _controllerSystem;
        public IControllerSystem ControllerSystem => _controllerSystem;
        public IPawnSystem Pawn => ControllerSystem.Pawn;

        public bool IsPossessed => ControllerSystem.IsPossess;

        public override void OnAwake()
        {
            base.OnAwake();

            _controllerSystem = gameObject.GetControllerSystem();
        }
    }
}

