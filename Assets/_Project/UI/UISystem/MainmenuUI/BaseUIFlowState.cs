using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class BaseUIFlowState : BaseStateMono
    {
        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<BaseUIFlowState> { }

        [Header(" [ UI Flow State ] ")]
        [SerializeField] private BaseUIFlowController _uiFlowController;
        public BaseUIFlowController UIFlowController => _uiFlowController;

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            base.OnValidate();

            if (!_uiFlowController)
                _uiFlowController = GetComponentInParent<BaseUIFlowController>();
#endif
        }
    }
}
