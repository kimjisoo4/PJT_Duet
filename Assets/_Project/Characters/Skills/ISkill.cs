using PF.PJT.Duet.Pawn.Effect;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface ISkill
    {
        public Sprite Icon { get; }
        public ESkillType SkillType { get; }
    }
    public interface ISkillState
    {
        public float CoolTime { get; }
        public float RemainCoolTime { get; }
        public float NormalizedCoolTime { get; }
    }
}
