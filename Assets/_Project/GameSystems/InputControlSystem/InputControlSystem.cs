using StudioScor.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace PF.PJT.Duet
{
    public class InputControlSystem : BaseMonoBehaviour
    {
        [Header(" [ Input Control System ] ")]
        [SerializeField] private GameObjectListVariable _activeUIVariable;
        [SerializeField] private string _inGameActionMap = "Player";
        [SerializeField] private string _uiActionMap = "UI";

        private bool _isPlaying = false;
        private InputActionMap _inGameMap;
        private InputActionMap _uiMap;

        private void Awake()
        {
            _activeUIVariable.OnAdded += _activeUIVariable_OnAdded;
            _activeUIVariable.OnRemoved += _activeUIVariable_OnRemoved;
        }

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
            if (_activeUIVariable.Values.Count > 0)
                OnUIInputControl();
            else
                EndUIInputControl();
        }

        private void OnUIInputControl()
        {
            _isPlaying = true;

            var playerinput = PlayerInput.all.ElementAtOrDefault(0);

            if (playerinput)
                playerinput.SwitchCurrentActionMap(_uiActionMap);

            SetCursorState(true);
        }
        private void EndUIInputControl()
        {
            _isPlaying = false;

            var playerinput = PlayerInput.all.ElementAtOrDefault(0);

            if (playerinput)
                playerinput.SwitchCurrentActionMap(_inGameActionMap);

            SetCursorState(false);
        }

        private void SetCursorState(bool visible)
        {
            if(visible)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                if(_isPlaying)
                {
                    SetCursorState(true);
                }
                else
                {
                    SetCursorState(false);
                }
                
            }
            else
            {
                SetCursorState(true);
            }
        }

        private void _activeUIVariable_OnAdded(ListVariableObject<GameObject> variable, GameObject value)
        {
            if (_isPlaying)
                return;

            OnUIInputControl();
        }
        private void _activeUIVariable_OnRemoved(ListVariableObject<GameObject> variable, GameObject value)
        {
            if (!_isPlaying)
                return;

            if (variable.Values.Count > 0)
                return;

            EndUIInputControl();
        }


        
    }
}
