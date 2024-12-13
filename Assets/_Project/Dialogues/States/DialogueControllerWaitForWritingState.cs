using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/States/Dialogue Controller Wait For Writing State")]
    public class DialogueControllerWaitForWritingState : DialogueControllerState
    {
        public override void OnSubmitedTextArea()
        {
            base.OnSubmitedTextArea();

            if (!IsPlaying)
                return;

            DialogueController.DialogueDisplay.SkipWriting();
        }
    }
}
