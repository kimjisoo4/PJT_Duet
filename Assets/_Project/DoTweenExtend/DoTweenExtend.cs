using DG.Tweening;

namespace PF.PJT.Duet
{
    public static class DoTweenExtend
    {
        public static T SetTweenEase<T>(this T tween, TweenEase ease) where T : Tween
        {
            if(ease.Ease == Ease.INTERNAL_Custom)
            {
                return tween.SetEase(ease.Curve);
            }
            else
            {
                return tween.SetEase(ease.Ease);
            }
        }
    }
}
