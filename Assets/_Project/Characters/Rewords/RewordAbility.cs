using StudioScor.AbilitySystem;
using UnityEngine;
using UnityEngine.Localization;

namespace PF.PJT.Duet.Pawn.PawnSkill
{
    public abstract class RewordAbility : GASAbility, IRewordAbility, IDisplayIcon, IDisplayName, IDisplayDescription
    {
        [Header(" [ Reword Ability ] ")]
        [SerializeField] private Sprite _icon;
        [SerializeField] private LocalizedString _name;
        [SerializeField] private LocalizedString _description;

        public Sprite Icon => _icon;
        public string Name => _name.GetLocalizedString();
        public virtual string Description => _description.GetLocalizedString();
    }
}
