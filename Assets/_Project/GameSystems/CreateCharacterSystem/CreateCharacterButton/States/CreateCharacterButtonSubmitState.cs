using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    [AddComponentMenu("Duet/CreateCharacter/Button/State/Create Character Button Submit State")]
    public class CreateCharacterButtonSubmitState : CreateCharacterButtonState
    {
        [Header(" [ Submit State ] ")]
        [SerializeField] private GameObject _uiActor;

        [Header(" Tween ")]
        [Header(" Expand ")]
        [SerializeField] private RectTransform _expandActor;
        [SerializeField] private float _expandDelay = 0f;
        [SerializeField] private float _expandDuration = 0.5f;
        [SerializeField] private Vector3 _startExpandScale = Vector3.one;
        [SerializeField] private Vector3 _expandScale = Vector3.one * 1.1f;
        [SerializeField] private TweenEase _expandEase;

        [Header(" Shrink ")]
        [SerializeField] private RectTransform _shrinkActor;
        [SerializeField] private float _shrinkDelay = 0f;
        [SerializeField] private float _shrinkDuration = 0.5f;
        [SerializeField] private Vector3 _shrinkScale = Vector3.one * 1.1f;
        [SerializeField] private TweenEase _shrinkEase;
        private Sequence _sequence;

        public override bool CanEnterState()
        {
            return base.CanEnterState() && CreateCharacterButton.CharacterData is null && !CreateCharacterButton.IsInTransition;
        }

        protected override void EnterState()
        {
            base.EnterState();

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);

            _expandActor.localScale = _startExpandScale;

            if (_sequence is null)
            {
                _sequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject).SetUpdate(true);
                _sequence.AppendInterval(_expandDelay);
                _sequence.Append(_expandActor.DOScale(_expandScale, _expandDuration).SetTweenEase(_expandEase));
                _sequence.AppendInterval(_shrinkDelay);
                _sequence.Append(_shrinkActor.DOScale(_shrinkScale, _shrinkDuration).SetTweenEase(_shrinkEase));
            }
            else
            {
                _sequence.Restart();
            }
        }

        protected override void ExitState()
        {
            base.ExitState();

            _sequence.Complete();
        }
    }
}
