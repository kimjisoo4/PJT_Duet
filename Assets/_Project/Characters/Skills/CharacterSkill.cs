using StudioScor.AbilitySystem;
using UnityEngine;


namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public abstract class CharacterSkill : GASAbility, ISkill
    {
        [Header(" [ Character Skill ] ")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _name;
        [SerializeField] private ESkillType _skillType = ESkillType.Skill;
        [SerializeField][TextArea] private string _description;

        public Sprite Icon => _icon;
        public string Name => _name;
        public ESkillType SkillType => _skillType;
        public string Description => _description;

        public virtual string GetDescription()
        {
            return Description;
        }
    }
}
