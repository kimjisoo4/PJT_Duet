using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/States/Dialogue Controller Wait For Deactivate State")]
    public class DialogueControllerWaitForDeactivateState : DialogueControllerState
    {
        [Header(" [ Active State ] ")]
        [SerializeField] private GameObject _uiActor;

        protected override void EnterState()
        {
            base.EnterState();

            DialogueDisplay.Deactivate();

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);
        }
    }
}
