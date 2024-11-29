using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordInformationDisplay : BaseMonoBehaviour
    {
        public delegate void RewordInformationStateEvent(RewordInformationDisplay rewordInformationDisplay);

        [Header(" [ Reword Information Display ] ")]
        [SerializeField] private DataComponent _dataComponent;

        [Header(" State Machine ")]
        [SerializeField] private RewordInformationDisplayState.StateMachine _stateMachine;
        [SerializeField] private RewordInformationDisplayState _activeTransitionState;
        [SerializeField] private RewordInformationDisplayState _activeState;
        [SerializeField] private RewordInformationDisplayState _inactiveTransitionState;
        [SerializeField] private RewordInformationDisplayState _inactiveState;

        private bool _wasInit = false;

        public event RewordInformationStateEvent OnFInishedActivate;
        public event RewordInformationStateEvent OnFInishedInactivate;

        private void OnValidate()
        {
            if(!_dataComponent)
            {
                _dataComponent = GetComponentInChildren<DataComponent>();
            }
        }

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            _stateMachine.Start();
        }

        public void Activate()
        {
            _stateMachine.TrySetState(_activeTransitionState);
        }
        public void Inactivate()
        {
            _stateMachine.TrySetState(_inactiveTransitionState);
        }

        public void SetData(ItemSO rewordItem)
        {
            _dataComponent.UpdateData(rewordItem);
        }

        public void FinishedState(RewordInformationDisplayState finishedState)
        {
            if (_stateMachine.CurrentState != finishedState)
                return;

            if(finishedState == _activeTransitionState)
            {
                _stateMachine.ForceSetState(_activeState);
                OnFInishedActivate?.Invoke(this);
            }
            else if (finishedState == _inactiveTransitionState)
            {
                _stateMachine.ForceSetState(_inactiveState);
                OnFInishedInactivate?.Invoke(this);
            }
        }
    }
}
