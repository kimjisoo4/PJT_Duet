using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/Utility/new InputBlocker", fileName = "InputBlocker")]
    public class InputBlocker : BaseScriptableObject
    {
        public delegate void InputBlockerEventHandler(InputBlocker inputBlocker, EBlockInputState newInputState, EBlockInputState prevInputState);

        private readonly Dictionary<object, EBlockInputState> _inputBlocks = new();

        [SerializeField][SReadOnly]private EBlockInputState _inputState;
        public EBlockInputState InputState
        {
            get => _inputState;
            private set
            {
                if (_inputState == value)
                    return;

                var prevValue = _inputState;
                _inputState = value;

                RaiseOnInputStateChanged(prevValue);
            }
        }

#if UNITY_EDITOR
        [SerializeField][SReadOnly]private List<string> EDITOR_Requests = new();
#endif

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void UpdateDebug()
        {
#if UNITY_EDITOR
            EDITOR_Requests.Clear();
            EDITOR_Requests.AddRange(_inputBlocks.Keys.ToList().ConvertAll( x => x.ToString()));
#endif
        }

        public bool IsUIEnabled => _inputState.HasFlag(EBlockInputState.UI);
        public bool IsGameEnabled => _inputState.HasFlag(EBlockInputState.Game);

        public event InputBlockerEventHandler OnInputStateChanged;

        protected override void OnReset()
        {
            base.OnReset();

            _inputBlocks.Clear();
            _inputState = EBlockInputState.None;

            OnInputStateChanged = null;
        }

        public void BlockInput(object key, EBlockInputState disableFlags)
        {
            if (key == null) 
                return;
            
            if (_inputBlocks.TryGetValue(key, out var current) && current == disableFlags)
                return;

            Log($"{nameof(BlockInput)} - {key.ToString()} : {disableFlags.ToString()}");
            _inputBlocks[key] = disableFlags;
            
            UpdateState();
        }

        public void UnblockInput(object key)
        {
            if (key == null) 
                return;

            Log($"{nameof(UnblockInput)} - {key.ToString()}");
            _inputBlocks.Remove(key);
            
            UpdateState();
        }

        private EBlockInputState CalculateEffectiveInputState()
        {
            var state = EBlockInputState.UI | EBlockInputState.Game;

            foreach (var pair in _inputBlocks.Values)
            {
                state &= ~pair;
            }

            return state;
        }

        private void UpdateState()
        {
            InputState = CalculateEffectiveInputState();
            UpdateDebug();
        }

        private void RaiseOnInputStateChanged(EBlockInputState prevState)
        {
            Log($"{nameof(OnInputStateChanged)} - Current : {_inputState} || Prev : {prevState}");

            OnInputStateChanged?.Invoke(this, _inputState, prevState);
        }
    }
}
