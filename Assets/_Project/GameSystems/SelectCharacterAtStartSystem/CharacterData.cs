using PF.PJT.Duet.Pawn;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class CharacterData : IDisplayName, IDisplayDescription, IDisplayIcon
    {
        private string _id;
        private GameObject _actor;
        private Sprite _icon;
        private string _name;
        private string _description;
        private Ability _appearSkill;
        private Ability _passiveSkill;
        private Ability _activeSkill_01;

        public CharacterData(CharacterInformationData characterInformationData)
        {
            _id = characterInformationData.ID;
            _actor = characterInformationData.Actor;
            _icon = characterInformationData.Icon;
            _name = characterInformationData.Name;
            _description = characterInformationData.Description;

            _appearSkill = characterInformationData.ActiveSkills.RandomElement();
            _passiveSkill = characterInformationData.PassiveSkills.RandomElement();
            _activeSkill_01 = characterInformationData.ActiveSkills.RandomElement();
        }

        public string ID => _id;
        public GameObject Actor => _actor;
        public Sprite Icon => _icon;
        public string Name => _name;
        public string Description => _description;
        public Ability AppearSkill => _appearSkill;
        public Ability PassiveSkill => _passiveSkill;
        public Ability ActiveSkill_01 => _activeSkill_01;
        public Ability Skill_01 => _activeSkill_01;
    }
}
