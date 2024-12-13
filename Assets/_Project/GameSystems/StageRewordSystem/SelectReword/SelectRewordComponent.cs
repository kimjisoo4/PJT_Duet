using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class SelectRewordComponent : BaseMonoBehaviour
    {
        public delegate void SelectRewordStateEventHandler(SelectRewordComponent selectRewordComponent);

        [Header(" [ Reword Data Component ] ")]
        [SerializeField] private GameObject _uiEventActor;
        [SerializeField] private DataComponent _dataContainer;

        [Header(" State Machine ")]
        [SerializeField] private SelectRewordState.StateMachine _stateMachine;
        [SerializeField] private SelectRewordState _activeTransitionState;
        [SerializeField] private SelectRewordState _activeState;
        [SerializeField] private SelectRewordState _inactiveTransitionState;
        [SerializeField] private SelectRewordState _inactiveState;

        [SerializeField][SReadOnly] private ItemSO _itemData;

        private ISubmitEventListener _submitEventListener;
        private ISelectEventListener _selectEventListener;

        private bool _wasInit;
        public ItemSO ItemData => _itemData;

        public event SelectRewordStateEventHandler OnSelected;
        public event SelectRewordStateEventHandler OnSubmited;

        public event SelectRewordStateEventHandler OnFinishedActivate;
        public event SelectRewordStateEventHandler OnFinishedInactivate;

        private void OnValidate()
        {
            if(!_dataContainer)
            {
                _dataContainer = GetComponent<DataComponent>();
            }

            if (!_uiEventActor)
            {
                if (gameObject.TryGetComponentInParentOrChildren(out ISubmitEventListener submitEventListener))
                {
                    _uiEventActor = submitEventListener.gameObject;
                }
            }
        }

        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
            if(_selectEventListener is not null)
            {
                _selectEventListener.OnSelected -= _selectEventListener_OnSelected;
            }
            if(_submitEventListener is not null)
            {
                _submitEventListener.OnSubmited += _submitEventListener_OnSubmited;
            }
        }

        public void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            _selectEventListener = _uiEventActor.GetComponent<ISelectEventListener>();
            _submitEventListener = _uiEventActor.GetComponent<ISubmitEventListener>();

            _selectEventListener.OnSelected += _selectEventListener_OnSelected;
            _submitEventListener.OnSubmited += _submitEventListener_OnSubmited;

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

        public void SetData(ItemSO reword)
        {
            _itemData = reword;

            _dataContainer.UpdateData(reword);
        }

        public void FinishedState(SelectRewordState finishedState)
        {
            if (finishedState == _activeTransitionState)
            {
                _stateMachine.TrySetState(_activeState);
                OnFinishedActivate?.Invoke(this);
            }
            else if (finishedState == _inactiveTransitionState)
            {
                _stateMachine.TrySetState(_inactiveState);
                OnFinishedInactivate?.Invoke(this);
            }
        }

        private void _selectEventListener_OnSelected(ISelectEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            OnSelected?.Invoke(this);
        }
        private void _submitEventListener_OnSubmited(ISubmitEventListener submitEventListener, UnityEngine.EventSystems.BaseEventData eventData)
        {
            if (!_itemData)
                return;

            if (_stateMachine.CurrentState != _activeState)
                return;

            OnSubmited?.Invoke(this);
        }
    }
}
