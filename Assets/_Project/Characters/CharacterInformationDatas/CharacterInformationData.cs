using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet.Pawn
{
    [CreateAssetMenu(menuName = "Project/Duet/Data/new Character Information Data", fileName = "Data_CharacterInformation_")]
    public class CharacterInformationData : BaseScriptableObject, IDisplayName, IDisplayDescription, IDisplayIcon
    {
        [Header(" [ Character Information Data ] ")]
        [SerializeField] private string _id;
        [SerializeField] private GameObject _actor;
        [SerializeField] private string _name;
        [SerializeField][TextArea]private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Ability[] _appearSkills;
        [SerializeField] private Ability[] _passiveSkills;
        [SerializeField] private Ability[] _activeSkills;


        public string ID => _id;
        public string Name => _name;
        public string Description => _description;
        public Sprite Icon => _icon;
        public GameObject Actor => _actor;
        public IReadOnlyCollection<Ability> AppearSkills => _appearSkills;
        public IReadOnlyCollection<Ability> PassiveSkills => _passiveSkills;
        public IReadOnlyCollection<Ability> ActiveSkills => _activeSkills;


        [ContextMenu(nameof(NameToID))]
        private void NameToID()
        {
            _id = name;
        }
    }
}
