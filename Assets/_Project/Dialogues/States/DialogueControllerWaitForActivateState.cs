using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/States/Dialogue Controller Wait For Activate State")]
    public class DialogueControllerWaitForActivateState : DialogueControllerState
    {
        [Header(" [ Active State ] ")]
        [SerializeField] private GameObject _uiActor;

        protected override void EnterState()
        {
            base.EnterState();

            DialogueDisplay.Activate();

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);
        }
    }
}
