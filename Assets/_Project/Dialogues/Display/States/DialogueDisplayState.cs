using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/Display/States/Dialogue Display State")]
    public class DialogueDisplayState : BaseStateMono
    {
        [System.Serializable]
        public class StateMachine : FiniteStateMachineSystem<DialogueDisplayState> { }

        [Header(" [ Dialogue Display State ] ")]
        [SerializeField] private DuetDialogueDisplay _dialogueDisplay;

        public DuetDialogueDisplay DialogueDisplay => _dialogueDisplay;


        protected override void OnDestroy()
        {
            base.OnDestroy();

#if UNITY_EDITOR
            if (!_dialogueDisplay)
            {
                _dialogueDisplay = GetComponentInParent<DuetDialogueDisplay>();
            }            
#endif
        }
    }
}
