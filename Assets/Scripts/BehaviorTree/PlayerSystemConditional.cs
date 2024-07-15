using BehaviorDesigner.Runtime.Tasks;

namespace StudioScor.PlayerSystem.BehaviorTree
{
    [TaskCategory("StudioScor/PlayerSystem")]
    public abstract class PlayerSystemConditional : Conditional
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

