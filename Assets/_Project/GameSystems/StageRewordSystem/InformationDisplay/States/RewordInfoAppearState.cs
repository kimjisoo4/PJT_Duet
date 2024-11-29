using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordInfoAppearState : RewordInformationDisplayState
    {
        [Header(" [ Reword Infomation Appear ] ")]
        [SerializeField] private GameObject _uiActor;

        [Header(" Tween ")]

        [Header(" Flash In ")]
        [SerializeField] private RectTransform _flashInActor;
        [SerializeField] private float _flashInDelay = 0f;
        [SerializeField] private float _flashInDuration = 0.5f;
        [SerializeField] private Vector3 _flashInStartScale = Vector3.zero;
        [SerializeField] private Vector3 _flashInEndScale = Vector3.one;
        [SerializeField] private TweenEase _flashInEase;

        private Sequence _sequence;

        protected override void EnterState()
        {
            base.EnterState();


            if (_sequence is null)
            {
                _sequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);

                _sequence.OnStart(() =>
                {
                    _flashInActor.localScale = _flashInStartScale;
                });

                _sequence.AppendInterval(_flashInDelay);
                _sequence.Append(_flashInActor.DOScale(_flashInEndScale, _flashInDuration).SetTweenEase(_flashInEase));

                _sequence.OnComplete(() => 
                { 
                    RewordInformationDisplay.FinishedState(this); 
                });
            }
            else
            {
                _sequence.Restart();
            }

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);
        }
    }
}
