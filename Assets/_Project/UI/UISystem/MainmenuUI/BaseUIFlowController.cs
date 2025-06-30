using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public abstract class BaseUIFlowController : BaseMonoBehaviour
    {
        [Header(" [ UI Flow Controller ] ")]
        [SerializeField] private BaseUIFlowState.StateMachine _stateMachine;

        [Header(" Input Control ")]
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private EBlockInputState _blockInput = EBlockInputState.Game;

        public BaseUIFlowState.StateMachine StateMachine => _stateMachine;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if(!_inputBlocker)
            {
                _inputBlocker = SUtility.FindAssetByType<InputBlocker>();
            }
#endif
        }
        protected virtual void OnDestory()
        {
            _inputBlocker.UnblockInput(this);
        }

        [ContextMenu(nameof(Activate), false, 1000000)]
        public void Activate()
        {
            _stateMachine.Start();

            _inputBlocker.BlockInput(this, _blockInput);
        }

        [ContextMenu(nameof(Deactivate), false, 1000000)]
        public void Deactivate()
        {
            _stateMachine.End();

            _inputBlocker.UnblockInput(this);
        }
    }
}
