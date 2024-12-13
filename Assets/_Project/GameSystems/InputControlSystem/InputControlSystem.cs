using StudioScor.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PF.PJT.Duet
{
    public class InputControlSystem : BaseMonoBehaviour
    {
        [Header(" [ Input Control System ] ")]
        [SerializeField] private GameObjectListVariable _activeUIVariable;
        [SerializeField][Readonly] private EBoolean _state = EBoolean.None;

        public bool UseUIInput => _state == EBoolean.True;
        public bool UseGameInput => _state == EBoolean.False;

#if UNITY_EDITOR
        [SerializeField][Readonly]private bool _cursorVisible;
        [SerializeField][Readonly]private CursorLockMode _cursorLockMode;
#endif

        private void OnDestroy()
        {
            if(_activeUIVariable)
            {
                _activeUIVariable.OnAdded -= _activeUIVariable_OnAdded;
                _activeUIVariable.OnRemoved -= _activeUIVariable_OnRemoved;
            }
        }

        private void Start()
        {
            UpdateUIControl();

            _activeUIVariable.OnAdded += _activeUIVariable_OnAdded;
            _activeUIVariable.OnRemoved += _activeUIVariable_OnRemoved;
        }

        private void UpdateUIControl()
        {
            if (_activeUIVariable.Values.Count > 0)
                OnUIInputControl();
            else
                EndUIInputControl();
        }

        private void OnUIInputControl()
        {
            if (UseUIInput)
                return;

            _state = EBoolean.True;

            Log(nameof(OnUIInputControl));

            var playerinput = PlayerInput.all.ElementAtOrDefault(0);

            if (playerinput)
            {
                playerinput.DeactivateInput();
            }
            
            SetCursorState(true);
        }

        private void EndUIInputControl()
        {
            if (UseGameInput)
                return;

            _state = EBoolean.False;

            Log(nameof(EndUIInputControl));

            var playerinput = PlayerInput.all.ElementAtOrDefault(0);

            if (playerinput)
            {
                playerinput.ActivateInput();
            }

            SetCursorState(false);
        }

        private void SetCursorState(bool visible)
        {
            if(visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

#if UNITY_EDITOR
            _cursorVisible = Cursor.visible;
            _cursorLockMode = Cursor.lockState;
#endif

        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                switch (_state)
                {
                    case EBoolean.None:
                        break;
                    case EBoolean.True:
                        SetCursorState(true);
                        break;
                    case EBoolean.False:
                        SetCursorState(false);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                SetCursorState(true);
            }
        }

        private void _activeUIVariable_OnAdded(ListVariableObject<GameObject> variable, GameObject value)
        {
            UpdateUIControl();
        }
        private void _activeUIVariable_OnRemoved(ListVariableObject<GameObject> variable, GameObject value)
        {
            UpdateUIControl();
        }
    }
}
