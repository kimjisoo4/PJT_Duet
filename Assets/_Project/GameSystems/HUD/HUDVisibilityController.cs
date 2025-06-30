using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/Utility/new HUDVisibilityController", fileName = "HUDVisibilityController")]
    public class HUDVisibilityController : BaseScriptableObject
    {
        public delegate void VisibilityChangedHandler(bool isVisible);
        
        private readonly HashSet<object> _hiddenRequests = new();
        private bool _isVisible = true;
        public bool IsVisible => _isVisible;

        public event VisibilityChangedHandler OnVisibilityChanged;
        
        protected override void OnReset()
        {
            base.OnReset();

            _hiddenRequests.Clear();
            _isVisible = true;
            OnVisibilityChanged = null;
        }

        public void RequestHide(object key)
        {
            if (key == null)
                return;

            if (_hiddenRequests.Add(key))
            {
                UpdateVisibility();
            }
        }

        public void RequestShow(object key)
        {
            if (key == null)
                return;

            if (_hiddenRequests.Remove(key))
            {
                UpdateVisibility();
            }
        }

        private void UpdateVisibility()
        {
            bool newVisibility = _hiddenRequests.Count == 0;

            if (_isVisible != newVisibility)
            {
                _isVisible = newVisibility;

                RaiseVisibilityChanged();
            }
        }

        private void RaiseVisibilityChanged()
        {
            Log($"{nameof(OnVisibilityChanged)} - Current: {_isVisible}");

            OnVisibilityChanged?.Invoke(_isVisible);
        }
    }
}
