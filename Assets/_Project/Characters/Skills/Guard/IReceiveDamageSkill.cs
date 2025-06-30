using StudioScor.Utilities;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface IReceiveDamageSkill
    {
        public bool TryActiveReceiveDamageSkill(DamageInfoData damageInfoData);
    }
}
