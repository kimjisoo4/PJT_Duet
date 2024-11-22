using System.Collections;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class AnimateDoorActor : DoorActor
    {
        [Header(" [ Animate Door Actor ] ")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _openAnim = "Open";
        [SerializeField] private string _closeAnim = "Close";

        private Coroutine _coroutine = null;

        protected override void OnOpen()
        {
            if(_coroutine is not null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _coroutine = StartCoroutine(WaitFinishAnimation(true, GetAnimationHash(true)));
        }

        protected override void OnClose()
        {
            if (_coroutine is not null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _coroutine = StartCoroutine(WaitFinishAnimation(false, GetAnimationHash(false)));
        }

        private int GetAnimationHash(bool isOpening)
        {
            return isOpening ? Animator.StringToHash(_openAnim) : Animator.StringToHash(_closeAnim);
        }

        private IEnumerator WaitFinishAnimation(bool isOpening, int hash)
        {
            _animator.Play(hash);

            while(true)
            {
                yield return null;

                var current = _animator.GetCurrentAnimatorStateInfo(0);
                
                if(current.shortNameHash == hash && current.normalizedTime > 1f)
                {
                    if (isOpening)
                    {
                        FinisheOpen();
                        yield break;
                    }
                    else
                    {
                        FinisheClose();
                        yield break;
                    }
                }
            }
        }
    }

}
