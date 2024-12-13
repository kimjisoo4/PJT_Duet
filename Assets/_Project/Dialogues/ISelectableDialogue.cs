using StudioScor.Utilities.DialogueSystem;
using System.Collections.Generic;

namespace PF.PJT.Duet
{
    public interface ISelectableDialogue
    {
        public IReadOnlyCollection<IDialogue> Dialogues { get; }
        public IDialogue GetDialogue(int index);
    }
}
