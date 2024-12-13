using StudioScor.Utilities;
using StudioScor.Utilities.DialogueSystem;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PF.PJT.Duet.DialogueSystem
{

    [AddComponentMenu("Duet/DialougeSystem/States/Dialogue Controller Wait For Select Button State")]
    public class DialogueControllerWaitForSelectButtonState : DialogueControllerState
    {
        [Header(" [ Wait For Select Button State ] ")]
        [SerializeField] private DialogueSelectButton[] _dialogueSelectButtons;

        public override bool CanEnterState()
        {
            if (!base.CanEnterState())
                return false;

            if (DialogueSystem.CurrentDialouge is not ISelectableDialogue selectableDialogue || selectableDialogue.Dialogues.IsNullOrEmpty())
                return false;

            return true;
        }

        protected override void EnterState()
        {
            base.EnterState();

            var selectableDialogue = DialogueSystem.CurrentDialouge as ISelectableDialogue;

            if (!selectableDialogue.Dialogues.IsNullOrEmpty())
            {
                for (int i = 0; i < selectableDialogue.Dialogues.Count; i++)
                {
                    var selectableDialogues = selectableDialogue.Dialogues.ElementAt(i);
                    var button = _dialogueSelectButtons.ElementAt(i);

                    button.SetSelectableDialogue(selectableDialogues);
                    button.Activate();
                }

                EventSystem.current.SetSelectedGameObject(_dialogueSelectButtons.ElementAt(0).gameObject);
            }
        }

        protected override void ExitState()
        {
            base.ExitState();

            foreach (var selectButton in _dialogueSelectButtons)
            {
                selectButton.Inactivate();
            }
        }

        public override void OnSubmitedButton(DialogueSelectButton dialogueSelectButton)
        {
            base.OnSubmitedButton(dialogueSelectButton);

            DialogueSystem.NextDialogue(dialogueSelectButton.Dialogue.NextDialogue);

            if (DialogueSystem.IsPlaying)
                DialogueDisplay.StartWriting();
        }
    }
}
