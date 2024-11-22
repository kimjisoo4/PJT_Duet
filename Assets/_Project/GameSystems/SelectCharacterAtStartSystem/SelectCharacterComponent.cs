using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{

    public class SelectCharacterComponent : BaseMonoBehaviour
    {
        public delegate void SelectCharacterStateEventHandler(SelectCharacterComponent selectCharacterUI);

        [Header(" [ Character Data UI ] ")]
        [SerializeField] private GameObject _submitActor;
        [SerializeField] private DataComponent _dataContainer;

        [Header(" State Machine ")]
        [SerializeField] private FiniteStateMachineSystem<SelectCharacterUIState> _stateMachine;
        [SerializeField] private SelectCharacterUIState _activeTransitionState;
        [SerializeField] private SelectCharacterUIState _activeState;

        [SerializeField] private SelectCharacterUIState _inactiveTransitionState;
        [SerializeField] private SelectCharacterUIState _inactiveState;


        private CharacterData _characterData;
        private ISubmitEventListener _submit;

        public CharacterData CharacterData => _characterData;

        public event SelectCharacterStateEventHandler OnFinishedActivate;
        public event SelectCharacterStateEventHandler OnFinishedInactivate;
        public event SelectCharacterStateEventHandler OnSubmited;


        private void Awake()
        {
            _submit = _submitActor.GetComponent<ISubmitEventListener>();
            _submit.OnSubmited += _submit_OnSubmited;

            _stateMachine.Start();
        }

        private void OnDestroy()
        {
            if(_submit is not null)
            {
                _submit.OnSubmited -= _submit_OnSubmited;
                _submit = null;
            }
        }

        public void Activate()
        {
            _stateMachine.TrySetState(_activeTransitionState);
        }
        public void Inactivate()
        {
            _stateMachine.TrySetState(_inactiveTransitionState);
        }

        public void FinishedState(SelectCharacterUIState state)
        {
            if(state == _activeTransitionState)
            {
                _stateMachine.TrySetState(_activeState);
                OnFinishedActivate?.Invoke(this);
            }
            else if (state == _inactiveTransitionState)
            {
                _stateMachine.TrySetState(_inactiveState);
                OnFinishedInactivate?.Invoke(this);
            }
        }
        
        public void SetCharacterData(CharacterData characterData)
        {
            Log($"{nameof(SetCharacterData)} - {(characterData is null ? "null" : characterData.ID)}");

            _characterData = characterData;

            _dataContainer.UpdateData(_characterData);
        }

        private void _submit_OnSubmited(ISubmitEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (_characterData is null)
                return;

            OnSubmited?.Invoke(this);
        }
    }
}
