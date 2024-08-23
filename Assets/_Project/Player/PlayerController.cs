using PF.PJT.Duet.Pawn;
using StudioScor.InputSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using static PF.PJT.Duet.Controller.IPlayerController;

namespace PF.PJT.Duet.Controller
{
    public interface IPlayerController
    {
        public delegate void PlayerChangeCharacterStateHandler(IPlayerController controller, ICharacter currentCharacter, ICharacter prevCharacter);
        public delegate void PlayerControllerCharacterStateHandler(IPlayerController controller, ICharacter character);
        public delegate void PlayerControllerStateHandler(IPlayerController playerController);
        
        public ICharacter CurrentCharacter { get; }
        public ICharacter NextCharacter { get; }

        public void AddCharacter(ICharacter addCharacter);
        public void RemoveCharacter(ICharacter removeCharacter);


        public event PlayerChangeCharacterStateHandler OnChangedCurrentCharacter;

        public event PlayerControllerCharacterStateHandler OnAddedCharacter;
        public event PlayerControllerCharacterStateHandler OnRemovedCharacter;

    }

    public class PlayerController : BaseMonoBehaviour, IPlayerController
    {
        [Header(" [ Player Controller ] ")]
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private LookAxisComponent _lookAxisComp;
        [SerializeField] private GameObject _subPlayerController;

        [Header(" Input Reference ")]
        [SerializeField] private InputActionReference _changeActionReference;
        [SerializeField] private InputActionReference _moveActionReference;
        [SerializeField] private InputActionReference _lookActionReference;
        [SerializeField] private InputActionReference _attackActionReference;
        [SerializeField] private InputActionReference _skillActionReference;
        [SerializeField] private InputActionReference _dashActionReference;

        [Header(" GameEvents ")]
        [SerializeField] private GameEvent _onDeadAllCharacter;

        [Header(" Change Test ")]
        [SerializeField] private GameObject _characterA;
        [SerializeField] private GameObject _characterB;

        private Camera _mainCamera;
        
        private IControllerSystem _controllerSystem;
        private IControllerSystem _nextControllerSystem;

        private InputAction _moveInputAction;
        private InputAction _lookInputAction;
        private InputAction _changeInputAction;
        private InputAction _attackInputAction;
        private InputAction _skillInputAction;
        private InputAction _dashInputAction;

        private Vector2 _inputMoveDirection;
        private float _inputMoveStrength;
        private Vector2 _inputLookDirection;

        private ICharacter _currentCharacter;
        private ICharacter _nextCharacter;

        public event PlayerChangeCharacterStateHandler OnChangedCurrentCharacter;

        public event PlayerControllerCharacterStateHandler OnAddedCharacter;
        public event PlayerControllerCharacterStateHandler OnRemovedCharacter;

        public ICharacter CurrentCharacter => _currentCharacter;
        public ICharacter NextCharacter => _nextCharacter;

        private void Awake()
        {
            InitPlayerController();
        }
        private void OnDestroy()
        {
            ResetInput();
            ResetController();
            ResetCharacter();
        }

        private void Start()
        {
            SetupInput();
            SetupControllerSystem();
            SetupCharacter();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            UpdateController(deltaTime);
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;

            FollowPawn(deltaTime);
            _lookAxisComp.Tick(deltaTime);
        }


        private void InitPlayerController()
        {
            _mainCamera = Camera.main;

            _controllerSystem = gameObject.GetControllerSystem();
            _nextControllerSystem = _subPlayerController.GetControllerSystem();
        }

        #region Controller
        private void SetupControllerSystem()
        {
            
            _controllerSystem.OnPossessedPawn += _controllerSystem_OnPossessedPawn;
            _controllerSystem.OnUnPossessedPawn += _controllerSystem_OnUnPossessedPawn;
        }
        private void ResetController()
        {
            if(_controllerSystem is not null)
            {
                _controllerSystem.OnPossessedPawn -= _controllerSystem_OnPossessedPawn;
                _controllerSystem.OnUnPossessedPawn -= _controllerSystem_OnUnPossessedPawn;

                _controllerSystem = null;
            }

            if(_subPlayerController is not null)
            {
                _subPlayerController = null;
            }
        }

        private void SetCurrentCharacter(ICharacter nextCharacter)
        {
            if (_currentCharacter == nextCharacter)
                return;

            if (!nextCharacter.CanAppear())
                return;

            ReleaseAllInput();

            _nextCharacter = _currentCharacter;
            _currentCharacter = nextCharacter;

            var prevPawn = _nextCharacter.gameObject.GetPawnSystem();
            var pawn = _currentCharacter.gameObject.GetPawnSystem();

            _controllerSystem.OnPossess(pawn);
            _nextControllerSystem.OnPossess(prevPawn);

            _currentCharacter.Teleport(_nextCharacter.transform.position, _nextCharacter.transform.rotation);

            if(_nextCharacter.CanLeave())
                _nextCharacter.Leave();

            _currentCharacter.Appear();

            Inovke_OnChangedCurrentCharacter(_nextCharacter);
        }

        private void UpdateController(float deltaTime)
        {
            var moveDirection = _inputMoveDirection.TurnDirectionFromY(_mainCamera.transform);

            _controllerSystem.SetMoveDirection(moveDirection, _inputMoveStrength);
            _controllerSystem.SetTurnDirection(moveDirection);

            _controllerSystem.SetLookPosition(_mainCamera.transform.position + _mainCamera.transform.forward * 10f);
        }

        private void _controllerSystem_OnPossessedPawn(IControllerSystem controller, IPawnSystem pawn)
        {
        }
        private void _controllerSystem_OnUnPossessedPawn(IControllerSystem controller, IPawnSystem pawn)
        {
        }

        #endregion

        #region Input 
        private void SetupInput()
        {
            var actionMap = _playerInput.currentActionMap;

            _moveInputAction = actionMap.FindAction(_moveActionReference.action.name);
            _moveInputAction.performed += _moveInputAction_performed;
            _moveInputAction.canceled += _moveInputAction_canceled;

            _lookInputAction = actionMap.FindAction(_lookActionReference.action.name);
            _lookInputAction.performed += _lookInputAction_performed;
            _lookInputAction.canceled += _lookInputAction_canceled;


            _changeInputAction = actionMap.FindAction(_changeActionReference.action.name);
            _changeInputAction.started += ChangeAction_started;

            _attackInputAction = actionMap.FindAction(_attackActionReference.action.name);
            _attackInputAction.started += AttackAction_started;
            _attackInputAction.canceled += AttackAction_canceled;

            _skillInputAction = actionMap.FindAction(_skillActionReference.action.name);
            _skillInputAction.started += _skillInputAction_started;
            _skillInputAction.canceled += _skillInputAction_canceled;

            _dashInputAction = actionMap.FindAction(_dashActionReference.action.name);
            _dashInputAction.started += DashAction_started;
            _dashInputAction.canceled += DashAction_canceled;
        }
        private void ResetInput()
        {
            if (_moveInputAction is not null)
            {
                _moveInputAction.performed -= _moveInputAction_performed;
                _moveInputAction.canceled -= _moveInputAction_canceled;

                _moveInputAction = null;
            }

            if (_lookInputAction is not null)
            {
                _lookInputAction.performed -= _lookInputAction_performed;
                _lookInputAction.canceled -= _lookInputAction_canceled;

                _lookInputAction = null;
            }

            if (_changeInputAction is not null)
            {
                _changeInputAction.started -= ChangeAction_started;

                _changeInputAction = null;
            }

            if (_attackInputAction is not null)
            {
                _attackInputAction.started -= AttackAction_started;
                _attackInputAction.canceled -= AttackAction_canceled;

                _attackInputAction = null;
            }

            if(_skillInputAction is not null)
            {
                _skillInputAction.started -= _skillInputAction_started;
                _skillInputAction.canceled -= _skillInputAction_canceled;

                _skillInputAction = null;
            }

            if(_dashInputAction is not null)
            {
                _dashInputAction.started -= DashAction_started;
                _dashInputAction.canceled -= DashAction_canceled;

                _dashInputAction = null;
            }
        }

        private void ReleaseAllInput()
        {
            if (_currentCharacter is null)
                return;

            if (_attackInputAction.IsPressed())
                _currentCharacter.SetInputAttack(false);

            if (_skillInputAction.IsPressed())
                _currentCharacter.SetInputSkill(false);

            if (_dashInputAction.IsPressed())
                _currentCharacter.SetInputDash(false);
        }

        private void _lookInputAction_performed(InputAction.CallbackContext obj)
        {
            _inputLookDirection = obj.ReadValue<Vector2>();
            _lookAxisComp.LookAxis.InputLookAxis(_inputLookDirection);
        }
        private void _lookInputAction_canceled(InputAction.CallbackContext obj)
        {
            _inputLookDirection = Vector2.zero;
            _lookAxisComp.LookAxis.InputLookAxis(_inputLookDirection);
        }


        private void _moveInputAction_performed(InputAction.CallbackContext obj)
        {
            _inputMoveDirection = obj.ReadValue<Vector2>();
            _inputMoveStrength = _inputMoveDirection.magnitude;
        }
        private void _moveInputAction_canceled(InputAction.CallbackContext obj)
        {
            _inputMoveDirection = Vector2.zero;
            _inputMoveStrength = 0f;
        }

        private void ChangeAction_started(InputAction.CallbackContext obj)
        {
            SetCurrentCharacter(_nextCharacter);
        }
        private void AttackAction_started(InputAction.CallbackContext obj)
        {
            if (_currentCharacter is null)
                return;

            _currentCharacter.SetInputAttack(true);
        }

        private void AttackAction_canceled(InputAction.CallbackContext obj)
        {
            if (_currentCharacter is null)
                return;

            _currentCharacter.SetInputAttack(false);
        }

        private void _skillInputAction_started(InputAction.CallbackContext obj)
        {
            if (_currentCharacter is null)
                return;

            _currentCharacter.SetInputSkill(true);
        }
        private void _skillInputAction_canceled(InputAction.CallbackContext obj)
        {
            if (_currentCharacter is null)
                return;

            _currentCharacter.SetInputSkill(false);
        }


        private void DashAction_started(InputAction.CallbackContext obj)
        {
            if (_currentCharacter is null)
                return;

            _currentCharacter.SetInputDash(true);
        }
        private void DashAction_canceled(InputAction.CallbackContext obj)
        {
            if (_currentCharacter is null)
                return;

            _currentCharacter.SetInputDash(false);
        }

        #endregion

        private void FollowPawn(float deltaTime)
        {
            if (!_controllerSystem.IsPossess)
                return;

            transform.SetPositionAndRotation(_controllerSystem.Pawn.transform.position, _controllerSystem.Pawn.transform.rotation);
        }

        private void SetupCharacter()
        {
            if (_controllerSystem.IsPossess)
            {
                var character = _controllerSystem.Pawn.gameObject.GetComponent<ICharacter>();

                AddCharacter(character);


                var nextCharacterActor = character.gameObject != _characterA ? _characterA : _characterB;
                var nextCharacter = nextCharacterActor.GetComponent<ICharacter>();
                
                AddCharacter(nextCharacter);
            }
            else
            {
                var currentCharacter = _characterA.GetComponent<ICharacter>();
                AddCharacter(currentCharacter);


                var nextCharacter = _characterB.GetComponent<ICharacter>();
                AddCharacter(nextCharacter);
            }

            Inovke_OnChangedCurrentCharacter(null);

            _nextCharacter.Leave();
        }

        private void ResetCharacter()
        {
            if(_currentCharacter is not null)
            {
                _currentCharacter.OnDead -= _currentCharacter_OnDead;
                _currentCharacter = null;
            }

            if(_nextCharacter is not null)
            {
                _nextCharacter.OnDead -= _currentCharacter_OnDead;
                _nextCharacter = null;
            }
        }
        public void AddCharacter(ICharacter addCharacter)
        {
            if (_currentCharacter is null)
            {
                OnAddCharacter(addCharacter);
                
                _currentCharacter = addCharacter;

                var currentPawn = addCharacter.gameObject.GetPawnSystem();
                _controllerSystem.OnPossess(currentPawn);

            }
            else if (_nextCharacter is null)
            {
                OnAddCharacter(addCharacter);
                
                _nextCharacter = addCharacter;

                var nextPawn = addCharacter.gameObject.GetPawnSystem();
                _nextControllerSystem.OnPossess(nextPawn);
            }

            Invoke_OnAddedCharacter(addCharacter);
        }

        public void RemoveCharacter(ICharacter removeCharacter)
        {
            if(removeCharacter == _currentCharacter)
            {
                OnRemoveCharacter(_currentCharacter);

                var currentPawn = removeCharacter.gameObject.GetPawnSystem();

                _currentCharacter = null;
                
                _controllerSystem.UnPossess(currentPawn);
            }
            else if (removeCharacter == _nextCharacter)
            {
                OnRemoveCharacter(_nextCharacter);

                var nextPawn = removeCharacter.gameObject.GetPawnSystem();
                _nextCharacter = null;

                _nextControllerSystem.OnPossess(nextPawn);
            }

            Invoke_OnRemoveCharacter(removeCharacter);
        }

        private void OnAddCharacter(ICharacter character)
        {
            character.OnDead += _currentCharacter_OnDead;
        }
        private void OnRemoveCharacter(ICharacter character)
        {
            character.OnDead -= _currentCharacter_OnDead;
        }
        private void _currentCharacter_OnDead(ICharacter character)
        {
            if (_nextCharacter is not null && !_nextCharacter.IsDead)
            {
                SetCurrentCharacter(_nextCharacter);
            }
            else
            {
                _onDeadAllCharacter.Invoke();
            }
        }
        private void Invoke_OnAddedCharacter(ICharacter addedCharacter)
        {
            Log($"{nameof(OnAddedCharacter)} - {addedCharacter.gameObject}");

            OnAddedCharacter?.Invoke(this, addedCharacter);
        }
        
        private void Invoke_OnRemoveCharacter(ICharacter removedCharacter)
        {
            Log($"{nameof(OnRemovedCharacter)} - {removedCharacter.gameObject}");

            OnRemovedCharacter?.Invoke(this, removedCharacter);
        }

        private void Inovke_OnChangedCurrentCharacter(ICharacter prevCharacter)
        {
            Log($"{nameof(OnChangedCurrentCharacter)} - Current Character {(_currentCharacter is null ? "Null" : _currentCharacter.gameObject)} || Prev Characvter {(prevCharacter is null ? "Null" : prevCharacter.gameObject)}");

            OnChangedCurrentCharacter?.Invoke(this, _currentCharacter, prevCharacter);
        }
    }

}