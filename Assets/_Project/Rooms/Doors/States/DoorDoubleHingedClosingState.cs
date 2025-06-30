using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet
{
    [AddComponentMenu("Duet/Door/State/Door Double Hinged Closing State")]
    public class DoorDoubleHingedClosingState : DoorActorState
    {
        [Header(" [ Door Double Hinged Closing ] ")]
        [Header(" Tween ")]
        [SerializeField] private Transform _closeLeftDoor;
        [SerializeField] private Transform _closeRightDoor;
        [SerializeField] private float _closeDelay = 0f;
        [SerializeField] private float _closeDuration = 1f;
        [SerializeField] private Vector3 _closeAngleL = new Vector3(0, 0, 0);
        [SerializeField] private Vector3 _closeAngleR = new Vector3(0, 180, 0);
        [SerializeField] private TweenEase _closeEase;

        private Sequence _closeSequence;

        private void Awake()
        {
            _closeSequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);

            _closeSequence.AppendInterval(_closeDelay);
            _closeSequence.Append(_closeLeftDoor.DOLocalRotate(_closeAngleL, _closeDuration, RotateMode.Fast)).SetTweenEase(_closeEase);
            _closeSequence.Join(_closeRightDoor.DOLocalRotate(_closeAngleR, _closeDuration, RotateMode.Fast)).SetTweenEase(_closeEase);
            _closeSequence.OnComplete(() =>
            {
                DoorActor.FInishedState(this);
            });
            _closeSequence.Pause();
        }

        protected override void EnterState()
        {
            base.EnterState();

            if (_closeSequence is null)
            {
                _closeSequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);

                _closeSequence.AppendInterval(_closeDelay);
                _closeSequence.Append(_closeLeftDoor.DOLocalRotate(_closeAngleL, _closeDuration, RotateMode.Fast)).SetTweenEase(_closeEase);
                _closeSequence.Join(_closeRightDoor.DOLocalRotate(_closeAngleR, _closeDuration, RotateMode.Fast)).SetTweenEase(_closeEase);
                _closeSequence.OnComplete(() =>
                {
                    DoorActor.FInishedState(this);
                });
            }
            else
            {
                _closeSequence.Restart();
            }
        }
    }

}
