using StudioScor.Utilities;
using StudioScor.Utilities.DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/Dialogue/new Select Dialogue", fileName = "Dialogue_Select_")]
    public class SelectDialogue : DuetDialogue, ISelectableDialogue
    {
        [Header(" [ Seelct Dialogue ] ")]
        [SerializeField] private Dialogue[] _dialogues;
        public IReadOnlyCollection<IDialogue> Dialogues => _dialogues;

        public IDialogue GetDialogue(int index)
        {
            if(!_dialogues.IsNullOrEmpty())
            {
                var dialogue = _dialogues.ElementAtOrDefault(index);

                return dialogue is not null ? dialogue.NextDialogue : NextDialogue;
            }
            else
            {
                return NextDialogue;
            }
        }
    }
}
