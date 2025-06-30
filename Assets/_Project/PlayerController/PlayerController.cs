using PF.PJT.Duet.Pawn;
using StudioScor.InputSystem;
using StudioScor.PlayerSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
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
        
        public IReadOnlyList<ICharacter> Characters { get; }
        public ICharacter CurrentCharacter { get; }
        public ICharacter NextCharacter { get; }

        public void AddCharacter(ICharacter addCharacter);
        public void RemoveCharacter(ICharacter removeCharacter);
        public void SetCurrentCharacter(ICharacter nextCharacter, bool useAppearSkill);

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
        [SerializeField] private int _maxCharacterCount = 2;

        [Header(" Input Reference ")]
        [SerializeField] private InputActionReference _changeActionReference;
        [SerializeField] private InputActionReference _moveActionReference;
        [SerializeField] private InputActionReference _lookActionReference;
        [SerializeField] private InputActionReference _attackActionReference;
        [SerializeField] private InputActionReference _skillActionReference;
        [SerializeField] private InputActionReference _shiftActionReference;
        [SerializeField] private InputActionReference _dashActionReference;
        [SerializeField] private InputActionReference _menuActionReference;

        [Header(" GameEvents ")]
        [SerializeField] private GameEvent _onMenuActionPressed;
        [SerializeField] private GameEvent _onDeadAllCharacter;

        [Header(" Change Test ")]
        [ContextMenuItem("Swap Character", nameof(SwapCharacterAB))]
        [SerializeField] private GameObject _characterA;
        [ContextMenuItem("Swap Character", nameof(SwapCharacterAB))]
        [SerializeField] private GameObject _characterB;

        private Camera _mainCamera;
        private Camera MainCamera
        {
            get
            {
                if (!_mainCamera)
                    _mainCamera = _playerInput.camera;

                if (!_mainCamera)
                    _mainCamera = Camera.main;

                return _mainCamera;
            }
        }

        private IControllerSystem _controllerSystem;

        private InputAction _moveInputAction;
        private InputAction _lookInputAction;
        private InputAction _changeInputAction;
        private InputAction _attackInputAction;
        private InputAction _skillInputAction;
        private InputAction _dashInputAction;
        private InputAction _shiftInputAction;
        private InputAction _menuInputAction;

        private Vector2 _inputMoveDirection;
        private float _inputMoveStrength;
        private Vector2 _inputLookDirection;

        private readonly List<ICharacter> _characters = new();

        public event PlayerChangeCharacterStateHandler OnChangedCurrentCharacter;

        public event PlayerControllerCharacterStateHandler OnAddedCharacter;
        public event PlayerControllerCharacterStateHandler OnRemovedCharacter;

        public IReadOnlyList<ICharacter> Characters => _characters;
        public ICharacter PrevCharacter => _prevCharacter;
        public ICharacter CurrentCharacter => _currentCharacter;
        public ICharacter NextCharacter => _nextCharacter;

        private int _currentCharacterIndex = 0;

        private ICharacter _currentCharacter;
        private ICharacter _prevCharacter;
        private ICharacter _nextCharacter;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void SwapCharacterAB()
        {
#if UNITY_EDITOR
            var temp = _characterA;
            _characterA = _characterB;
            _characterB = temp;

            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
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
            _controllerSystem = gameObject.GetControllerSystem();
        }

        #region Controller

        private void SetupControllerSystem()
        {
            _controllerSystem.OnPossessedPawn += _controllerSystem_OnPossessedPawn;
            _controllerSystem.OnUnPossessedPawn += _controllerSystem_OnUnPossessedPawn;
        }
        private void ResetController()
        {
            Log(nameof(ResetController));

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

        private void UpdateController(float deltaTime)
        {
            var moveDirection = _inputMoveDirection.TurnDirectionFromY(MainCamera.transform);

            _controllerSystem.SetMoveDirection(moveDirection, _inputMoveStrength);
            _controllerSystem.SetTurnDirection(moveDirection);

            _controllerSystem.SetLookPosition(MainCamera.transform.position + MainCamera.transform.forward * 10f);
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
            Log("Setup Input");

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

            _shiftInputAction = actionMap.FindAction(_shiftActionReference.action.name);

            _menuInputAction = actionMap.FindAction(_menuActionReference.action.name);
            _menuInputAction.started += _menuInputAction_started;
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

            if (_dashInputAction is not null)
            {
                _dashInputAction.started -= DashAction_started;
                _dashInputAction.canceled -= DashAction_canceled;

                _dashInputAction = null;
            }

            if (_menuInputAction is not null)
            {
                _menuInputAction.started -= _menuInputAction_started;

                _menuInputAction = null;
            }
        }
        private bool _wasPressedShiftAttack;
        private bool _wasPressedShiftSkill;

        private void ReleaseAllInput()
        {
            if (CurrentCharacter is null)
                return;

            if (_attackInputAction.IsPressed())
            {
                if(_wasPressedShiftAttack)
                {
                    _wasPressedShiftAttack = false;
                    CurrentCharacter.SetInputSkill(1, false);
                }
                else
                {
                    CurrentCharacter.SetInputAttack(false);
                }
            }

            if (_skillInputAction.IsPressed())
            {
                if (_wasPressedShiftSkill)
                {
                    _wasPressedShiftSkill = false;
                    CurrentCharacter.SetInputSkill(2, false);
                }
                else
                {
                    CurrentCharacter.SetInputSkill(0, false);
                }
                    
            }

            if (_dashInputAction.IsPressed())
                CurrentCharacter.SetInputDash(false);
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
            ChangeNextCharacter();
        }
        private void AttackAction_started(InputAction.CallbackContext obj)
        {
            if (CurrentCharacter is null)
                return;

            if (_shiftInputAction.IsPressed())
            {
                _wasPressedShiftAttack = true;
                CurrentCharacter.SetInputSkill(1, true);
            }
            else
            {
                CurrentCharacter.SetInputAttack(true);
            }
            
        }



        private void AttackAction_canceled(InputAction.CallbackContext obj)
        {
            if (CurrentCharacter is null)
                return;


            if (_wasPressedShiftAttack)
            {
                _wasPressedShiftAttack = false;
                CurrentCharacter.SetInputSkill(1, false);
            }
            else
            {
                CurrentCharacter.SetInputAttack(false);
            }
        }

        private void _skillInputAction_started(InputAction.CallbackContext obj)
        {
            if (CurrentCharacter is null)
                return;

            if(_shiftInputAction.IsPressed())
            {
                _wasPressedShiftSkill = true;
                CurrentCharacter.SetInputSkill(2, true);
            }
            else
            {
                CurrentCharacter.SetInputSkill(0, true);
            }
        }
        private void _skillInputAction_canceled(InputAction.CallbackContext obj)
        {
            if (CurrentCharacter is null)
                return;


            if (_wasPressedShiftSkill)
            {
                _wasPressedShiftSkill = false;
                CurrentCharacter.SetInputSkill(2, false);
            }
            else
            {
                CurrentCharacter.SetInputSkill(0, false);
            }
        }


        private void DashAction_started(InputAction.CallbackContext obj)
        {
            if (CurrentCharacter is null)
                return;

            CurrentCharacter.SetInputDash(true);
        }
        private void DashAction_canceled(InputAction.CallbackContext obj)
        {
            if (CurrentCharacter is null)
                return;

            CurrentCharacter.SetInputDash(false);
        }
        private void _menuInputAction_started(InputAction.CallbackContext obj)
        {
            _onMenuActionPressed.Invoke();
        }

        #endregion

        private void FollowPawn(float deltaTime)
        {
            if (!_controllerSystem.IsPossessed)
                return;

            transform.SetPositionAndRotation(_controllerSystem.Pawn.transform.position, _controllerSystem.Pawn.transform.rotation);
        }

        private void SetupCharacter()
        {
            if (_characterA)
            {
                var character = _characterA.GetComponent<ICharacter>();
                AddCharacter(character);
            }

            if(_characterB)
            {
                var character = _characterB.GetComponent<ICharacter>();
                AddCharacter(character);
            }

            Invoke_OnChangedCurrentCharacter(null);

            if(_nextCharacter is not null)
            {
             //   _nextCharacter.Leave();
            }
        }

        private void ResetCharacter()
        {
            foreach (var character in _characters)
            {
                character.OnDead -= Character_OnDead;
            }

            _characters.Clear();
        }


        public void AddCharacter(ICharacter addCharacter)
        {
            if (addCharacter is null)
                return;

            if (_characters.Contains(addCharacter))
                return;

            _characters.Add(addCharacter);

            OnAddCharacter(addCharacter);

            UpdateSubCharacters();

            Invoke_OnAddedCharacter(addCharacter);

            int count = _characters.Count;
            int lastIndex = _characters.Count - 1;

            if (count > _maxCharacterCount)
            {
                for (int i = 0; i < count; i++)
                {
                    if (_characters[i].IsDead)
                    {
                        RemoveCharacter(_characters[i]);
                        break;
                    }
                    else if (i == lastIndex)
                    {
                        var currentCharacter = CurrentCharacter;

                        SetCurrentCharacter(addCharacter, false);

                        RemoveCharacter(currentCharacter);
                    }
                }
            }
            else if (count == 1)
            {
                _currentCharacterIndex = 0;

                SetCurrentCharacter(addCharacter, false);
            }
            else
            {
                addCharacter.Leave();
            }
        }

        public void RemoveCharacter(ICharacter removeCharacter)
        {
            if (!_characters.Contains(removeCharacter))
                return;

            _characters.Remove(removeCharacter);

            OnRemoveCharacter(removeCharacter);

            if(CurrentCharacter == removeCharacter)
            {
                SetCurrentCharacter(NextCharacter, true);
            }
            else
            {
                UpdateSubCharacters();
            }

            Invoke_OnRemoveCharacter(removeCharacter);
        }
        public void SetCurrentCharacter(ICharacter nextCharacter, bool useAppearSkill)
        {
            if (nextCharacter is null)
                return;

            if (_currentCharacter == nextCharacter)
                return;

            if (!nextCharacter.CanAppear())
                return;

            Log(nameof(SetCurrentCharacter));

            ReleaseAllInput();

            var prevCharacter = _currentCharacter;
            _currentCharacter = nextCharacter;

            UpdateSubCharacters();

            var pawn = _currentCharacter.gameObject.GetPawnSystem();
            _controllerSystem.Possess(pawn);

            if (prevCharacter is not null)
            {
                _currentCharacter.Teleport(prevCharacter.transform.position, prevCharacter.transform.rotation);

                if (prevCharacter.CanLeave())
                    prevCharacter.Leave();
            }

            _currentCharacter.Appear(useAppearSkill);

            Invoke_OnChangedCurrentCharacter(prevCharacter);
        }


        private void UpdateSubCharacters()
        {
            Log(nameof(UpdateSubCharacters));

            _currentCharacterIndex = _characters.IndexOf(_currentCharacter);

            int prevIndex = _currentCharacterIndex - 1;
            prevIndex = prevIndex >= 0 ? prevIndex : _characters.Count + prevIndex;

            int nextIndex = (_currentCharacterIndex + 1) % _characters.Count;

            var prevCharacter = _characters.ElementAtOrDefault(prevIndex);
            var nextCharacter = _characters.ElementAtOrDefault(nextIndex);

            if (prevCharacter is null || _currentCharacter != prevCharacter)
            {
                _prevCharacter = prevCharacter;
                Log($"Prev Character - {(_prevCharacter is not null ? _prevCharacter.CharacterInformationData.Name : "Null")}");
            }

            if (nextCharacter is null || _currentCharacter != nextCharacter)
            {
                _nextCharacter = nextCharacter;
                Log($"Next Character - {(_nextCharacter is not null ? _nextCharacter.CharacterInformationData.Name : "Null")}");
            }
        }
        private void ChangePrevCharacter()
        {
            if (_characters.Count <= 1)
                return;

            int index = _currentCharacterIndex - 1;
            index = index >= 0 ? index : _characters.Count + index;

            var character = _characters.ElementAtOrDefault(index);

            SetCurrentCharacter(character, true);
        }
        private void ChangeNextCharacter()
        {
            if (_characters.Count <= 1)
                return;

            int index = (_currentCharacterIndex + 1) % _characters.Count;
            var character = _characters.ElementAtOrDefault(index);

            SetCurrentCharacter(character, true);
        }

        

        private void OnAddCharacter(ICharacter character)
        {
            character.OnDead += Character_OnDead;
        }
        private void OnRemoveCharacter(ICharacter character)
        {
            character.OnDead -= Character_OnDead;
        }
        private void Character_OnDead(ICharacter character)
        {
            if(CurrentCharacter.IsDead)
            {
                if (NextCharacter is not null && !NextCharacter.IsDead)
                {
                    SetCurrentCharacter(NextCharacter, true);
                }
                else
                {
                    _onDeadAllCharacter.Invoke();
                }
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

        private void Invoke_OnChangedCurrentCharacter(ICharacter prevCharacter)
        {
            Log($"{nameof(OnChangedCurrentCharacter)} - Current Character {(CurrentCharacter is null ? "Null" : CurrentCharacter.gameObject)} || Prev Characvter {(prevCharacter is null ? "Null" : prevCharacter.gameObject)}");

            OnChangedCurrentCharacter?.Invoke(this, CurrentCharacter, prevCharacter);
        }
    }

}