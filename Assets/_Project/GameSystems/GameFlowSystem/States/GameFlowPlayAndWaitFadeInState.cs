using StudioScor.Utilities.FadeSystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [AddComponentMenu("Duet/GameFlow/State/Play and Wait For Fade In State")]
    public class GameFlowPlayAndWaitFadeInState : GameFlowState
    {
        [Header(" [ Wait For Fade In State ] ")]
        [SerializeField] private ScriptableFadeSystem _fadeSystem;
        [SerializeField] private float _fadeInTime = 1f;

        public override bool CanEnterState()
        {
            if (!base.CanEnterState())
                return false;

            if (_fadeSystem.State == EFadeState.FadeIn && !_fadeSystem.IsFading)
            {
                Log($"Fade State : {_fadeSystem.State} || Is Fading : {_fadeSystem.IsFading}");
                return false;
            }

            return true;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_fadeSystem)
            {
                _fadeSystem.OnFadeInFinished -= FadeSystem_OnFinishedFadeIn;
            }
        }

        protected override void EnterState()
        {
            base.EnterState();

            _fadeSystem.OnFadeInFinished += FadeSystem_OnFinishedFadeIn;
            
            if(!_fadeSystem.IsFading)
            {
                _fadeSystem.StartFadeIn(_fadeInTime, true);
            }
        }

        protected override void ExitState()
        {
            base.ExitState();

            if(_fadeSystem)
            {
                _fadeSystem.OnFadeInFinished -= FadeSystem_OnFinishedFadeIn;
            }
        }

        private void FadeSystem_OnFinishedFadeIn(IFadeSystem fadeSystem)
        {
            ComplateState();
        }
    }
}
