using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class SelectRewordState : BaseStateMono
    {
        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<SelectRewordState> { }

        [Header(" [ Select Reword State ] ")]
        [SerializeField] private SelectRewordComponent _selectReword;
        public SelectRewordComponent SelectReword => _selectReword;

        protected virtual void OnValidate()
        {
            if(!_selectReword)
            {
                _selectReword = GetComponentInParent<SelectRewordComponent>();
            }
        }
    }
}
