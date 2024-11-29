using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordInformationDisplayState : BaseStateMono
    {
        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<RewordInformationDisplayState> { }

        [Header(" [ Reword Information Display State ] ")]
        [SerializeField] private RewordInformationDisplay _rewordInformationDisplay;

        public RewordInformationDisplay RewordInformationDisplay => _rewordInformationDisplay;

        protected virtual void OnValidate()
        {
            if(!_rewordInformationDisplay)
            {
                _rewordInformationDisplay = GetComponentInParent<RewordInformationDisplay>();
            }
        }
    }
}
