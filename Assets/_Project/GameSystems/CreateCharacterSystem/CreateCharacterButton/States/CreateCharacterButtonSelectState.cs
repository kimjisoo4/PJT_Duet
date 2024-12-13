using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{


    [AddComponentMenu("Duet/CreateCharacter/Button/State/Create Character Button Select State")]
    public class CreateCharacterButtonSelectState : CreateCharacterButtonState
    {
        [Header(" [ Select State ] ")]
        [SerializeField] private GameObject _uiActor;

        [Header(" Tween ")]
        [SerializeField] private RectTransform _expandActor;
        [SerializeField] private float _expandDelay = 0f;
        [SerializeField] private float _expandDuration = 0.5f;
        [SerializeField] private Vector3 _startScale = Vector3.one;
        [SerializeField] private Vector3 _expandScale = Vector3.one * 1.1f;
        [SerializeField] private TweenEase _expandEase;

        private Sequence _sequence;

        public override bool CanEnterState()
        {
            return base.CanEnterState() && CreateCharacterButton.CharacterData is not null && !CreateCharacterButton.IsInTransition;
        }

        protected override void EnterState()
        {
            base.EnterState();

            if (!_uiActor.activeSelf)
                _uiActor.SetActive(true);

            _expandActor.localScale = _startScale;

            if (_sequence is null)
            {
                _sequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject).SetUpdate(true);
                _sequence.AppendInterval(_expandDelay);
                _sequence.Append(_expandActor.DOScale(_expandScale, _expandDuration).SetTweenEase(_expandEase));
            }
            else
            {
                _sequence.Restart();
            }
        }

        protected override void ExitState()
        {
            base.ExitState();

            if (_sequence.IsPlaying())
                _sequence.Pause();

            _expandActor.localScale = _startScale;
        }
    }
}
