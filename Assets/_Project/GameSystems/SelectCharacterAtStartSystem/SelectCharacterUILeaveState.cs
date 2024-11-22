using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class SelectCharacterUILeaveState : SelectCharacterUIState
    {
        [Header(" [ Leave State ] ")]
        [SerializeField] private GameObject _uiActor;
        [SerializeField] private SelectCharacterUIState _nextState;

        [Header(" Tween ")]
        [SerializeField] private Vector3 _startPosition = Vector3.zero;

        [Header(" Push ")]
        [SerializeField] private float _pushDelay = 0f;
        [SerializeField] private RectTransform _pushUI;
        [SerializeField] private Vector3 _pushPosition = Vector3.zero;
        [SerializeField] private float _pushDuration = 1f;
        [SerializeField] private AnimationCurve _pushEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);


        private Sequence _sequence;

        protected override void EnterState()
        {
            base.EnterState();

            if (_sequence is null)
            {
                _sequence = DOTween.Sequence().SetLink(gameObject).SetAutoKill(false);
                _sequence.AppendCallback(() =>
                {
                    _pushUI.transform.localPosition = _startPosition;
                });
                _sequence.AppendInterval(_pushDelay);
                _sequence.Append(_pushUI.DOLocalMove(_pushPosition, _pushDuration).SetEase(_pushEase));

                _sequence.OnComplete(() =>
                {
                    SelectCharacterUI.FinishedState(this);
                });

                _sequence.Play();
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

            transform.localPosition = _pushPosition;
        }
    }
}
