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
        private bool _isPlaying = false;

        private void Awake()
        {
            var playerinput = PlayerInput.all.ElementAtOrDefault(0);

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
            Log(nameof(OnUIInputControl));

            _isPlaying = true;

            var playerinput = PlayerInput.all.ElementAtOrDefault(0);

            if (playerinput)
            {
                playerinput.DeactivateInput();
            }
            
            SetCursorState(true);
        }

        private void EndUIInputControl()
        {
            Log(nameof(EndUIInputControl));

            _isPlaying = false;

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
