using StudioScor.Utilities;
using StudioScor.Utilities.DialogueSystem;
using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/States/Dialogue Controller State")]
    public class DialogueControllerState : BaseStateMono
    {
        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<DialogueControllerState> { }

        [Header(" [ Dialogue Controller State ] ")]
        [SerializeField] private DialogueController _dialogueController;
        public DialogueController DialogueController => _dialogueController;

        public IDialogueSystem DialogueSystem => _dialogueController.DialogueSystem;
        public IDialogueDisplay DialogueDisplay => _dialogueController.DialogueDisplay;

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            if (SUtility.IsPlayingOrWillChangePlaymode)
                return;

            base.OnValidate();

            if (!_dialogueController)
            {
                _dialogueController = GetComponentInParent<DialogueController>();
            }
#endif
        }

        public virtual void OnSubmitedTextArea()
        {
            return;
        }
        public virtual void OnSubmitedButton(DialogueSelectButton dialogueSelectButton)
        {
            return;
        }

    }
}
