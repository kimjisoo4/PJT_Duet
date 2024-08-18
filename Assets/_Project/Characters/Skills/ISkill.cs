using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface ISkill
    {
        public string Name { get; }
        public Sprite Icon { get; }
        public ESkillType SkillType { get; }
        public string GetDescription();
    }
}
