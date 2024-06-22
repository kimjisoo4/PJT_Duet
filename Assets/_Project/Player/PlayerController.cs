using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using PF.PJT.Duet.Pawn;
using StudioScor.InputSystem;
using StudioScor.MovementSystem;

namespace PF.PJT.Duet.Controller
{

    public class PlayerController : BaseMonoBehaviour
    {
        [Header(" [ Player Controller ] ")]
        [SerializeField] private PlayerInput _playerInput;
        [SerializeField] private LookAxisComponent _lookAxisComp;

        [SerializeField] private InputActionReference _changeActionReference;
        [SerializeField] private InputActionReference _moveActionReference;
        [SerializeField] private InputActionReference _lookActionReference;
        [SerializeField] private InputActionReference _attackActionReference;
        [SerializeField] private InputActionReference _dashActionReference;

        [Header(" Change Test ")]
        [SerializeField] private Character _characterA;
        [SerializeField] private Character _characterB;

        private Camera _mainCamera;
        
        private IControllerSystem _controllerSystem;

        private InputAction _moveInputAction;
        private InputAction _lookInputAction;
        private InputAction _attackInputAction;
        private InputAction _dashInputAction;

        private Vector2 _inputMoveDirection;
        private float _inputMoveStrength;
        private Vector2 _inputLookDirection;

        private ICharacter _character;

        private void Awake()
        {
            _mainCamera = Camera.main;

            SetupInput();
            SetupController();
        }
        private void OnDestroy()
        {
            ResetInput();
            ResetController();
        }

        private void Start()
        {
            _characterB.TryLeave();
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

        #region Controller
        private void SetupController()
        {
            _controllerSystem = gameObject.GetControllerSystem();

            if(_controllerSystem.IsPossess)
            {
                var character = _controllerSystem.Pawn.gameObject.GetComponent<ICharacter>();

                SetCharacter(character);
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

        private void SetCharacter(ICharacter newCharacter)
        {
            if (_character is not null)
            {
                _character.TryLeave();
            }

            _character = newCharacter;

            if(_character is not null)
            {
                _character.Appear();
            }
        }

        private void UpdateController(float deltaTime)
        {
            var moveDirection = _inputMoveDirection.TurnDirectionFromY(_mainCamera.transform);

            _controllerSystem.SetMoveDirection(moveDirection, _inputMoveStrength);
            _controllerSystem.SetTurnDirection(moveDirection);
        }

        private void _controllerSystem_OnPossessedPawn(IControllerSystem controller, IPawnSystem pawn)
        {
            var character = _controllerSystem.Pawn.gameObject.GetComponent<ICharacter>();

            SetCharacter(character);
        }
        private void _controllerSystem_OnUnPossessedPawn(IControllerSystem controller, IPawnSystem pawn)
        {
            SetCharacter(null);
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

            _dashInputAction.started -= DashAction_started;
            _dashInputAction.canceled -= DashAction_canceled;
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
            // 전환이 가능한지 체크해야함

            ICharacter currentCharacter;
            ICharacter nextCharacter;

            if(_controllerSystem.Pawn.gameObject == _characterA.gameObject)
            {
                currentCharacter = _characterA;
                nextCharacter = _characterB;
            }
            else
            {   
                currentCharacter = _characterB;
                nextCharacter = _characterA;
            }

            if(currentCharacter.TryLeave())
            {
                nextCharacter.Teleport(currentCharacter.transform.position, currentCharacter.transform.rotation);

                var nextPawn = nextCharacter.gameObject.GetPawnSystem();

                _controllerSystem.OnPossess(nextPawn);
            }
        }


        private void AttackAction_started(InputAction.CallbackContext obj)
        {
            if (_character is null)
                return;

            _character.SetInputAttack(true);
        }

        private void AttackAction_canceled(InputAction.CallbackContext obj)
        {
            if (_character is null)
                return;

            _character.SetInputAttack(false);
        }
        private void DashAction_started(InputAction.CallbackContext obj)
        {
            if (_character is null)
                return;

            _character.SetInputDash(true);
        }
        private void DashAction_canceled(InputAction.CallbackContext obj)
        {
            if (_character is null)
                return;

            _character.SetInputDash(false);
        }

        #endregion

        private void FollowPawn(float deltaTime)
        {
            if (!_controllerSystem.IsPossess)
                return;

            transform.SetPositionAndRotation(_controllerSystem.Pawn.transform.position, _controllerSystem.Pawn.transform.rotation);
        }
    }

}