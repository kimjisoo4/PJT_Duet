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

        protected override void OnValidate()
        {
#if UNITY_EDITOR
            base.OnValidate();

            if (SUtility.IsPlayingOrWillChangePlaymode)
                return;

            if(!_rewordInformationDisplay)
            {
                _rewordInformationDisplay = GetComponentInParent<RewordInformationDisplay>();
            }
#endif
        }
    }
}
