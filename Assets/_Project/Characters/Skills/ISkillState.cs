namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface ISkillState
    {
        public float CoolTime { get; }
        public float RemainCoolTime { get; }
        public float NormalizedCoolTime { get; }
    }
}
