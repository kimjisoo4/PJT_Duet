using StudioScor.Utilities;

namespace PF.PJT.Duet
{
    public abstract class PlayerDamageEventModifier : BaseMonoBehaviour
    {
        public abstract void OnHit(DamageInfoData damageInfo);
    }
}
