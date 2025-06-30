using StudioScor.Utilities;
using System;
using UnityEngine;
namespace PF.PJT.Duet.UISystem
{
    public class TitleController : BaseUIController
    {
        public delegate void TitleControllerEventHandler(TitleController titleController);

        [Header(" [ Title Controller ] ")]
        [SerializeField] private TitleUI _titleUI;
        [SerializeField] private PressAnyKeyUI _pressAnyKeyUI;

        public TitleUI TitleUI => _titleUI;
        public PressAnyKeyUI PressAnyKeyUI => _pressAnyKeyUI;

        public event TitleControllerEventHandler OnAnyKeyPress;

        private void Awake()
        {
            _titleUI.OnActivateStarted += TitleUI_OnActivateStarted;
            _titleUI.OnActivateEnded += TitleUI_OnActivateEnded;
            _titleUI.OnDeactivateStarted += TitleUI_OnDeactivateStarted;
            _titleUI.OnDeactivateEnded += TitleUI_OnDeactivateEnded;
            _titleUI.OnTranslateFinished += TitleUI_OnTranslateFinished;
            _pressAnyKeyUI.OnAnyKeyPressed += PressAnyKeyUI_OnAnyKeyPressed;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            Debug.Log($"<color=red>I'm Destroyed!!!!!!!!!!!!!!!!!!!!!!!!!</color>");

            if (_titleUI)
            {
                _titleUI.OnActivateStarted -= TitleUI_OnActivateStarted;
                _titleUI.OnActivateEnded -= TitleUI_OnActivateEnded;
                _titleUI.OnDeactivateStarted -= TitleUI_OnDeactivateStarted;
                _titleUI.OnDeactivateEnded -= TitleUI_OnDeactivateEnded;
                _titleUI.OnTranslateFinished -= TitleUI_OnTranslateFinished;
            }

            if(_pressAnyKeyUI)
            {
                _pressAnyKeyUI.OnAnyKeyPressed -= PressAnyKeyUI_OnAnyKeyPressed;
            }

            OnAnyKeyPress = null;
        }

        protected override void OnActivate()
        {
            _titleUI.Activate();
            FocusUI = _titleUI;
        }
        protected override void OnDeactivate()
        {
            _titleUI.Deactivate();
            _pressAnyKeyUI.Deactivate();
        }

        public override void SetSortingOrder(float newOrder)
        {
            if (_titleUI.SortingOrder.SafeEquals(newOrder))
                return;

            var gap = _pressAnyKeyUI.SortingOrder - _titleUI.SortingOrder;

            _titleUI.SetSortingOrder(newOrder);
            _pressAnyKeyUI.SetSortingOrder(newOrder + gap);
        }

        public override void ResetSortingOrder()
        {
            _titleUI.ResetSortingOrder();
            _pressAnyKeyUI.ResetSortingOrder();
        }
        public void SetInteraction(bool enabled)
        {
            _titleUI.SetInteraction(enabled);
            _pressAnyKeyUI.SetInteraction(enabled);
        }
        public void OpenPressAnyKeyUI()
        {
            Log(nameof(OpenPressAnyKeyUI));

            _pressAnyKeyUI.Activate();
            FocusUI = _pressAnyKeyUI;
        }
        public void ClosePressAnyKeyUI()
        {
            Log(nameof(ClosePressAnyKeyUI));

            _pressAnyKeyUI.Deactivate();
            FocusUI = _titleUI;
        }
        public void PressAnyKey()
        {
            Log(nameof(PressAnyKey));

            Invoke_OnAnyKeyPress();
        }


        private void TitleUI_OnActivateStarted(object sender, EventArgs e)
        {
            RaiseOnActivateStarted();
        }

        private void TitleUI_OnActivateEnded(object sender, EventArgs e)
        {
            RaiseOnActivateEnded();
        }

        private void TitleUI_OnDeactivateStarted(object sender, EventArgs e)
        {
            RaiseOnDeactivateStarted();
        }

        private void TitleUI_OnDeactivateEnded(object sender, EventArgs e)
        {
            RaiseOnDeactivateEnded();
        }

        private void TitleUI_OnTranslateFinished(TitleUI titleUIController)
        {
            OpenPressAnyKeyUI();
        }
        private void PressAnyKeyUI_OnAnyKeyPressed(PressAnyKeyUI pressAnyKeyUIController)
        {
            PressAnyKey();
        }


        private void Invoke_OnAnyKeyPress()
        {
            Log(nameof(OnAnyKeyPress));

            OnAnyKeyPress?.Invoke(this);
        }
    }
}
