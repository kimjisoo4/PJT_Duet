using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/States/Dialogue Controller Wait For Next Dialogue State")]
    public class DialogueControllerWaitForNextDialogueState : DialogueControllerState
    {
        public override void OnSubmitedTextArea()
        {
            base.OnSubmitedTextArea();

            DialogueSystem.NextDialogue();

            if (DialogueSystem.IsPlaying)
                DialogueDisplay.StartWriting();
        }
    }
}
