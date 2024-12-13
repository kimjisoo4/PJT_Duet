using StudioScor.Utilities.FadeSystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [AddComponentMenu("Duet/GameFlow/State/Wait For Fade In State")]
    public class GameFlowWaitForFadeInState : GameFlowState
    {
        [Header(" [ Wait For Fade In State ] ")]
        [SerializeField] private GameObject _fadeSystemActor;
        [SerializeField] private float _fadeInTime = 1f;

        private IFadeSystem _fadeSystem;

        public override bool CanEnterState()
        {
            if (!base.CanEnterState())
                return false;

            if (!_fadeSystemActor || !_fadeSystemActor.TryGetComponent(out _fadeSystem))
                return false;

            if (_fadeSystem.State == EFadeState.FadeIn && !_fadeSystem.IsFading)
                return false;

            return true;
        }

        protected override void EnterState()
        {
            base.EnterState();

            if (_fadeSystemActor && _fadeSystemActor.TryGetComponent(out _fadeSystem))
            {
                _fadeSystem.OnFinishedFadeIn += FadeSystem_OnFinishedFadeIn;

                if (!_fadeSystem.IsFading)
                {
                    _fadeSystem.StartFadeIn(_fadeInTime);
                }
            }
            else
            {
                TryNextState();
            }
        }
        protected override void ExitState()
        {
            base.ExitState();

            if(_fadeSystem is not null)
            {
                _fadeSystem.OnFinishedFadeIn -= FadeSystem_OnFinishedFadeIn;
            }
        }

        private void FadeSystem_OnFinishedFadeIn(IFadeSystem fadeSystem)
        {
            TryNextState();
        }
    }
}
