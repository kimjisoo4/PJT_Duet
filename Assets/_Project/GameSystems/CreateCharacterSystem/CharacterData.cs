using PF.PJT.Duet.Pawn;
using StudioScor.AbilitySystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet.CreateCharacterSystem
{
    public class CharacterData : IDisplayName, IDisplayDescription, IDisplayIcon
    {
        private readonly CharacterInformationData _characterInformationData;

        private readonly Ability _appearSkill;
        private readonly Ability _activeSkill_01;

        public string ID => _characterInformationData.ID;
        public PoolContainer ActorPool => _characterInformationData.Pool;
        public Sprite Icon => _characterInformationData.Icon;
        public string Name => _characterInformationData.Name;
        public string Description => _characterInformationData.Description;
        public Ability AppearSkill => _appearSkill;
        public Ability Skill_01 => _activeSkill_01;
        public IReadOnlyCollection<Ability> DefaultSkills => _characterInformationData.DefaultSkills;

        public CharacterData(CharacterInformationData characterInformationData)
        {
            _characterInformationData = characterInformationData;

            _appearSkill = characterInformationData.AppearSkills.RandomElement();
            _activeSkill_01 = characterInformationData.ActiveSkills.RandomElement();
        }

        public ICharacter CreateCharacter()
        {
            var characterActor = _characterInformationData.Pool.Get();
            characterActor.transform.SetParent(null);

            var character = characterActor.GetComponent<ICharacter>();
            character.ResetCharacter();

            character.GrantSkill(ESkillSlot.Appear, AppearSkill);
            character.GrantSkill(ESkillSlot.Skill_01, Skill_01);

            foreach (var ability in DefaultSkills)
            {
                character.GrantSkill(ESkillSlot.None, ability);
            }

            return character;
        }
    }
}
