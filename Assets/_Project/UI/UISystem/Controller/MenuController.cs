using StudioScor.Utilities;
using System;
using UnityEngine;

namespace PF.PJT.Duet.UISystem
{
    public class MenuController : BaseUIController
    {
        public delegate void MenuControllerEventHandler(MenuController menuController);

        [Header(" [ Menu Controller ] ")]
        [SerializeField] private MenuUI _menuUI;
        [SerializeField] private CheckExitGameUI _checkExitGameUI;

        public MenuUI MenuUI => _menuUI;
        public CheckExitGameUI CheckExitGameUI => _checkExitGameUI;

        public event MenuControllerEventHandler OnGameStart;
        public event MenuControllerEventHandler OnSettingOpen;
        public event MenuControllerEventHandler OnGameExit;

        private void Awake()
        {
            _menuUI.OnActivateStarted += MenuUI_OnActivateStarted;
            _menuUI.OnActivateEnded += MenuUI_OnActivateEnded;
            _menuUI.OnDeactivateStarted += MenuUI_OnDeactivateStarted;
            _menuUI.OnDeactivateEnded += MenuUI_OnDeactivateEnded;

            _menuUI.OnGameStartButtonPressed += MenuUI_OnGameStartButtonPressed;
            _menuUI.OnSettingButtonPressed += MenuUI_OnSettingButtonPressed;
            _menuUI.OnExitButtonPressed += MenuUI_OnExitButtonPressed;


            _checkExitGameUI.OnActivateEnded += CheckExitGameUI_OnActivateEnded;
            _checkExitGameUI.OnDeactivateEnded += CheckExitGameUI_OnDeactivateEnded;
            _checkExitGameUI.OnAccepted += CheckExitGameUI_OnAccepted;
            _checkExitGameUI.OnRejected += CheckExitGameUI_OnRejected;
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_menuUI)
            {
                _menuUI.OnActivateStarted -= MenuUI_OnActivateStarted;
                _menuUI.OnActivateEnded -= MenuUI_OnActivateEnded;
                _menuUI.OnDeactivateStarted -= MenuUI_OnDeactivateStarted;
                _menuUI.OnDeactivateEnded -= MenuUI_OnDeactivateEnded;

                _menuUI.OnGameStartButtonPressed -= MenuUI_OnGameStartButtonPressed;
                _menuUI.OnSettingButtonPressed -= MenuUI_OnSettingButtonPressed;
                _menuUI.OnExitButtonPressed -= MenuUI_OnExitButtonPressed;
            }

            if(_checkExitGameUI)
            {
                _checkExitGameUI.OnActivateEnded -= CheckExitGameUI_OnActivateEnded;
                _checkExitGameUI.OnDeactivateEnded -= CheckExitGameUI_OnDeactivateEnded;

                _checkExitGameUI.OnAccepted -= CheckExitGameUI_OnAccepted;
                _checkExitGameUI.OnRejected -= CheckExitGameUI_OnRejected;
            }

            OnGameStart = null;
            OnSettingOpen = null;
            OnGameExit = null;
        }



        protected override void OnActivate()
        {
            _menuUI.Activate();
            FocusUI = _menuUI;
        }

        protected override void OnDeactivate()
        {
            _menuUI.Deactivate();
            _checkExitGameUI.Deactivate();
        }

        public override void SetSortingOrder(float newOrder)
        {
            if (_menuUI.SortingOrder.SafeEquals(newOrder))
                return;

            float gap = _checkExitGameUI.SortingOrder - _menuUI.SortingOrder;

            _menuUI.SetSortingOrder(newOrder);
            _checkExitGameUI.SetSortingOrder(newOrder + gap);
        }

        public override void ResetSortingOrder()
        {
            _menuUI.ResetSortingOrder();
            _checkExitGameUI.ResetSortingOrder();
        }

        public void SetInteraction(bool enabled)
        {
            _menuUI.SetInteraction(enabled);
            _checkExitGameUI.SetInteraction(enabled);
        }

        public void StartGame()
        {
            Log(nameof(StartGame));

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


        public void OpenCheckExitGameUI()
        {
            Log(nameof(OpenCheckExitGameUI));

            _menuUI.SetInteraction(false);
            _checkExitGameUI.Activate();

            FocusUI = null;
        }
        public void CloseCheckExitGameUI()
        {
            Log(nameof(CloseCheckExitGameUI));

            _checkExitGameUI.Deactivate();

            _menuUI.SetInteraction(true);
            _menuUI.SettingElement.Focus();

            FocusUI = null;
        }
        public void AcceptExitGame()
        {
            Log(nameof(AcceptExitGame));

            ExitGame();
        }
        public void RejectExitGame()
        {
            Log(nameof(RejectExitGame));

            CloseCheckExitGameUI();
        }



        private void MenuUI_OnActivateStarted(object sender, EventArgs e)
        {
            RaiseOnActivateStarted();
        }
        private void MenuUI_OnActivateEnded(object sender, EventArgs e)
        {
            RaiseOnActivateEnded();
        }

        private void MenuUI_OnDeactivateStarted(object sender, EventArgs e)
        {
            RaiseOnDeactivateStarted();
        }
        private void MenuUI_OnDeactivateEnded(object sender, EventArgs e)
        {
            RaiseOnDeactivateEnded();
        }
        private void MenuUI_OnGameStartButtonPressed(MenuUI menuUIController)
        {
            StartGame();
        }
        private void MenuUI_OnSettingButtonPressed(MenuUI menuUIController)
        {
            OpenSetting();
        }
        private void MenuUI_OnExitButtonPressed(MenuUI menuUIController)
        {
            OpenCheckExitGameUI();
        }

        private void CheckExitGameUI_OnActivateEnded(object sender, EventArgs e)
        {
            FocusUI = _checkExitGameUI;
        }
        private void CheckExitGameUI_OnDeactivateEnded(object sender, EventArgs e)
        {
            FocusUI = _menuUI;
        }

        private void CheckExitGameUI_OnRejected(CheckExitGameUI checkExitGameUI)
        {
            RejectExitGame();
        }

        private void CheckExitGameUI_OnAccepted(CheckExitGameUI checkExitGameUI)
        {
            AcceptExitGame();
        }


        private void RaiseOnGameStart()
        {
            Log(nameof(OnGameStart));

            OnGameStart?.Invoke(this);
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
