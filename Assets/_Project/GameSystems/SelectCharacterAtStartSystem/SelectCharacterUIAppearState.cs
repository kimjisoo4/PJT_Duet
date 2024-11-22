using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class SelectCharacterUIAppearState : SelectCharacterUIState
    {
        [Header(" [ Appear State ] ")]
        [SerializeField] private GameObject _uiActor;
        [SerializeField] private SelectCharacterUIState _nextState;

        [Header(" Tween ")]
        [SerializeField] private Vector3 _startPosition = new Vector3(0, -1000, 0);
        [SerializeField] private Vector3 _startScale = Vector3.zero;

        [Header(" Pop ")]
        [SerializeField] private float _popDelay = 0f;
        [SerializeField] private RectTransform _popUI;
        [SerializeField] private Vector3 _popPosition = Vector3.zero;
        [SerializeField] private float _popDuration = 1f;
        [SerializeField] private AnimationCurve _popEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header(" Flip ")]
        [SerializeField] private float _flipDelay = 0f;
        [SerializeField] private RectTransform _flipUI;
        [SerializeField] private Vector3 _flipScale = Vector3.one;
        [SerializeField] private float _flipDuration = 1f;
        [SerializeField] private AnimationCurve _flipEase = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        private Sequence _sequence;

        protected override void EnterState()
        {
            base.EnterState();

            if (_sequence is null)
            {
                _sequence = DOTween.Sequence().SetLink(gameObject).SetAutoKill(false);
                _sequence.AppendCallback(() =>
                {
                    _popUI.transform.localPosition = _startPosition;
                    _popUI.transform.localScale = _startScale;
                });
                _sequence.AppendInterval(_popDelay);
                _sequence.Append(_popUI.DOLocalMove(_popPosition, _popDuration).SetEase(_popEase));

                _sequence.AppendInterval(_flipDelay);
                _sequence.Append(_flipUI.DOScale(_flipScale, _flipDuration).SetEase(_flipEase));

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

            _popUI.transform.localPosition = _popPosition;
            _popUI.transform.localScale = _flipScale;
        }
    }
}
