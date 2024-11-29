using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordInfoLeaveState : RewordInformationDisplayState
    {
        [Header(" [ Reword Infomation Leave ] ")]
        [SerializeField] private GameObject _uiActor;

        [Header(" Tween ")]
        [Header(" Flash Out ")]
        [SerializeField] private RectTransform _flashOutActor;
        [SerializeField] private float _flashOutDelay = 0f;
        [SerializeField] private float _flashOutDuration = 0.5f;
        [SerializeField] private Vector3 _flashOutStartScale = Vector3.one;
        [SerializeField] private Vector3 _flashOutEndScale = Vector3.zero;
        [SerializeField] private TweenEase _flashOutEase;

        private Sequence _sequence;

        protected override void EnterState()
        {
            base.EnterState();

            if (_sequence is null)
            {
                _sequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);

                _sequence.OnStart(() =>
                {
                    _flashOutActor.localScale = _flashOutStartScale;
                });
                _sequence.AppendInterval(_flashOutDelay);
                _sequence.Append(_flashOutActor.DOScale(_flashOutEndScale, _flashOutDuration).SetTweenEase(_flashOutEase));
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
