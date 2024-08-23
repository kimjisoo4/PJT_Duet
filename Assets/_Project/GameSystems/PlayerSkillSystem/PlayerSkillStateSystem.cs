using PF.PJT.Duet.Controller;
using PF.PJT.Duet.Pawn;
using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.PlayerSystem;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class PlayerSkillStateSystem : BaseMonoBehaviour
    {
        [Header(" [ Player Skill System ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private InputSkillSlotUI _attackSlot;
        [SerializeField] private InputSkillSlotUI _skillSlot;

        [Header(" Tag Character ")]
        [SerializeField] private TagCharacterSlotUI _tagCharacterSlot;
        [SerializeField] private StatusUIToSimpleAmount _tagCharacterStatusAmount;
        [SerializeField] private InputSkillSlotUI _tagCharacterAppearSlot;

        private IPlayerController _playerController;

        private readonly Dictionary<ICharacter, IAbilitySystem> _abilitySystems = new();

        private void Start()
        {
            if(_playerManager.HasPlayerController)
            {
                SetPlayerController();
            }

            _playerManager.OnChangedPlayerController += _playerManager_OnChangedPlayerController;
        }
        private void OnDestroy()
        {
            if (_playerManager)
            {
                _playerManager.OnChangedPlayerController -= _playerManager_OnChangedPlayerController;
            }

            if (_playerController is not null)
            {
                _playerController.OnAddedCharacter -= _playerController_OnAddedCharacter;
                _playerController.OnRemovedCharacter -= _playerController_OnRemovedCharacter;
                _playerController.OnChangedCurrentCharacter -= _playerController_OnChangedCurrentCharacter;

                _playerController = null;
            }


            foreach (var abilitySystem in _abilitySystems.Values)
            {
                if (abilitySystem is not null)
                {
                    abilitySystem.OnGrantedAbility -= _abilitySystem_OnGrantedAbility;
                    abilitySystem.OnRemovedAbility -= _abilitySystem_OnRemovedAbility;
                }
            }

            _abilitySystems.Clear();
        }


        private void _playerManager_OnChangedPlayerController(PlayerManager playerManager, IControllerSystem currentController, IControllerSystem prevController = null)
        {
            SetPlayerController();
        }

        private void SetPlayerController()
        {
            _playerController = _playerManager.PlayerController.gameObject.GetComponent<IPlayerController>();

            if (_playerController.CurrentCharacter is not null)
            {
                _abilitySystems.Add(_playerController.CurrentCharacter, _playerController.CurrentCharacter.gameObject.GetAbilitySystem());
            }

            if (_playerController.NextCharacter is not null)
            {
                _abilitySystems.Add(_playerController.NextCharacter, _playerController.NextCharacter.gameObject.GetAbilitySystem());
            }

            foreach (var abilitySystem in _abilitySystems.Values)
            {
                abilitySystem.OnGrantedAbility += _abilitySystem_OnGrantedAbility;
                abilitySystem.OnRemovedAbility += _abilitySystem_OnRemovedAbility;
            }

            UpdateCurrentCharacter();

            _playerController.OnAddedCharacter += _playerController_OnAddedCharacter;
            _playerController.OnRemovedCharacter += _playerController_OnRemovedCharacter;
            _playerController.OnChangedCurrentCharacter += _playerController_OnChangedCurrentCharacter;
        }

        private void _playerController_OnAddedCharacter(IPlayerController controller, ICharacter character)
        {
            var abilitySystem = character.gameObject.GetAbilitySystem();

            _abilitySystems.Add(character, abilitySystem);

            abilitySystem.OnGrantedAbility += _abilitySystem_OnGrantedAbility;
            abilitySystem.OnRemovedAbility += _abilitySystem_OnRemovedAbility;

            UpdateCurrentCharacter();
        }
        private void _playerController_OnRemovedCharacter(IPlayerController controller, ICharacter character)
        {
            if(_abilitySystems.TryGetValue(character, out IAbilitySystem abilitySystem))
            {
                _abilitySystems.Remove(character);

                abilitySystem.OnGrantedAbility -= _abilitySystem_OnGrantedAbility;
                abilitySystem.OnRemovedAbility -= _abilitySystem_OnRemovedAbility;
            }

            UpdateCurrentCharacter();
        }

        private void _playerController_OnChangedCurrentCharacter(IPlayerController controller, ICharacter currentCharacter, ICharacter prevCharacter)
        {
            UpdateCurrentCharacter();
        }

        public void ClearSlots()
        {
            _attackSlot.SetSkill(null, null);
            _skillSlot.SetSkill(null, null);
            _tagCharacterAppearSlot.SetSkill(null, null);
            _tagCharacterSlot.SetCharacter(null);
        }

        private void UpdateCurrentCharacter()
        {
            ClearSlots();

            UpdateSlotUIs();
        }

        private void UpdateSlotUIs()
        {
            var currentAbilitySystem = _abilitySystems[_playerController.CurrentCharacter];

            for (int i = 0; i < currentAbilitySystem.Abilities.Count(); i++)
            {
                var ability = currentAbilitySystem.Abilities.ElementAt(i);

                SetSlot(ability.Key, ability.Value);
            }

            var tagCharacter = _playerController.NextCharacter;

            _tagCharacterSlot.SetCharacter(tagCharacter);
            _tagCharacterStatusAmount.SetTarget(tagCharacter.gameObject);

            var tagAbilitySystem = _abilitySystems[_playerController.NextCharacter];

            for (int i = 0; i < tagAbilitySystem.Abilities.Count(); i++)
            {
                var ability = tagAbilitySystem.Abilities.ElementAt(i);

                if(ability.Key is ISkill skill && skill.SkillType == ESkillType.Appear)
                {
                    var skillState = ability.Value as ISkillState;

                    _tagCharacterAppearSlot.SetSkill(skill, skillState);
                }
            }
        }


        private void SetSlot(Ability ability, IAbilitySpec spec)
        {
            if (ability is ISkill skill)
            {
                var skillState = spec as ISkillState;

                switch (skill.SkillType)
                {
                    case ESkillType.None:
                        break;
                    case ESkillType.Attack:
                        _attackSlot.SetSkill(skill, skillState);
                        break;
                    case ESkillType.Skill:
                        _skillSlot.SetSkill(skill, skillState);
                        break;
                    case ESkillType.Appear:
                        break;
                    default:
                        break;
                }
            }
        }

        private void _abilitySystem_OnRemovedAbility(IAbilitySystem abilitySystem, IAbilitySpec abilitySpec)
        {
            if (_playerController is null || _playerController.CurrentCharacter is null || _abilitySystems is null)
                return;

            if (!_abilitySystems.TryGetValue(_playerController.CurrentCharacter, out IAbilitySystem currentAbilitySystem))
                return;

            if (abilitySystem != currentAbilitySystem)
                return;

            if (abilitySpec.Ability is not ISkill skill)
                return;

            switch (skill.SkillType)
            {
                case ESkillType.None:
                    break;
                case ESkillType.Attack:
                    _attackSlot.SetSkill(null, null);
                    break;
                case ESkillType.Skill:
                    _skillSlot.SetSkill(null, null);
                    break;
                case ESkillType.Appear:
                    break;
                default:
                    break;
            }
        }

        private void _abilitySystem_OnGrantedAbility(IAbilitySystem abilitySystem, IAbilitySpec abilitySpec)
        {
            if (_abilitySystems is null)
                return;

            if (!_abilitySystems.TryGetValue(_playerController.CurrentCharacter, out IAbilitySystem currentAbilitySystem))
                return;

            if (abilitySystem != currentAbilitySystem)
                return;

            SetSlot(abilitySpec.Ability, abilitySpec);
        }
    }
}
