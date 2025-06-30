using StudioScor.Utilities;
using System;
using UnityEngine;
using UnityEngine.Profiling;

namespace PF.PJT.Duet.UISystem
{
    public class ResultController : BaseUIController
    {
        public enum EResult
        {
            None,
            Gameover,
            StageClear,
        }

        public delegate void ResultControllerEventHandler(ResultController resultController);
        public delegate void ResultStateChangeEventHandler(ResultController resultController, EResult currentState, EResult prevState);

        [Header(" [ Result Controller ]")]
        [SerializeField] private StageClearUI _stageClearUI;
        [SerializeField] private GameoverUI _gameoverUI;

        private EResult _resultState;
        public EResult ResultState => _resultState;

        public event ResultControllerEventHandler OnRestart;
        public event ResultControllerEventHandler OnExit;
        public event ResultStateChangeEventHandler OnResultStateChanged;

        private void Awake()
        {
            _stageClearUI.OnActivateStarted += StageClearUI_OnActivateStarted;
            _stageClearUI.OnActivateEnded += StageClearUI_OnActivateEnded;
            _stageClearUI.OnDeactivateStarted += StageClearUI_OnDeactivateStarted;
            _stageClearUI.OnDeactivateEnded += StageClearUI_OnDeactivateEnded;
            _stageClearUI.OnRestartPressed += StageClearUI_OnRestartPressed;
            _stageClearUI.OnExitPressed += StageClearUI_OnExitPressed;

            _gameoverUI.OnActivateStarted += GameoverUI_OnActivateStarted;
            _gameoverUI.OnActivateEnded += GameoverUI_OnActivateEnded;
            _gameoverUI.OnDeactivateStarted += GameoverUI_OnDeactivateStarted;
            _gameoverUI.OnDeactivateEnded += GameoverUI_OnDeactivateEnded;
            _gameoverUI.OnRestartPressed += GameoverUI_OnRestartPressed;
            _gameoverUI.OnExitPressed += GameoverUI_OnExitPressed;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_stageClearUI)
            {
                _stageClearUI.OnActivateStarted -= StageClearUI_OnActivateStarted;
                _stageClearUI.OnActivateEnded -= StageClearUI_OnActivateEnded;
                _stageClearUI.OnDeactivateStarted -= StageClearUI_OnDeactivateStarted;
                _stageClearUI.OnDeactivateEnded -= StageClearUI_OnDeactivateEnded;
                _stageClearUI.OnRestartPressed -= StageClearUI_OnRestartPressed;
                _stageClearUI.OnExitPressed -= StageClearUI_OnExitPressed;
            }

            if (_gameoverUI)
            {
                _gameoverUI.OnActivateStarted -= GameoverUI_OnActivateStarted;
                _gameoverUI.OnActivateEnded -= GameoverUI_OnActivateEnded;
                _gameoverUI.OnDeactivateStarted -= GameoverUI_OnDeactivateStarted;
                _gameoverUI.OnDeactivateEnded -= GameoverUI_OnDeactivateEnded;
                _gameoverUI.OnRestartPressed -= GameoverUI_OnRestartPressed;
                _gameoverUI.OnExitPressed -= GameoverUI_OnExitPressed;
            }

            OnRestart = null;
            OnExit = null;
            OnResultStateChanged = null;
        }

        protected override void OnActivate()
        {
            Log(nameof(OnActivate));

            switch (_resultState)
            {
                case EResult.None:
                    break;
                case EResult.Gameover:
                    OpenGameoverUI();
                    break;
                case EResult.StageClear:
                    OpenStageClearUI();
                    break;
                default:
                    break;
            }
        }
        protected override void OnDeactivate()
        {
            Log(nameof(OnDeactivate));

            switch (_resultState)
            {
                case EResult.None:
                    break;
                case EResult.Gameover:
                    CloseGameoverUI();
                    break;
                case EResult.StageClear:
                    CloseStageClearUI();
                    break;
                default:
                    break;
            }
        }
        public override void SetSortingOrder(float newOrder)
        {
            if (_stageClearUI.SortingOrder.SafeEquals(newOrder) && _gameoverUI.SortingOrder.SafeEquals(newOrder))
                return;

            _stageClearUI.SetSortingOrder(newOrder);
            _gameoverUI.SetSortingOrder(newOrder);
        }

        public override void ResetSortingOrder()
        {
            _stageClearUI.ResetSortingOrder();
            _gameoverUI.ResetSortingOrder();
        }


        public void ChangeResult(EResult result)
        {
            if (_resultState == result)
                return;
            
            Log($"{nameof(ChangeResult)} New Result : {result}");

            var prevState = _resultState;
            _resultState = result;

            RaiseOnResultStateChanged(prevState);
        }

        public void OpenStageClearUI()
        {
            if (_resultState == EResult.Gameover)
                CloseGameoverUI();

            Log(nameof(OpenStageClearUI));

            _resultState = EResult.StageClear;

            _stageClearUI.Activate();
            FocusUI = _stageClearUI;
        }

        public void CloseStageClearUI()
        {
            Log(nameof(CloseStageClearUI));

            _resultState = EResult.None;

            _stageClearUI.Deactivate();
        }

        public void OpenGameoverUI()
        {
            if (_resultState == EResult.StageClear)
                CloseStageClearUI();

            Log(nameof(OpenGameoverUI));

            _resultState = EResult.Gameover;

            _gameoverUI.Activate();
            FocusUI = _gameoverUI;
        }
        public void CloseGameoverUI()
        {
            Log(nameof(CloseGameoverUI));

            _resultState = EResult.None;

            _gameoverUI.Deactivate();
        }

        public void ExitGame()
        {
            Log(nameof(ExitGame));

            RaiseOnExit();
        }
        public void RestartGame()
        {
            Log(nameof(RestartGame));

            RaiseOnRestart();
        }
        
        private void GameoverUI_OnActivateStarted(object sender, EventArgs e)
        {
            RaiseOnActivateStarted();
        }
        private void GameoverUI_OnActivateEnded(object sender, EventArgs e)
        {
            RaiseOnActivateEnded();
        }
        private void GameoverUI_OnDeactivateStarted(object sender, EventArgs e)
        {
            RaiseOnDeactivateStarted();
        }
        private void GameoverUI_OnDeactivateEnded(object sender, EventArgs e)
        {
            RaiseOnActivateEnded();
        }
        private void GameoverUI_OnRestartPressed(GameoverUI gameoverUI)
        {
            RestartGame();
        }
        private void GameoverUI_OnExitPressed(GameoverUI gameoverUI)
        {
            ExitGame();
        }


        private void StageClearUI_OnActivateStarted(object sender, EventArgs e)
        {
            RaiseOnActivateStarted();
        }
        private void StageClearUI_OnActivateEnded(object sender, EventArgs e)
        {
            RaiseOnActivateEnded();
        }
        private void StageClearUI_OnDeactivateStarted(object sender, EventArgs e)
        {
            RaiseOnDeactivateStarted();
        }
        private void StageClearUI_OnDeactivateEnded(object sender, EventArgs e)
        {
            RaiseOnActivateEnded();
        }

        private void StageClearUI_OnRestartPressed(StageClearUI stageClearUI)
        {
            RestartGame();
        }
        private void StageClearUI_OnExitPressed(StageClearUI stageClearUI)
        {
            ExitGame();
        }


        private void RaiseOnRestart()
        {
            Log(nameof(OnRestart));

            OnRestart?.Invoke(this);
        }
        private void RaiseOnExit()
        {
            Log(nameof(OnExit));

            OnExit?.Invoke(this);
        }
        private void RaiseOnResultStateChanged(EResult prevResult)
        {
            Log($"{nameof(OnResultStateChanged)} CurrentState : {_resultState} || PrevState : {prevResult}" );

            OnResultStateChanged?.Invoke(this, _resultState, prevResult);
        }
    }
}
