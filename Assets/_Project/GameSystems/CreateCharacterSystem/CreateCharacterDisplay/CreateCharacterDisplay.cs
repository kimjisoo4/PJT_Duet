using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Display/Create Character Display")]
    public class CreateCharacterDisplay : BaseMonoBehaviour, ICreateCharacterDisplay
    {
        [Header(" [ Create Character Display ] ")]
        [SerializeField] private GameObject _uiActor;
        [SerializeField] private CreateCharacterButton[] _createCharacterButtons;

        [Header(" State Machine")]
        [SerializeField] private CreateCharacterDisplayState.StateMachine _stateMachine;
        [SerializeField] private CreateCharacterDisplayState _transitionActiveState;
        [SerializeField] private CreateCharacterDisplayState _activeState;
        [SerializeField] private CreateCharacterDisplayState _transitionInactiveState;
        [SerializeField] private CreateCharacterDisplayState _inactiveState;

        [Header(" Events ")]
        [SerializeField] private ToggleableUnityEvent _onActivated;
        [SerializeField] private ToggleableUnityEvent _onFinishedBlendIn;
        [SerializeField] private ToggleableUnityEvent _onStartedBlendOut;
        [SerializeField] private ToggleableUnityEvent _onInactivated;

        private bool _wasInit;
        private bool _isPlaying;

        public event ICreateCharacterDisplay.CreateCharacterDisplayEventHandler OnActivated;
        public event ICreateCharacterDisplay.CreateCharacterDisplayEventHandler OnFinishedBlendIn;
        public event ICreateCharacterDisplay.CreateCharacterDisplayEventHandler OnStartedBlendOut;
        public event ICreateCharacterDisplay.CreateCharacterDisplayEventHandler OnInactivated;

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
            Invoke_OnActivated();
            
            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);

            if (!_stateMachine.TrySetState(_transitionActiveState))
            {
                if (_stateMachine.TrySetState(_activeState))
                {
                    Invoke_OnFinishedBlendIn();   
                }
            }
        }
        public void Inactivate()
        {
            Invoke_OnStartedBlendOut();
            
            if (!_stateMachine.TrySetState(_transitionInactiveState))
            {
                if (_stateMachine.TrySetState(_inactiveState))
                {
                    if (_uiActor.activeSelf)
                        _uiActor.SetActive(false);

                    Invoke_OnInactivated();
                }
            }
        }

        public void SetCharacterDatas(IEnumerable<CharacterData> chracterDatas)
        {
            for(int i = 0; i < _createCharacterButtons.Length; i++)
            {
                var button = _createCharacterButtons[i];
                var data = chracterDatas.ElementAtOrDefault(i);

                button.SetCharacterData(data);
            }
        }

        public void FinishedState(CreateCharacterDisplayState finishedState)
        {
            Log($"{nameof(FinishedState)} - {finishedState}");

            if(finishedState == _transitionActiveState)
            {
                Invoke_OnFinishedBlendIn();

                _stateMachine.TrySetState(_activeState);
            }
            else if(finishedState == _transitionInactiveState)
            {
                _stateMachine.TrySetState(_inactiveState);

                if (_uiActor.activeSelf)
                    _uiActor.SetActive(false);

                Invoke_OnInactivated();
            }
        }

        private void Invoke_OnActivated()
        {
            Log(nameof(OnActivated));

            _onActivated.Invoke();
            OnActivated?.Invoke(this);
        }
        private void Invoke_OnFinishedBlendIn()
        {
            Log(nameof(OnFinishedBlendIn));

            _onFinishedBlendIn.Invoke();
            OnFinishedBlendIn?.Invoke(this);
        }
        private void Invoke_OnStartedBlendOut()
        {
            Log(nameof(OnStartedBlendOut));

            _onStartedBlendOut.Invoke();
            OnStartedBlendOut?.Invoke(this);
        }
        private void Invoke_OnInactivated()
        {
            Log(nameof(OnInactivated));

            _onInactivated.Invoke();
            OnInactivated?.Invoke(this);
        }
    }
}
