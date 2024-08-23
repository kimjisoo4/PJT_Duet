using StudioScor.AbilitySystem;
using UnityEngine;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public abstract class RewordAbility : GASAbility, IRewordAbility, IDisplayIcon, IDisplayName, IDisplayDescription
    {
        [Header(" [ Reword Ability ] ")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private string _name;
        [SerializeField][TextArea] private string _description;

        public Sprite Icon => _icon;
        public string Name => _name;
        public virtual string Description => _description;
    }
}
