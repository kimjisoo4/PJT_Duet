using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/States/Dialogue Controller Inactive State")]
    public class DialogueControllerInactiveState : DialogueControllerState
    {
        [Header(" [ Active State ] ")]
        [SerializeField] private GameObject _uiActor;

        protected override void EnterState()
        {
            base.EnterState();

            if (_uiActor.activeSelf)
                _uiActor.SetActive(false);
        }
    }
}
