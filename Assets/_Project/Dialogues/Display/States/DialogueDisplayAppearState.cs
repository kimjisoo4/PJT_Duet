using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet.DialogueSystem
{
    [AddComponentMenu("Duet/DialougeSystem/Display/States/Dialogue Display Appear State")]
    public class DialogueDisplayAppearState : DialogueDisplayState
    {
        [Header(" [ Appear State ] ")]
        [SerializeField] private GameObject _uiActor;

        [Header(" Tweens ")]
        [Header(" Fade ")]
        [SerializeField] private float _fadeDelay = 0f;
        [SerializeField] private float _fadeDuration = 1f;
        [SerializeField] private CanvasGroup _fadeCanvas;

        private Sequence _sequence;

        protected override void EnterState()
        {
            base.EnterState();

            _fadeCanvas.alpha = 0;

            if (_sequence is null)
            {
                _sequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject).SetUpdate(true);
                _sequence.AppendInterval(_fadeDelay);
                _sequence.Append(_fadeCanvas.DOFade(1f, _fadeDuration));
                _sequence.OnComplete(() =>
                {
                    DialogueDisplay.FinishedState(this);
                });
            }
            else
            {
                _sequence.Restart();
            }

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);
        }

        protected override void ExitState()
        {
            base.ExitState();

            _sequence.Complete(true);
        }
    }
}
