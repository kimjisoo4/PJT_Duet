using DG.Tweening;
using StudioScor.Utilities;
using StudioScor.Utilities.DialogueSystem;
using TMPro;
using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    public class DuetDialogueDisplay : DialogueDisplay
    {
        [Header(" [ Duet Dialogue Display ] ")]
        [SerializeField] private GameObject _dataComponentActor;
        [SerializeField] private TMP_Text _dialogueField;
        [SerializeField] private float _writingSpeed = 50f;

        [Header(" State Machine ")]
        [SerializeField] private DialogueDisplayState.StateMachine _stateMachine;
        [SerializeField] private DialogueDisplayState _transitionActiveState;
        [SerializeField] private DialogueDisplayState _activeState;
        [SerializeField] private DialogueDisplayState _transitionInactiveState;
        [SerializeField] private DialogueDisplayState _inactiveState;

        private bool _isPlaying = false;
        private IDataComponent _dataComponent;
        private Tweener _writingTweener;

        protected void OnValidate()
        {
            if(!_dataComponentActor)
            {
                if(gameObject.TryGetComponentInParentOrChildren(out IDataComponent dataComponent))
                {
                    _dataComponentActor = dataComponent.gameObject;
                }
            }
        }

        protected override void OnInit()
        {
            base.OnInit();

            _dataComponent = _dataComponentActor.GetComponent<IDataComponent>();

            _dialogueField.text = null;
            _dataComponent.UpdateData(null);

            _stateMachine.Start();
        }

        public override void Activate()
        {
            if (_isPlaying)
                return;

            _isPlaying = true;

            Invoke_OnActivated();

            _stateMachine.TrySetState(_transitionActiveState);

        }
        public override void Inactivate()
        {
            if (!_isPlaying)
                return;

            _isPlaying = false;

            _dataComponent.UpdateData(null);
            _dialogueField.text = null;

            _stateMachine.TrySetState(_transitionInactiveState);

            Invoke_OnStartedBlendOut();
        }

        protected override void OnStartWriting()
        {
            _dataComponent.UpdateData(Dialogue);

            _dialogueField.text = null;

            if (_writingTweener is null)
            {
                _writingTweener = _dialogueField.DOText(Dialogue.DialogueText, _writingSpeed)
                                                .SetSpeedBased()
                                                .OnComplete(() => EndWriting())
                                                .SetLink(gameObject).SetAutoKill(false)
                                                .SetUpdate(true);
                
            }
            else
            {
                _writingTweener.ChangeEndValue(Dialogue.DialogueText, _writingSpeed);
                _writingTweener.Restart();
            }
        }

        protected override void OnSkipWriting()
        {
            base.OnSkipWriting();

            _writingTweener.Complete();
        }

        public void FinishedState(DialogueDisplayState finishedState)
        {
            if(finishedState == _transitionActiveState)
            {
                _stateMachine.TrySetState(_activeState);

                Invoke_OnFinishedBlendIn();
            }
            else if(finishedState == _transitionInactiveState)
            {
                _stateMachine.TrySetState(_inactiveState);

                Invoke_OnInactivated();
            }
        }
    }
}
