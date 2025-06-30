using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Button/Create Character Button")]
    public class CreateCharacterButton : BaseMonoBehaviour
    {
        public delegate void SelectCharacterStateEventHandler(CreateCharacterButton selectCharacterUI);

        [Header(" [ Character Data UI ] ")]
        [SerializeField] private GameObject _eventListennerActor;
        [SerializeField] private DataComponent _dataContainer;

        [Header(" State Machine ")]
        [SerializeField] private FiniteStateMachineSystem<CreateCharacterButtonState> _stateMachine;
        [SerializeField] private CreateCharacterButtonState _activeTransitionState;
        [SerializeField] private CreateCharacterButtonState _activeState;
        [SerializeField] private CreateCharacterButtonState _inactiveTransitionState;
        [SerializeField] private CreateCharacterButtonState _inactiveState;
        [SerializeField] private CreateCharacterButtonState _selectState;
        [SerializeField] private CreateCharacterButtonState _submitState;

        [Header(" Unity Events ")]
        [SerializeField] private ToggleableUnityEvent _onActivated;
        [SerializeField] private ToggleableUnityEvent _onFinishedBlendIn;
        [SerializeField] private ToggleableUnityEvent _onStartedBlendOut;
        [SerializeField] private ToggleableUnityEvent _onDeactivated;

        private CharacterData _characterData;
        private ISubmitEventListener _submitEvent;
        private ISelectEventListener _selectEvent;
        private IDeselectEventListener _deselectEvent;

        private bool _wasInit;
        private bool _isInTransition;

        public CharacterData CharacterData => _characterData;
        public bool IsInTransition => _isInTransition;

        public event SelectCharacterStateEventHandler OnAcitvated;
        public event SelectCharacterStateEventHandler OnFInishedBlendIn;
        public event SelectCharacterStateEventHandler OnStartedBlendOut;
        public event SelectCharacterStateEventHandler OnDeactivated;

        public event SelectCharacterStateEventHandler OnSubmited;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_eventListennerActor)
            {
                if(!gameObject.TryGetGameObjectByTypeInChildren<ISubmitEventListener>(out _eventListennerActor))
                {
                    _eventListennerActor = gameObject.GetGameObjectByTypeInParentOrChildren<ISubmitEventListener>();
                }
            }
#endif

        }
        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
            if(_submitEvent is not null)
            {
                _submitEvent.OnSubmited -= _submitEvent_OnSubmited;
                _submitEvent = null;
            }

            if(_selectEvent is not null)
            {
                _selectEvent.OnSelected += _selectEvent_OnSelected;
                _selectEvent = null;
            }

            if(_deselectEvent is not null)
            {
                _deselectEvent.OnDeselected -= _deselectEvent_OnDeselected;
                _deselectEvent = null;
            }
        }

        public void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            _submitEvent = _eventListennerActor.GetComponent<ISubmitEventListener>();
            _submitEvent.OnSubmited += _submitEvent_OnSubmited;

            _selectEvent = _eventListennerActor.GetComponent<ISelectEventListener>();
            _selectEvent.OnSelected += _selectEvent_OnSelected;

            _deselectEvent = _eventListennerActor.GetComponent<IDeselectEventListener>();
            _deselectEvent.OnDeselected += _deselectEvent_OnDeselected;

            _stateMachine.Start();
        }

        public void Activate()
        {
            Invoke_OnActivated();

            _isInTransition = true;

            if (!_stateMachine.TrySetState(_activeTransitionState))
            {
                _isInTransition = false;

                _stateMachine.ForceSetState(_activeState);
                
                Invoke_OnDeactivated();
            }
        }

        public void Deactivate()
        {
            Invoke_OnStartedBlendOut();

            _isInTransition = true;

            if(!_stateMachine.TrySetState(_inactiveTransitionState))
            {
                _isInTransition = false;

                _stateMachine.ForceSetState(_inactiveState);
                
                Invoke_OnDeactivated();
            }
        }

        public void FinishedState(CreateCharacterButtonState state)
        {
            if(state == _activeTransitionState)
            {
                _isInTransition = false;

                _stateMachine.ForceSetState(_activeState);

                Invoke_OnFinishedBlendIn();
            }
            else if (state == _inactiveTransitionState)
            {
                _isInTransition = false;

                _stateMachine.ForceSetState(_inactiveState);

                Invoke_OnDeactivated();
            }
        }
        
        public void SetCharacterData(CharacterData characterData)
        {
            Log($"{nameof(SetCharacterData)} - {(characterData is null ? "null" : characterData.ID)}");

            _characterData = characterData;

            _dataContainer.UpdateData(_characterData);
        }

        private void _submitEvent_OnSubmited(ISubmitEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (_characterData is null)
                return;

            OnSubmited?.Invoke(this);

            _stateMachine.TrySetState(_submitState);
        }
        private void _selectEvent_OnSelected(ISelectEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            _stateMachine.TrySetState(_selectState);
        }
        private void _deselectEvent_OnDeselected(IDeselectEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            _stateMachine.TrySetState(_activeState);
        }

        private void Invoke_OnActivated()
        {
            Log(nameof(OnAcitvated));

            _onActivated.Invoke();
            OnAcitvated?.Invoke(this);
        }
        private void Invoke_OnFinishedBlendIn()
        {
            Log(nameof(OnFInishedBlendIn));

            _onFinishedBlendIn.Invoke();
            OnFInishedBlendIn?.Invoke(this);
        }
        private void Invoke_OnStartedBlendOut()
        {
            Log(nameof(OnStartedBlendOut));

            _onStartedBlendOut.Invoke();
            OnStartedBlendOut?.Invoke(this);
        }
        private void Invoke_OnDeactivated()
        {
            Log(nameof(OnDeactivated));

            _onDeactivated.Invoke();
            OnDeactivated?.Invoke(this);
        }

    }
}
