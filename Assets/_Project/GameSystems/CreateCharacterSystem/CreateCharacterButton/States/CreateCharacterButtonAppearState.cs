using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Button/State/Create Character Button Appear State")]
    public class CreateCharacterButtonAppearState : CreateCharacterButtonState
    {
        [Header(" [ Appear State ] ")]
        [SerializeField] private GameObject _uiActor;

        [Header(" Tween ")]

        [Header(" Pop ")]
        [SerializeField] private RectTransform _popUI;
        [SerializeField] private float _popDelay = 0f;
        [SerializeField] private Vector3 _popStartPosition = new Vector3(0, -1000, 0);
        [SerializeField] private Vector3 _popEndPosition = Vector3.zero;
        [SerializeField] private float _popDuration = 1f;
        [SerializeField] private TweenEase _popEase;

        [Header(" Flip ")]
        [SerializeField] private RectTransform _flipUI;
        [SerializeField] private float _flipDelay = 0f;
        [SerializeField] private Vector3 _flipStartScale = Vector3.zero;
        [SerializeField] private Vector3 _flipEndScale = Vector3.one;
        [SerializeField] private float _flipDuration = 1f;
        [SerializeField] private TweenEase _flipEase;

        private Sequence _sequence;

        protected override void EnterState()
        {
            base.EnterState();

            _popUI.transform.localPosition = _popStartPosition;
            _popUI.transform.localScale = _flipStartScale;

            if (_sequence is null)
            {
                _sequence = DOTween.Sequence().SetLink(gameObject).SetAutoKill(false).SetUpdate(true);
                _sequence.AppendInterval(_popDelay);
                _sequence.Append(_popUI.DOLocalMove(_popEndPosition, _popDuration).SetTweenEase(_popEase));

                _sequence.AppendInterval(_flipDelay);
                _sequence.Append(_flipUI.DOScale(_flipEndScale, _flipDuration).SetTweenEase(_flipEase));

                _sequence.OnComplete(() =>
                {
                    CreateCharacterButton.FinishedState(this);
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

            _popUI.transform.localPosition = _popEndPosition;
            _popUI.transform.localScale = _flipEndScale;
        }
    }
}
