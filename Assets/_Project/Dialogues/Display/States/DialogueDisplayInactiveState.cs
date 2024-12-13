using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/Display/States/Dialogue Display Inactive State")]
    public class DialogueDisplayInactiveState : DialogueDisplayState
    {
        [Header(" [ Appear State ] ")]
        [SerializeField] private GameObject _uiActor;
        [SerializeField] private CanvasGroup _fadeCanvas;


        protected override void EnterState()
        {
            if (!_uiActor.activeSelf)
                _uiActor.SetActive(false);

            _fadeCanvas.alpha = 0f;
        }
    }
}
