using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class IngameMenuController : BaseUIController
    {
        public delegate void IngameMenuControllerEventHandler(IngameMenuController ingameMenuController);

        [Header(" [ Menu Controller ] ")]
        [SerializeField] private IngameMenuUI _ingameMenuUI;
        [SerializeField] private CheckToTitleUI _checkToTitleUI;


        public event IngameMenuControllerEventHandler OnContinue;
        public event IngameMenuControllerEventHandler OnSettingOpen;
        public event IngameMenuControllerEventHandler OnGameExit;

        private void Awake()
        {
            _ingameMenuUI.OnActivateStarted += IngameMenuUI_OnActivateStarted;
            _ingameMenuUI.OnActivateEnded += IngameMenuUI_OnActivateEnded;
            _ingameMenuUI.OnDeactivateStarted += IngameMenuUI_OnDeactivateStarted;
            _ingameMenuUI.OnDeactivateEnded += IngameMenuUI_OnDeactivateEnded;

            _ingameMenuUI.OnContinueButtonPress += IngameMenuUI_OnGameStartButtonPressed;
            _ingameMenuUI.OnSettingButtonPressed += IngameMenuUI_OnSettingButtonPressed;
            _ingameMenuUI.OnExitButtonPressed += IngameMenuUI_OnExitButtonPressed;
            
            _checkToTitleUI.OnAccepted += CheckToTltieUI_OnAccepted;
            _checkToTitleUI.OnRejected += CheckToTltieUI_OnRejected;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_ingameMenuUI)
            {
                _ingameMenuUI.OnActivateStarted -= IngameMenuUI_OnActivateStarted;
                _ingameMenuUI.OnActivateEnded -= IngameMenuUI_OnActivateEnded;
                _ingameMenuUI.OnDeactivateStarted -= IngameMenuUI_OnDeactivateStarted;
                _ingameMenuUI.OnDeactivateEnded -= IngameMenuUI_OnDeactivateEnded;

                _ingameMenuUI.OnContinueButtonPress -= IngameMenuUI_OnGameStartButtonPressed;
                _ingameMenuUI.OnSettingButtonPressed -= IngameMenuUI_OnSettingButtonPressed;
                _ingameMenuUI.OnExitButtonPressed -= IngameMenuUI_OnExitButtonPressed;
            }

            if (_checkToTitleUI)
            {
                _checkToTitleUI.OnAccepted -= CheckToTltieUI_OnAccepted;
                _checkToTitleUI.OnRejected -= CheckToTltieUI_OnRejected;
            }

            OnContinue = null;
            OnSettingOpen = null;
            OnGameExit = null;
        }

        protected override void OnActivate()
        {
            _ingameMenuUI.Activate();
            FocusUI = _ingameMenuUI;
        }
        protected override void OnDeactivate()
        {
            _ingameMenuUI.Deactivate();
            _checkToTitleUI.Deactivate();
        }
        public override void SetSortingOrder(float newOrder)
        {
            if (_ingameMenuUI.SortingOrder.SafeEquals(newOrder))
                return;

            float gap = _checkToTitleUI.SortingOrder - _ingameMenuUI.SortingOrder;

            _ingameMenuUI.SetSortingOrder(newOrder);
            _checkToTitleUI.SetSortingOrder(newOrder + gap);
        }

        public override void ResetSortingOrder()
        {
            _ingameMenuUI.ResetSortingOrder();
            _checkToTitleUI.ResetSortingOrder();
        }
        public void SetInteraction(bool enabled)
        {
            _ingameMenuUI.SetInteraction(enabled);
            _checkToTitleUI.SetInteraction(enabled);
        }
        public void Contunue()
        {
            Log(nameof(Contunue));

            RaiseOnGameStart();
        }
        public void OpenSetting()
        {
            Log(nameof(OpenSetting));

            RaiseOnSettingOpen();
        }
        public void ExitGame()
        {
            Log(nameof(ExitGame));

            RaiseOnGameExit();
        }


        public void OpenCheckToTitleUI()
        {
            Log(nameof(OpenCheckToTitleUI));

            _ingameMenuUI.SetInteraction(false);
            _checkToTitleUI.Activate();

            FocusUI = _checkToTitleUI;
        }
        public void CloseCheckToTitleUI()
        {
            Log(nameof(CloseCheckToTitleUI));

            _checkToTitleUI.Deactivate();

            _ingameMenuUI.SetInteraction(true);
            _ingameMenuUI.SettingElement.Focus();

            FocusUI = _ingameMenuUI;
        }
        public void AcceptExitGame()
        {
            Log(nameof(AcceptExitGame));

            ExitGame();
        }
        public void RejectExitGame()
        {
            Log(nameof(RejectExitGame));

            CloseCheckToTitleUI();
        }


        private void IngameMenuUI_OnDeactivateEnded(object sender, EventArgs e)
        {
            RaiseOnDeactivateEnded();
        }

        private void IngameMenuUI_OnDeactivateStarted(object sender, EventArgs e)
        {
            RaiseOnDeactivateStarted();
        }

        private void IngameMenuUI_OnActivateEnded(object sender, EventArgs e)
        {
            RaiseOnActivateEnded();
        }
        private void IngameMenuUI_OnActivateStarted(object sender, EventArgs e)
        {
            RaiseOnActivateStarted();
        }

        private void IngameMenuUI_OnExitButtonPressed(IngameMenuUI ingameMenuUI)
        {
            OpenCheckToTitleUI();
        }
        private void IngameMenuUI_OnSettingButtonPressed(IngameMenuUI ingameMenuUI)
        {
            OpenSetting();
        }
        private void IngameMenuUI_OnGameStartButtonPressed(IngameMenuUI ingameMenuUI)
        {
            Contunue();
        }

        private void CheckToTltieUI_OnRejected(CheckToTitleUI checkToTitleUI)
        {
            RejectExitGame();
        }

        private void CheckToTltieUI_OnAccepted(CheckToTitleUI checkToTitleUI)
        {
            AcceptExitGame();
        }


        private void RaiseOnGameStart()
        {
            Log(nameof(OnContinue));

            OnContinue?.Invoke(this);
        }
        private void RaiseOnSettingOpen()
        {
            Log(nameof(OnSettingOpen));

            OnSettingOpen?.Invoke(this);
        }
        private void RaiseOnGameExit()
        {
            Log(nameof(OnGameExit));

            OnGameExit?.Invoke(this);
        }
    }
}
