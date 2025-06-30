using UnityEngine;

namespace PF.PJT.Duet.UISystem
{

    public class MainmenuUIFlowController : BaseUIFlowController
    {
        public delegate void MainmenuControllerEventHandler(MainmenuUIFlowController mainmenuController);

        [Header(" [ Mainmenu UI Flow Controller ] ")]
        [SerializeField] private MenuController _menuController;

        public event MainmenuControllerEventHandler OnGameStart;
        public event MainmenuControllerEventHandler OnGameExit;


        private void Awake()
        {
            _menuController.OnGameStart += MenuUIController_OnGameStart;
            _menuController.OnGameExit += MenuUIController_OnGameExit;
        }

        protected override void OnDestory()
        {
            base.OnDestory();

            if (_menuController)
            {
                _menuController.OnGameStart -= MenuUIController_OnGameStart;
                _menuController.OnGameExit -= MenuUIController_OnGameExit;
            }

            OnGameStart = null;
            OnGameExit = null;
        }

        public void StartGame()
        {
            Log(nameof(StartGame));

            Invoke_OnGameStart();
        }

        public void ExitGame()
        {
            Log(nameof(ExitGame));

            Invoke_OnGameExit();
        }


        private void MenuUIController_OnGameStart(MenuController menuController)
        {
            StartGame();
        }
        private void MenuUIController_OnGameExit(MenuController menuController)
        {
            ExitGame();
        }

        // Event Invoker
        private void Invoke_OnGameStart()
        {
            Log(nameof(OnGameStart));

            OnGameStart?.Invoke(this);
        }
        private void Invoke_OnGameExit()
        {
            Log(nameof(OnGameExit));

            OnGameExit?.Invoke(this);
        }
    }
}
