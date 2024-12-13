using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public abstract class ConditionVariable : BaseScriptableObject
    {
        [Header(" [ Condition ] ")]
        [SerializeField] private bool _isInvertResult = false;

        public bool CheckCondition()
        {
            return Condition() != _isInvertResult;
        }
        protected abstract bool Condition();
    }
}
