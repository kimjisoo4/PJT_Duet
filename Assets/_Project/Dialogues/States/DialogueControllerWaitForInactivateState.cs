using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/States/Dialogue Controller Wait For Inactivate State")]
    public class DialogueControllerWaitForInactivateState : DialogueControllerState
    {
        [Header(" [ Active State ] ")]
        [SerializeField] private GameObject _uiActor;

        protected override void EnterState()
        {
            base.EnterState();

            DialogueDisplay.Inactivate();

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);
        }
    }
}
