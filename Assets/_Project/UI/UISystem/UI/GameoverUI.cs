using UnityEngine;
using UnityEngine.UIElements;
namespace PF.PJT.Duet.UISystem
{
    public class GameoverUI : BaseToolkitUI
    {
        public delegate void GameoverUIEventHandler(GameoverUI gameoverUI);

        [Header(" [ Gameover UI ] ")]
        [SerializeField] private string _gameoverUIName = "Gameover";

        [Header(" Class ")]
        [SerializeField] private string _appearName = "gameover__appear";
        [SerializeField] private string _appearEndElementName = "ButtonContainer";

        [Header(" Buttons ")]
        [SerializeField] private GameObject _restartButtonActor;
        [SerializeField] private GameObject _exitButtonActor;

        private VisualElement _gameover;
        private VisualElement _appearEndElement;


        private IUIToolkitSubmitEventListener _restartSubmit;
        private IUIToolkitCancelEventListener _restartCancel;

        private IUIToolkitSubmitEventListener _exitSubmit;

        public event GameoverUIEventHandler OnRestartPressed;
        public event GameoverUIEventHandler OnExitPressed;

        protected override void Awake()
        {
            base.Awake();

            _restartSubmit = _restartButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
            _restartCancel = _restartButtonActor.GetComponent<IUIToolkitCancelEventListener>();

            _exitSubmit = _exitButtonActor.GetComponent<IUIToolkitSubmitEventListener>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            RemoveAllBinding();

            OnRestartPressed = null;
            OnExitPressed = null;
        }
        private void RemoveAllBinding()
        {
            if (_restartSubmit is not null)
            {
                _restartSubmit.OnSubmited -= RestartSubmit_OnSubmited;
            }
            if (_restartCancel is not null)
            {
                _restartCancel.OnCanceled -= RestartCancel_OnCanceled;
            }

            if (_exitSubmit is not null)
            {
                _exitSubmit.OnSubmited -= ExitSubmit_OnSubmited;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _gameover = Root.Q<VisualElement>(_gameoverUIName);
            _appearEndElement = Root.Q<VisualElement>(_appearEndElementName);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            _gameover?.UnregisterCallback<TransitionEndEvent>(OnGameoverTransitionEnded);

            _gameover = null;
            _appearEndElement = null;
        }

        protected override void OnActivateEnd()
        {
            base.OnActivateEnd();

            _gameover.RegisterCallback<TransitionEndEvent>(OnGameoverTransitionEnded);
            
            _gameover.AddToClassList(_appearName);
        }

        protected override void OnDeactivateStart()
        {
            base.OnDeactivateStart();

            RemoveAllBinding();
            _gameover?.UnregisterCallback<TransitionEndEvent>(OnGameoverTransitionEnded);
        }

        protected override void OnDeactivateEnd()
        {
            base.OnDeactivateEnd();

            _gameover.RemoveFromClassList(_appearName);
        }

        public void PressRestart()
        {
            Log(nameof(PressRestart));

            Invoke_OnRestartPressed();
        }
        public void PressExit()
        {
            Log(nameof(PressExit));

            Invoke_OnExitPressed();
        }

        private void EndAppearTransition()
        {
            _restartSubmit.OnSubmited += RestartSubmit_OnSubmited;
            _restartCancel.OnCanceled += RestartCancel_OnCanceled;

            _exitSubmit.OnSubmited += ExitSubmit_OnSubmited;

            _restartSubmit.Target.Focus();
        }
        private void OnGameoverTransitionEnded(TransitionEndEvent evt)
        {
            if (evt.target is VisualElement element && element == _appearEndElement)
            {
                _gameover?.UnregisterCallback<TransitionEndEvent>(OnGameoverTransitionEnded);

                EndAppearTransition();
            }
        }


        private void RestartSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            PressRestart();
        }
        private void RestartCancel_OnCanceled(IUIToolkitEventListener uitookiEventListener)
        {
            _exitSubmit.Target.Focus();
        }
        private void ExitSubmit_OnSubmited(IUIToolkitEventListener uitookiEventListener)
        {
            PressExit();
        }

        private void Invoke_OnRestartPressed()
        {
            Log(nameof(OnRestartPressed));

            OnRestartPressed?.Invoke(this);
        }
        private void Invoke_OnExitPressed()
        {
            Log(nameof(OnExitPressed));

            OnExitPressed?.Invoke(this);
        }
    }
}
