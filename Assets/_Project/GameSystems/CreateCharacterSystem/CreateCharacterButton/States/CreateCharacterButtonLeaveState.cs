using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Button/State/Create Character Button Leave State")]
    public class CreateCharacterButtonLeaveState : CreateCharacterButtonState
    {
        [Header(" [ Leave State ] ")]
        [SerializeField] private GameObject _uiActor;
        [SerializeField] private CreateCharacterButtonState _nextState;

        [Header(" Tween ")]
        [Header(" Push ")]
        [SerializeField] private RectTransform _pushUI;
        [SerializeField] private float _pushDelay = 0f;
        [SerializeField] private Vector3 _pushStartPosition = Vector3.zero;
        [SerializeField] private Vector3 _pushEndPosition = Vector3.zero;
        [SerializeField] private float _pushDuration = 1f;
        [SerializeField] private TweenEase _pushEase;


        private Sequence _sequence;

        protected override void EnterState()
        {
            base.EnterState();

            _pushUI.transform.localPosition = _pushStartPosition;

            if (_sequence is null)
            {
                _sequence = DOTween.Sequence().SetLink(gameObject).SetAutoKill(false).SetUpdate(true);
                _sequence.AppendInterval(_pushDelay);
                _sequence.Append(_pushUI.DOLocalMove(_pushEndPosition, _pushDuration).SetTweenEase(_pushEase));

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

            transform.localPosition = _pushEndPosition;
        }
    }
}
