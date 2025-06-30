using PF.PJT.Duet.Controller;
using PF.PJT.Duet.Pawn;
using StudioScor.AbilitySystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Controller/Create Player Character Controller")]
    public class CreatePlayerCharacterController : BaseMonoBehaviour
    {
        public delegate void CreatePlayerCharacterEventHandler(CreatePlayerCharacterController createPlayerCharacterController);

        [Header(" [ Create Player Character System ] ")]
        [SerializeField] private GameObject _createCharacterSystemActor;
        [SerializeField] private GameObject _createCharacterDisplayActor;
        [SerializeField] private CreateCharacterButton[] _createCharacterButtons;

        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private Transform _spawnPoint;

        [Header(" State Machine ")]
        [SerializeField] private CreatePlayerCharacterControllerState.StateMachine _stateMachine;
        [SerializeField] private CreatePlayerCharacterControllerState _inactiveState;
        [SerializeField] private CreatePlayerCharacterControllerState _waitForActivateState;
        [SerializeField] private CreatePlayerCharacterControllerState _waitForSelectState;
        [SerializeField] private CreatePlayerCharacterControllerState _waitForDeactivateState;

        [Header(" Input ")]
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private EBlockInputState _blockInput = EBlockInputState.Game;

        [Header(" HUD ")]
        [SerializeField] private HUDVisibilityController _hudVisibility;
        [SerializeField] private bool _hideHUD = true;

        [Header(" Evetns ")]
        [SerializeField] private ToggleableUnityEvent _onActivated;
        [SerializeField] private ToggleableUnityEvent _onDeactivated;
        [SerializeField] private ToggleableUnityEvent _onCreatedCharacter;

        private IPlayerController _playerController;
        private ICreateCharacterSystem _createCharacterSystem;
        private ICreateCharacterDisplay _createCharacterDisplay;

        private bool _isPlaying = false;
        private bool _wasInit = false;

        public ICreateCharacterDisplay CreateCharacterDisplay => _createCharacterDisplay;
        public IReadOnlyCollection<CreateCharacterButton> CreateCharacterButtons => _createCharacterButtons;

        public event CreatePlayerCharacterEventHandler OnActivated;
        public event CreatePlayerCharacterEventHandler OnDeactivated;
        public event CreatePlayerCharacterEventHandler OnCreatedCharacter;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_createCharacterSystemActor)
            {
                _createCharacterSystemActor = gameObject.GetGameObjectByTypeInParentOrChildren<ICreateCharacterSystem>();
            }

            if(!_createCharacterDisplayActor)
            {
                _createCharacterDisplayActor = gameObject.GetGameObjectByTypeInParentOrChildren<ICreateCharacterDisplay>();
            }

            if(!_inputBlocker)
            {
                _inputBlocker = SUtility.FindAssetByType<InputBlocker>();
            }

            if(!_hudVisibility)
            {
                _hudVisibility = SUtility.FindAssetByType<HUDVisibilityController>();
            }
#endif
        }
        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
            if (_createCharacterButtons is not null)
            {
                foreach (var createCharacterButton in _createCharacterButtons)
                {
                    if (!createCharacterButton)
                        continue;

                    createCharacterButton.OnSubmited -= CreateCharacterButton_OnSubmited;
                }
            }

            if(_playerManager)
            {
                _playerManager.OnChangedPlayerController -= _playerManager_OnChangedPlayerController;
            }

            if(_createCharacterDisplay is not null)
            {
                _createCharacterDisplay.OnFinishedBlendIn -= CreateCharacterDisplay_OnFinishedBlendIn;
                _createCharacterDisplay.OnDeactivated -= CreateCharacterDisplay_OnDeactivated;

                _createCharacterDisplay = null;
            }

            _inputBlocker.UnblockInput(this);

            if (_hideHUD)
                _hudVisibility.RequestShow(this);
        }

        public void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            if(_playerManager.HasPlayerController)
                _playerController = _playerManager.PlayerController.gameObject.GetComponent<IPlayerController>();
            
            _playerManager.OnChangedPlayerController += _playerManager_OnChangedPlayerController;

            _createCharacterSystem = _createCharacterSystemActor.GetComponent<ICreateCharacterSystem>();
            _createCharacterDisplay = _createCharacterDisplayActor.GetComponent<ICreateCharacterDisplay>();

            _createCharacterSystem.Init();

            _createCharacterDisplay.OnFinishedBlendIn += CreateCharacterDisplay_OnFinishedBlendIn;
            _createCharacterDisplay.OnDeactivated += CreateCharacterDisplay_OnDeactivated;
            _createCharacterDisplay.Init();
            _createCharacterDisplay.Deactivate();

            foreach (var createCharacterButton in _createCharacterButtons)
            {
                createCharacterButton.OnSubmited += CreateCharacterButton_OnSubmited;
                createCharacterButton.Init();
            }

            _stateMachine.Start();
        }

        [ContextMenu(nameof(Activate))]
        public void Activate()
        {
            Log($"{nameof(Activate)}");

            _isPlaying = true;

            _inputBlocker.BlockInput(this, _blockInput);
            if (_hideHUD)
                _hudVisibility.RequestHide(this);

            _createCharacterSystem.CreateCharacterDatas(_createCharacterButtons.Length);
            _createCharacterDisplay.SetCharacterDatas(_createCharacterSystem.CharacterDatas);

            _stateMachine.TrySetState(_waitForActivateState);

            Invoke_OnActivated();
        }

        public void Deactivate()
        {
            Log($"{nameof(Deactivate)}");

            _stateMachine.TrySetState(_waitForDeactivateState);

            _createCharacterSystem.ClearCharacterDatas();
        }

        public void AddPlayerCharacter(CharacterData characterData)
        {
            var character = characterData.CreateCharacter();

            character.Teleport(_spawnPoint.position, _spawnPoint.rotation);

            _playerController.AddCharacter(character);

            character.OnSpawn();
        }

        private void OnFinishedBlendInDisplay()
        {
            _stateMachine.TrySetState(_waitForSelectState);
        }
        private void OnDeactivatedDisplay()
        {
            _stateMachine.TrySetState(_inactiveState);

            _inputBlocker.UnblockInput(this);
            if(_hideHUD)
                _hudVisibility.RequestShow(this);

            _isPlaying = false;

            Invoke_OnDeactivated();
        }

        private void UpdatePlayerController()
        {
            if (_playerManager.HasPlayerController)
            {
                _playerController = _playerManager.PlayerController.gameObject.GetComponent<IPlayerController>();
            }
            else
            {
                _playerController = null;
            }
        }
        private void OnSubmitedButton(CreateCharacterButton createCharacterButton)
        {
            if (_stateMachine.IsPlaying)
                _stateMachine.CurrentState.SubmitedButton(createCharacterButton);
        }


        private void CreateCharacterDisplay_OnFinishedBlendIn(ICreateCharacterDisplay createCharacterDisplay)
        {
            OnFinishedBlendInDisplay();
        }
        private void CreateCharacterDisplay_OnDeactivated(ICreateCharacterDisplay createCharacterDisplay)
        {
            OnDeactivatedDisplay();
        }

        private void _playerManager_OnChangedPlayerController(PlayerManager playerManager, IControllerSystem currentController, IControllerSystem prevController = null)
        {
            UpdatePlayerController();
        }

        private void CreateCharacterButton_OnSubmited(CreateCharacterButton createCharacterButton)
        {
            OnSubmitedButton(createCharacterButton);
        }


        private void Invoke_OnActivated()
        {
            Log(nameof(OnActivated));

            _onActivated.Invoke();
            OnActivated?.Invoke(this);
        }
        private void Invoke_OnDeactivated()
        {
            Log(nameof(OnDeactivated));

            _onDeactivated.Invoke();
            OnDeactivated?.Invoke(this);
        }

        private void Invoke_OnCreatedCharacter()
        {
            Log(nameof(OnCreatedCharacter));

            _onCreatedCharacter.Invoke();
            OnCreatedCharacter?.Invoke(this);
        }
    }
}
