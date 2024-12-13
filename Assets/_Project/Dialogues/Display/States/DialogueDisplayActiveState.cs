using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/Display/States/Dialogue Display Active State")]
    public class DialogueDisplayActiveState : DialogueDisplayState
    {
        [Header(" [ Appear State ] ")]
        [SerializeField] private GameObject _uiActor;
        [SerializeField] private CanvasGroup _fadeCanvas;


        protected override void EnterState()
        {
            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);

            _fadeCanvas.alpha = 1f;
        }
    }
}
