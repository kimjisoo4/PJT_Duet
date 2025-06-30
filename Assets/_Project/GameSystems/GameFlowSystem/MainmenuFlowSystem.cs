using PF.PJT.Duet.UISystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [AddComponentMenu("Duet/GameFlow/Mainmenu Flow System Controller")]
    public class MainmenuFlowSystem : BaseGameFlowSystem
    {
        [Header(" [ Mainmenu Flow System Controller ] ")]
        [SerializeField] private MainmenuUIFlowController _mainmenuController;

        [Header(" States ")]
        [SerializeField] private GameFlowState _waitForSceneLoadedState;
        [SerializeField] private GameFlowState _fadeInState;
        [SerializeField] private GameFlowState _mainmenuState;
        [SerializeField] private GameFlowState _loadStageState;

        private void Awake()
        {
            _mainmenuController.OnGameStart += MainmenuController_OnGameStart;
            _mainmenuController.OnGameExit += MainmenuController_OnGameExit;

            _waitForSceneLoadedState.OnStateComplated += WaitForSceneLoadedState_OnStateComplated;
            _fadeInState.OnStateComplated += FadeInState_OnStateComplated;
        }



        private void OnDestroy()
        {
            if (_mainmenuController)
            {
                _mainmenuController.OnGameStart -= MainmenuController_OnGameStart;
                _mainmenuController.OnGameExit -= MainmenuController_OnGameExit;
            }

            if(_fadeInState)
            {
                _fadeInState.OnStateComplated -= FadeInState_OnStateComplated;
            }

            if(_waitForSceneLoadedState)
            {
                _waitForSceneLoadedState.OnStateComplated -= WaitForSceneLoadedState_OnStateComplated;
            }
        }

        private void Start()
        {
            StateMachine.Start();

            if(!StateMachine.TrySetState(_fadeInState))
            {
                StateMachine.TrySetState(_mainmenuState);
            }
        }

        public void StartGame()
        {
            Log(nameof(StartGame));

            StateMachine.TrySetState(_loadStageState);
        }
        public void ExitGame()
        {
            Log(nameof(ExitGame));

#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
        public void OnFadeStateComplated()
        {
            Log(nameof(OnFadeStateComplated));

            StateMachine.TrySetState(_mainmenuState);
        }

        public void OnSceneLoadedComplated()
        {
            if (!StateMachine.TrySetState(_fadeInState))
            {
                StateMachine.TrySetState(_mainmenuState);
            }
        }

        private void FadeInState_OnStateComplated(GameFlowState gameFlowState)
        {
            OnFadeStateComplated();
        }
        private void WaitForSceneLoadedState_OnStateComplated(GameFlowState gameFlowState)
        {
            OnSceneLoadedComplated();
        }

        private void MainmenuController_OnGameStart(MainmenuUIFlowController mainmenuUIController)
        {
            StartGame();
        }
        private void MainmenuController_OnGameExit(MainmenuUIFlowController mainmenuUIController)
        {
            ExitGame();
        }
    }
}
