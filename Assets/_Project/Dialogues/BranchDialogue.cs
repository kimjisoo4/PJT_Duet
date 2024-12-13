using StudioScor.Utilities;
using StudioScor.Utilities.DialogueSystem;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/Dialogue/new Branch Dialogue", fileName = "Dialogue_Branch_")]
    public class BranchDialogue : DuetDialogue, IBranchDialogue
    {
        [Header(" [ Branch Dialogue ] ")]
        [SerializeField] private IBranchDialogue.Branch[] _branches;
        public IReadOnlyCollection<IBranchDialogue.Branch> Branches => _branches;

        public IDialogue GetDialogue()
        {
            if (!_branches.IsNullOrEmpty())
            {
                foreach (var transition in _branches)
                {
                    if (transition.CanTransition())
                    {
                        return transition.Dialogue;
                    }
                }
            }

            return NextDialogue;
        }
    }
}
