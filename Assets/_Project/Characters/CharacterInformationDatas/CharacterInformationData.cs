using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace PF.PJT.Duet.Pawn
{
    [CreateAssetMenu(menuName = "Project/Duet/Data/new Character Information Data", fileName = "Data_CharacterInformation_")]
    public class CharacterInformationData : BaseScriptableObject, IDisplayName, IDisplayDescription, IDisplayIcon
    {
        [Header(" [ Character Information Data ] ")]
        [SerializeField] private string _id;
        [SerializeField] private PoolContainer _pool;
        [SerializeField] private LocalizedString _name;
        [SerializeField] private LocalizedString _description;
        [SerializeField] private Sprite _icon;

        [Header(" Skills ")]
        [SerializeField] private CharacterSkill[] _activeSkills;
        [SerializeField] private CharacterSkill[] _appearSkills;
        [SerializeField] private CharacterSkill[] _defaultSkills;


        public string ID => _id;
        public string Name => _name.GetLocalizedString();
        public string Description => _description.GetLocalizedString();
        public Sprite Icon => _icon;
        public PoolContainer Pool => _pool;
        public IReadOnlyCollection<Ability> AppearSkills => _appearSkills;
        public IReadOnlyCollection<Ability> ActiveSkills => _activeSkills;
        public IReadOnlyCollection<Ability> DefaultSkills => _defaultSkills;


        [ContextMenu(nameof(NameToID))]
        private void NameToID()
        {
            _id = name;
        }
    }
}
