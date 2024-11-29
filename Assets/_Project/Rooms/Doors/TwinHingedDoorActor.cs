using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class TwinHingedDoorActor : DoorActor
    {
        [Header(" [ Twin Hinged Door Actor ] ")]
        [SerializeField] private Transform _leftDoor;
        [SerializeField] private Transform _rightDoor;

        [Header(" Tween ")]
        [Header(" Oepn ")]
        [SerializeField] private float _openDuration = 1f;
        [SerializeField] private Vector3 _openAngle;
        [SerializeField] private TweenEase _openEase;

        [Header(" Clsoe ")]
        [SerializeField] private float _closeDuration = 1f;
        [SerializeField] private Vector3 _closeAngle;
        [SerializeField] private TweenEase _closeEase;

        private Sequence _openSequence;
        private Sequence _closeSequence;

        protected override void OnOpen()
        {
            if(_openSequence is null)
            {
                _openSequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);

                _openSequence.Append(_leftDoor.DOLocalRotate(_openAngle, _openDuration).SetTweenEase(_openEase));
                _openSequence.Join(_rightDoor.DOLocalRotate(-_openAngle, _openDuration).SetTweenEase(_openEase));
                _openSequence.OnComplete(FinisheOpen);
            }
            else
            {
                _openSequence.Restart();
            }
        }

        protected override void OnClose()
        {
            if(_closeSequence is null)
            {
                _closeSequence = DOTween.Sequence().SetAutoKill(false).SetLink(gameObject);

                _closeSequence.Append(_leftDoor.DOLocalRotate(_closeAngle, _closeDuration).SetTweenEase(_closeEase));
                _closeSequence.Join(_rightDoor.DOLocalRotate(-_closeAngle, _closeDuration).SetTweenEase(_closeEase));
                _closeSequence.OnComplete(FinisheClose);
            }
            else
            {
                _closeSequence.Restart();
            }
            
            return;
        }
    }
}
