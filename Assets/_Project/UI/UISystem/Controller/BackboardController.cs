using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class BackboardController : BaseUIController
    {
        [Header(" [ Backboard Controller ] ")]
        [SerializeField] private BackboardUI _backboardUI;

        private void Awake()
        {
            _backboardUI.OnActivateStarted += BackboardUI_OnActivateStarted;
            _backboardUI.OnActivateEnded += BackboardUI_OnActivateEnded;
            _backboardUI.OnDeactivateStarted += BackboardUI_OnDeactivateStarted;
            _backboardUI.OnDeactivateEnded += BackboardUI_OnDeactivateEnded;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_backboardUI)
            {
                _backboardUI.OnActivateStarted -= BackboardUI_OnActivateStarted;
                _backboardUI.OnActivateEnded -= BackboardUI_OnActivateEnded;
                _backboardUI.OnDeactivateStarted -= BackboardUI_OnDeactivateStarted;
                _backboardUI.OnDeactivateEnded -= BackboardUI_OnDeactivateEnded;
            }
        }

        protected override void OnActivate()
        {
            _backboardUI.Activate();
        }
        protected override void OnDeactivate()
        {
            _backboardUI.Deactivate();
        }
        public override void SetSortingOrder(float newOrder)
        {
            if (_backboardUI.SortingOrder.SafeEquals(newOrder))
                return;

            _backboardUI.SetSortingOrder(newOrder);
        }
        public override void ResetSortingOrder()
        {
            _backboardUI.ResetSortingOrder();
        }


        private void BackboardUI_OnActivateStarted(object sender, EventArgs e)
        {
            RaiseOnActivateStarted();
        }
        private void BackboardUI_OnActivateEnded(object sender, EventArgs e)
        {
            RaiseOnActivateEnded();
        }
        private void BackboardUI_OnDeactivateStarted(object sender, EventArgs e)
        {
            RaiseOnDeactivateStarted();
        }
        private void BackboardUI_OnDeactivateEnded(object sender, EventArgs e)
        {
            RaiseOnDeactivateEnded();
        }
    }
}
