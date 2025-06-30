using StudioScor.Utilities.FadeSystem;
using UnityEngine;

namespace PF.PJT.Duet.GameFlowSystem
{
    [CreateAssetMenu(menuName = "Project/Duet/GameFlowSystem/new FadeStatedDecision", fileName = "Decision_FadeState")]
    public class FadeStateDecision : GameFlowSystemDecision
    {
        [Header(" [ Fade State Decision ] ")]
        [SerializeField] private ScriptableFadeSystem _fadeSystem;
        [SerializeField] private EFadeState _fadeState;

        public override bool Decide(object owner)
        {
            return _fadeSystem.State == _fadeState && !_fadeSystem.IsFading;
        }
    }
}
