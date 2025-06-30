using StudioScor.Utilities;
using StudioScor.Utilities.DialogueSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PF.PJT.Duet.DialogueSystem
{
    public class DialogueController : BaseMonoBehaviour
    {
        public delegate void DialogueControllerEventHandler(DialogueController dialogueController);

        [Header(" [ Dialogue Controller ] ")]
        [SerializeField] private GameObject _dialogueUIActor;
        [SerializeField] private GameObject _dialogueSystemActor;
        [SerializeField] private GameObject _dialogueDisplayActor;
        [SerializeField] private GameObject _eventListenerActor;
        [SerializeField] private DialogueSelectButton[] _dialogueSelectButtons;

        [Header(" State Machine  ")]
        [SerializeField] private DialogueControllerState.StateMachine _stateMachine;
        [SerializeField] private DialogueControllerState _inactiveState;
        [SerializeField] private DialogueControllerState _waitForShowDisplayState;
        [SerializeField] private DialogueControllerState _waitForWritingState;
        [SerializeField] private DialogueControllerState _waitForNextDialogueState;
        [SerializeField] private DialogueControllerState _waitForSelectButtonState;
        [SerializeField] private DialogueControllerState _waitForHideDisplayState;

        [Header(" Block Input ")]
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private EBlockInputState _blockInput = EBlockInputState.Game;

        [Header(" Variables ")]
        [SerializeField] private GameObjectListVariable _inActiveStatusVariable;

        [Header(" Events ")]
        [Header(" Post ")]
        [SerializeField] private GameEvent _onStartedDialogue;
        [SerializeField] private GameEvent _onFinishedDialogue;

        public event DialogueControllerEventHandler OnStartedActivate;
        public event DialogueControllerEventHandler OnFinishedActivate;
        public event DialogueControllerEventHandler OnStartedDeactivate;
        public event DialogueControllerEventHandler OnDeactivateFinished;

        private IDialogue _dialogue;
        private IDialogueSystem _dialogueSystem;
        private IDialogueDisplay _dialogueDisplay;
        private ISubmitEventListener _submitEventListener;

        public IDialogue Dialogue => _dialogue;
        public IDialogueSystem DialogueSystem => _dialogueSystem;
        public IDialogueDisplay DialogueDisplay => _dialogueDisplay;

        private bool _wasInit = false;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_inputBlocker)
            {
                _inputBlocker = SUtility.FindAssetByType<InputBlocker>();
            }
#endif
        }
        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
            if(_dialogueSystem is not null)
            {
                _dialogueSystem.OnStartedDialogue -= _dialogueSystem_OnStartedDialogue;
                _dialogueSystem.OnFinishedDialogue -= _dialogueSystem_OnFinishedDialogue;
            }

            if(_dialogueDisplay is not null)
            {
                _dialogueDisplay.OnFinishedBlendIn -= _dialogueDisplay_OnFinishedBlendIn;
                _dialogueDisplay.OnDeactivated -= _dialogueDisplay_OnDeactivated;
                _dialogueDisplay.OnStartedWriting -= _dialogueDisplay_OnStartedWriting;
                _dialogueDisplay.OnEndedWriting -= _dialogueDisplay_OnEndedWriting;
            }

            if (!_dialogueSelectButtons.IsNullOrEmpty())
            {
                foreach (var selectButton in _dialogueSelectButtons)
                {
                    if (selectButton)
                        selectButton.OnSubmited -= SelectButton_OnSubmited;
                }
            }

            if(_submitEventListener is not null)
            {
                _submitEventListener.OnSubmited -= _submitEventListener_OnSubmited;
            }

            _inputBlocker.UnblockInput(this);
        }

        public void Init()
        {
            if (_wasInit)
                return;

            _wasInit = true;

            _dialogueSystem = _dialogueSystemActor.GetComponent<IDialogueSystem>();
            _dialogueDisplay = _dialogueDisplayActor.GetComponent<IDialogueDisplay>();
            _submitEventListener = _eventListenerActor.GetComponent<ISubmitEventListener>();
            

            _dialogueSystem.OnStartedDialogue += _dialogueSystem_OnStartedDialogue;
            _dialogueSystem.OnFinishedDialogue += _dialogueSystem_OnFinishedDialogue;

            _dialogueDisplay.Init();
            _dialogueDisplay.Deactivate();

            _dialogueDisplay.OnFinishedBlendIn += _dialogueDisplay_OnFinishedBlendIn;
            _dialogueDisplay.OnDeactivated += _dialogueDisplay_OnDeactivated;
            _dialogueDisplay.OnStartedWriting += _dialogueDisplay_OnStartedWriting;
            _dialogueDisplay.OnEndedWriting += _dialogueDisplay_OnEndedWriting;

            _submitEventListener.OnSubmited += _submitEventListener_OnSubmited;

            foreach (var selectButton in _dialogueSelectButtons)
            {
                selectButton.OnSubmited += SelectButton_OnSubmited;
                selectButton.Init();
                selectButton.Deactivate();
            }


            _dialogueUIActor.SetActive(false);
            _stateMachine.Start();
        }

        public void Activate(IDialogue dialogue)
        {
            if(_stateMachine.CanSetState(_waitForShowDisplayState))
            {
                _dialogue = dialogue;

                _inActiveStatusVariable.Add(gameObject);
                _inputBlocker.BlockInput(this, _blockInput);

                _stateMachine.ForceSetState(_waitForShowDisplayState);

                Invoke_OnStartedActivate();
            }
        }

        public void Deactivate()
        {
            _dialogueDisplay.Deactivate();

            _stateMachine.TrySetState(_waitForHideDisplayState);

            Invoke_OnStartedDeactivate();
        }

        private void _dialogueDisplay_OnFinishedBlendIn(IDialogueDisplay dialogueDisplay)
        {
            Invoke_OnFinishedActivate();

            DialogueSystem.PlayDialogue(_dialogue);

        }
        private void _dialogueDisplay_OnDeactivated(IDialogueDisplay dialogueDisplay)
        {
            _inActiveStatusVariable.Remove(gameObject);
            _inputBlocker.UnblockInput(this);

            _stateMachine.TrySetState(_inactiveState);

            Invoke_OnFinishedDeactivate();
        }

        private void _dialogueDisplay_OnStartedWriting(IDialogueDisplay dialogueDisplay)
        {
            _stateMachine.TrySetState(_waitForWritingState);
        }
        private void _dialogueDisplay_OnEndedWriting(IDialogueDisplay dialogueDisplay)
        {
            if(!_stateMachine.TrySetState(_waitForSelectButtonState))
            {
                _stateMachine.TrySetState(_waitForNextDialogueState);
            }
        }

        private void _dialogueSystem_OnStartedDialogue(IDialogueSystem dialogueSystem)
        {
            _dialogueDisplay.SetDialogue(_dialogueSystem.CurrentDialouge);
            _dialogueDisplay.StartWriting();

            if(dialogueSystem.CurrentDialouge is IHasGameEventDialogue eventDialogue)
            {
                eventDialogue.OnStart();
            }
        }

        private void _dialogueSystem_OnFinishedDialogue(IDialogueSystem dialogueSystem)
        {
            if (dialogueSystem.CurrentDialouge is IHasGameEventDialogue eventDialogue)
            {
                eventDialogue.OnFinish();
            }

            Deactivate();
        }

        private void _submitEventListener_OnSubmited(ISubmitEventListener submitEventListener, BaseEventData eventData)
        {
            if(_stateMachine.IsPlaying)
                _stateMachine.CurrentState.OnSubmitedTextArea();
        }

        private void SelectButton_OnSubmited(DialogueSelectButton dialogueSelectButton)
        {
            if (_stateMachine.IsPlaying)
                _stateMachine.CurrentState.OnSubmitedButton(dialogueSelectButton);
        }


        private void Invoke_OnStartedActivate()
        {
            Log(nameof(OnStartedActivate));

            OnStartedActivate?.Invoke(this);
        }
        private void Invoke_OnFinishedActivate()
        {
            Log(nameof(OnFinishedActivate));

            OnFinishedActivate?.Invoke(this);
        }
        private void Invoke_OnStartedDeactivate()
        {
            Log(nameof(OnStartedDeactivate));

            OnStartedDeactivate?.Invoke(this);
        }
        private void Invoke_OnFinishedDeactivate()
        {
            Log(nameof(OnDeactivateFinished));

            OnDeactivateFinished?.Invoke(this);
        }
    }
}
