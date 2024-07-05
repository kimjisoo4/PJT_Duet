using PF.PJT.Duet.Pawn;
using StudioScor.InputSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PF.PJT.Duet.Controller
{
    public interface IPlayerController
    {
        public delegate void PlayerControllerStateHandler(IPlayerController playerController);
        
        public bool IsPlayingMainCharacter { get; }
        public ICharacter CurrentCharacter { get; }
        public ICharacter MainCharacter { get; }
        public ICharacter SubCharacter { get; }

        public event PlayerControllerStateHandler OnChangedCurrentCharacter;
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

        [Header(" Change Test ")]
        [SerializeField] private GameObject _characterA;
        [SerializeField] private GameObject _characterB;

        [Header(" Cursor Test ")]
        [SerializeField] private bool _useHideCursor = true; 

        private Camera _mainCamera;
        
        private IControllerSystem _controllerSystem;
        private IControllerSystem _subControllerSystem;

        private InputAction _moveInputAction;
        private InputAction _lookInputAction;
        private InputAction _attackInputAction;
        private InputAction _skillInputAction;
        private InputAction _dashInputAction;

        private Vector2 _inputMoveDirection;
        private float _inputMoveStrength;
        private Vector2 _inputLookDirection;

        private ICharacter _mainCharacter;
        private ICharacter _subCharacter;
        private ICharacter _currentCharacter;

        public event IPlayerController.PlayerControllerStateHandler OnChangedCurrentCharacter;

        public bool IsPlayingMainCharacter => CurrentCharacter == MainCharacter;
        public ICharacter MainCharacter => _mainCharacter;
        public ICharacter SubCharacter => _subCharacter;
        public ICharacter CurrentCharacter => _currentCharacter;

        private void Awake()
        {
            InitPlayerController();
        }
        private void OnDestroy()
        {
            ResetInput();
            ResetController();
        }

        private void Start()
        {
            SetupInput();
            SetupControllerSystem();
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

        private void OnApplicationFocus(bool focus)
        {
            if (!_useHideCursor)
                return;

            if(focus)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private void InitPlayerController()
        {
            _mainCamera = Camera.main;

            _controllerSystem = gameObject.GetControllerSystem();
            _subControllerSystem = _subPlayerController.GetControllerSystem();

            _mainCharacter = _characterA.GetComponent<ICharacter>();
            _subCharacter = _characterB.GetComponent<ICharacter>();
        }

        #region Controller
        private void SetupControllerSystem()
        {
            if(_controllerSystem.IsPossess)
            {
                var character = _controllerSystem.Pawn.gameObject.GetComponent<ICharacter>();

                _currentCharacter = character;
            }
            else
            {
                var mainPawn = _mainCharacter.gameObject.GetPawnSystem();
                var subPawn = _subCharacter.gameObject.GetPawnSystem();

                _controllerSystem.OnPossess(mainPawn);
                _subControllerSystem.OnPossess(subPawn);

                _currentCharacter = _mainCharacter;
            }

            Inovke_OnChangedCurrentCharacter();


            if(_currentCharacter != _mainCharacter)
            {
                _mainCharacter.TryLeave();
            }
            else
            {
                _subCharacter.TryLeave();
            }

            _controllerSystem.OnPossessedPawn += _controllerSystem_OnPossessedPawn;
            _controllerSystem.OnUnPossessedPawn += _controllerSystem_OnUnPossessedPawn;
        }
        private void ResetController()
        {
            if(_controllerSystem is not null)
            {
                _controllerSystem.OnPossessedPawn -= _controllerSystem_OnPossessedPawn;
                _controllerSystem.OnUnPossessedPawn -= _controllerSystem_OnUnPossessedPawn;
            }
        }

        
        private void ChangeCharacter()
        {
            var prevCharacter = _currentCharacter;
            var nextCharacter = _currentCharacter != _mainCharacter ? _mainCharacter : _subCharacter;

            if (!prevCharacter.TryLeave())
                return;

            ReleaseAllInput();

            _currentCharacter = nextCharacter;

            var prevPawn = prevCharacter.gameObject.GetPawnSystem();
            var pawn = _currentCharacter.gameObject.GetPawnSystem();

            _controllerSystem.OnPossess(pawn);
            _subControllerSystem.OnPossess(prevPawn);

            _currentCharacter.Teleport(prevCharacter.transform.position, prevCharacter.transform.rotation);

            _currentCharacter.Appear();

            Inovke_OnChangedCurrentCharacter();
        }

        private void UpdateController(float deltaTime)
        {
            var moveDirection = _inputMoveDirection.TurnDirectionFromY(_mainCamera.transform);

            _controllerSystem.SetMoveDirection(moveDirection, _inputMoveStrength);
            _controllerSystem.SetTurnDirection(moveDirection);

            _controllerSystem.SetLookPosition(_mainCamera.transform.TransformPoint(Vector3.forward * 10f));
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


            var changeAction = actionMap.FindAction(_changeActionReference.action.name);
            changeAction.started += ChangeAction_started;

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
            if (!_playerInput)
                return;

            var actionMap = _playerInput.currentActionMap;

            if (actionMap is null)
                return;

            _moveInputAction.performed -= _moveInputAction_performed;
            _moveInputAction.canceled -= _moveInputAction_canceled;

            _lookInputAction.performed -= _lookInputAction_performed;
            _lookInputAction.canceled -= _lookInputAction_canceled;


            var changeAction = actionMap.FindAction(_changeActionReference.action.name);
            changeAction.started -= ChangeAction_started;

            _attackInputAction.started -= AttackAction_started;
            _attackInputAction.canceled -= AttackAction_canceled;

            _skillInputAction.started -= _skillInputAction_started;
            _skillInputAction.canceled -= _skillInputAction_canceled;

            _dashInputAction.started -= DashAction_started;
            _dashInputAction.canceled -= DashAction_canceled;
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
            ChangeCharacter();
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

        private void Inovke_OnChangedCurrentCharacter()
        {
            Log($"{nameof(OnChangedCurrentCharacter)}");

            OnChangedCurrentCharacter?.Invoke(this);
        }
    }

}