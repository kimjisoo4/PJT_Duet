using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet
{
    [AddComponentMenu("Duet/Door/State/Door Double Hinged Opening State")]
    public class DoorDoubleHingedOpeningState : DoorActorState
    {
        [Header(" [ Door Double Hinged Opening ] ")]
        [Header(" Tween ")]
        [SerializeField] private Transform _openLeftDoor;
        [SerializeField] private Transform _openRightDoor;
        [SerializeField] private float _openDelay = 0f;
        [SerializeField] private float _openDuration = 1f;
        [SerializeField] private Vector3 _openAngleL = new Vector3(0, 90, 0);
        [SerializeField] private Vector3 _openAngleR = new Vector3(0, -90, 0);
        [SerializeField] private TweenEase _openEase;

        private Sequence _openSequence;

        private void Awake()
        {
            _openSequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);

            _openSequence.AppendInterval(_openDelay);
            _openSequence.Append(_openLeftDoor.DOLocalRotate(_openAngleL, _openDuration, RotateMode.Fast)).SetTweenEase(_openEase);
            _openSequence.Join(_openRightDoor.DOLocalRotate(_openAngleR, _openDuration, RotateMode.Fast)).SetTweenEase(_openEase);
            _openSequence.OnComplete(() =>
            {
                DoorActor.FInishedState(this);
            });
            _openSequence.Pause();
        }

        protected override void EnterState()
        {
            base.EnterState();

            if(_openSequence is null)
            {
                _openSequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);

                _openSequence.AppendInterval(_openDelay);
                _openSequence.Append(_openLeftDoor.DOLocalRotate(_openAngleL, _openDuration, RotateMode.Fast)).SetTweenEase(_openEase);
                _openSequence.Join(_openRightDoor.DOLocalRotate(_openAngleR, _openDuration, RotateMode.Fast)).SetTweenEase(_openEase);
                _openSequence.OnComplete(() =>
                {
                    DoorActor.FInishedState(this);
                });
            }
            else
            {
                _openSequence.Restart();
            }
        }
    }

}
