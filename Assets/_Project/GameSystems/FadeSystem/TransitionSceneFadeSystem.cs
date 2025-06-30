using StudioScor.Utilities;
using StudioScor.Utilities.FadeSystem;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class TransitionSceneFadeSystem : FadeSystemComponent
    {
        [Header(" [ Transition Scene Fade System ] ")]
        [Header(" Block Input ")]
        [SerializeField] private InputBlocker _inputBlocker;
        [SerializeField] private EBlockInputState _blockInput;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_inputBlocker)
            {
                _inputBlocker = SUtility.FindAssetByType<InputBlocker>();
            }
#endif
        }
        private void OnDestroy()
        {
            _inputBlocker.UnblockInput(this);
        }

        protected override void OnStartFadeOut()
        {
            base.OnStartFadeOut();

            _inputBlocker.BlockInput(this, _blockInput);
        }

        protected override void OnEndFadeIn()
        {
            base.OnEndFadeIn();

            _inputBlocker.UnblockInput(this);
        }
    }
}
