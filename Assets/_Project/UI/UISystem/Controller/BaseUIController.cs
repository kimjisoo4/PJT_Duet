using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.UISystem
{

    public abstract class BaseUIController : BaseMonoBehaviour
    {
        public class OnFocusUIChangedEventArgs : EventArgs
        {
            public readonly BaseToolkitUI FocusUI;
            public readonly BaseToolkitUI PrevFocusUI;

            public OnFocusUIChangedEventArgs(BaseToolkitUI newForcusUI, BaseToolkitUI prevFocusUI)
            {
                FocusUI = newForcusUI;
                PrevFocusUI = prevFocusUI;
            }
        }

        [Header(" [ Base UI Controller ] ")]
        
        [Header(" Input ")]
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private EBlockInputState _blockInput = EBlockInputState.Game;

        private BaseToolkitUI _focueUI;
        private bool _isActive;
        public bool IsActive => _isActive;
        public BaseToolkitUI FocusUI
        {
            get
            {
                return _focueUI;
            }
            set
            {
                if (_focueUI == value)
                    return;

                var prev = _focueUI;
                _focueUI = value;

                RaiseOnFocusUIChanged(_focueUI, prev);
            }
        }

        public event EventHandler OnActivateStarted;
        public event EventHandler OnDeactivateStarted;
        public event EventHandler OnActivateEnded;
        public event EventHandler OnDeactivateEnded;
        public event EventHandler<OnFocusUIChangedEventArgs> OnFocusUIChanged;

        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            if(!_inputBlocker)
            {
                _inputBlocker = SUtility.FindAssetByType<InputBlocker>();
            }
#endif
        }
        protected virtual void OnDestroy()
        {
            _inputBlocker.UnblockInput(this);
        }

        public void Activate()
        {
            if (_isActive)
                return;

            _isActive = true;
            _inputBlocker.BlockInput(this, _blockInput);

            OnActivate();
        }
        public void Deactivate()
        {
            if (!_isActive)
                return;

            _isActive = false;
            _inputBlocker.UnblockInput(this);

            OnDeactivate();
        }
        protected abstract void OnActivate();
        protected abstract void OnDeactivate();
        public abstract void SetSortingOrder(float newOrder);
        public abstract void ResetSortingOrder();

        protected void RaiseOnActivateStarted()
        {
            Log(nameof(OnActivateStarted));

            OnActivateStarted?.Invoke(this, EventArgs.Empty);
        }
        protected void RaiseOnDeactivateStarted()
        {
            Log(nameof(OnDeactivateStarted));

            OnDeactivateStarted?.Invoke(this, EventArgs.Empty);
        }
        protected void RaiseOnActivateEnded()
        {
            Log(nameof(OnActivateEnded));

            OnActivateEnded?.Invoke(this, EventArgs.Empty);
        }
        protected void RaiseOnDeactivateEnded()
        {
            Log(nameof(OnDeactivateEnded));

            OnDeactivateEnded?.Invoke(this, EventArgs.Empty);
        }
        protected void RaiseOnFocusUIChanged(BaseToolkitUI lastUI, BaseToolkitUI prevLastUI)
        {
            Log(nameof(OnFocusUIChanged));

            OnFocusUIChanged?.Invoke(this, new OnFocusUIChangedEventArgs(lastUI, prevLastUI));
        }
    }
}
