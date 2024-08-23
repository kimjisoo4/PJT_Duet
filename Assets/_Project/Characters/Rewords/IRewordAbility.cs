using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public interface IRewordAbility
    {
        public Sprite Icon { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
