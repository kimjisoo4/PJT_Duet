using DG.Tweening;
using UnityEngine;

namespace PF.PJT.Duet
{
    [System.Serializable]
    public class TweenEase
    {
        [SerializeField] private Ease _ease = Ease.Linear;
        [SerializeField] private AnimationCurve _curve = AnimationCurve.Linear(0,0,1,1);

        public Ease Ease => _ease;
        public AnimationCurve Curve => _curve;
    }
}
