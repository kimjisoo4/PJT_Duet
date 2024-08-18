using PF.PJT.Duet.Controller;
using PF.PJT.Duet.Pawn;
using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using StudioScor.PlayerSystem;
using StudioScor.StatusSystem;
using StudioScor.Utilities;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet.System
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

        private IAbilitySystem _currentAbilitySystem;

        private IAbilitySystem _mainAbilitySystem;
        private IAbilitySystem _subAbilitySystem;

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
                _playerController.OnChangedCurrentCharacter -= _playerController_OnChangedCurrentCharacter;
            }

            if (_mainAbilitySystem is not null)
            {
                _mainAbilitySystem.OnGrantedAbility -= _abilitySystem_OnGrantedAbility;
                _mainAbilitySystem.OnRemovedAbility -= _abilitySystem_OnRemovedAbility;
            }

            if (_subAbilitySystem is not null)
            {
                _subAbilitySystem.OnGrantedAbility -= _abilitySystem_OnGrantedAbility;
                _subAbilitySystem.OnRemovedAbility -= _abilitySystem_OnRemovedAbility;
            }
        }


        private void _playerManager_OnChangedPlayerController(PlayerManager playerManager, IControllerSystem currentController, IControllerSystem prevController = null)
        {
            SetPlayerController();
        }

        private void SetPlayerController()
        {
            _playerController = _playerManager.PlayerController.gameObject.GetComponent<IPlayerController>();

            _mainAbilitySystem = _playerController.MainCharacter.gameObject.GetAbilitySystem();
            _subAbilitySystem = _playerController.SubCharacter.gameObject.GetAbilitySystem();

            _mainAbilitySystem.OnGrantedAbility += _abilitySystem_OnGrantedAbility;
            _mainAbilitySystem.OnRemovedAbility += _abilitySystem_OnRemovedAbility;
            _subAbilitySystem.OnGrantedAbility += _abilitySystem_OnGrantedAbility;
            _subAbilitySystem.OnRemovedAbility += _abilitySystem_OnRemovedAbility;

            UpdateCurrentCharacter();

            _playerController.OnChangedCurrentCharacter += _playerController_OnChangedCurrentCharacter;
        }
       

        private void _playerController_OnChangedCurrentCharacter(IPlayerController playerController)
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
            IAbilitySystem targetAbilitySystem;

            if (_playerController.IsPlayingMainCharacter)
            {
                targetAbilitySystem = _mainAbilitySystem;
            }
            else
            {
                targetAbilitySystem = _subAbilitySystem;
            }

            if (_currentAbilitySystem == targetAbilitySystem)
                return;

            ClearSlots();

            _currentAbilitySystem = targetAbilitySystem;

            UpdateSlotUIs();
        }

        private void UpdateSlotUIs()
        {
            for (int i = 0; i < _currentAbilitySystem.Abilities.Count(); i++)
            {
                var ability = _currentAbilitySystem.Abilities.ElementAt(i);

                SetSlot(ability.Key, ability.Value);
            }

            var tagCharacter = _playerController.IsPlayingMainCharacter ? _playerController.SubCharacter : _playerController.MainCharacter;

            _tagCharacterSlot.SetCharacter(tagCharacter);
            _tagCharacterStatusAmount.SetTarget(tagCharacter.gameObject);

            var tagAbilitySystem = _playerController.IsPlayingMainCharacter ? _subAbilitySystem : _mainAbilitySystem;

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
            if (abilitySystem != _currentAbilitySystem)
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
            if (abilitySystem != _currentAbilitySystem)
                return;

            SetSlot(abilitySpec.Ability, abilitySpec);
        }
    }
}
