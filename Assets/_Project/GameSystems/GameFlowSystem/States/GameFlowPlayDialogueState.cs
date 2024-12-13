using PF.PJT.Duet.DialogueSystem;
using StudioScor.Utilities.DialogueSystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [AddComponentMenu("Duet/GameFlow/State/Play Dialouge State")]
    public class GameFlowPlayDialogueState : GameFlowState
    {
        [Header(" [ Play Dialogue Flow State ] ")]
        [SerializeField] private DialogueController _dialogueController;
        [SerializeField] private Dialogue _dialogue;

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            base.OnValidate();

            if(!_dialogueController)
            {
                _dialogueController = FindAnyObjectByType<DialogueController>();
            }
#endif
        }
        public override bool CanEnterState()
        {
            if (!base.CanEnterState())
                return false;

            if (!_dialogue)
                return false;

            if (!_dialogueController)
                return false;

            return true;
        }

        protected override void EnterState()
        {
            base.EnterState();

            _dialogueController.OnFinishedInactivate += _dialogueController_OnFinishedInactivate;

            _dialogueController.Activate(_dialogue);
        }

        private void _dialogueController_OnFinishedInactivate(DialogueController dialogueController)
        {
            TryNextState();
        }

        protected override void ExitState()
        {
            base.ExitState();

            if(_dialogueController)
            {
                _dialogueController.OnFinishedInactivate -= _dialogueController_OnFinishedInactivate;
            }
        }
    }
}
