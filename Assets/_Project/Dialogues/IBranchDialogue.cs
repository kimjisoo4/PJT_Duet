using StudioScor.Utilities;
using StudioScor.Utilities.DialogueSystem;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    public interface IBranchDialogue
    {
        [System.Serializable]
        public class Branch
        {
            [SerializeField] private ConditionVariable[] _conditions;
            [SerializeField] private Dialogue _dialogue;

            public IReadOnlyCollection<ConditionVariable> Conditions => _conditions;
            public Dialogue Dialogue => _dialogue;

            public bool CanTransition()
            {
                if (_conditions.IsNullOrEmpty())
                    return false;

                foreach (var condition in _conditions)
                {
                    if (!condition.CheckCondition())
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public IReadOnlyCollection<Branch> Branches { get; }

        public IDialogue GetDialogue();
    }
}
