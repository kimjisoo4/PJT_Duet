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
using UnityEngine.TextCore.Text;

namespace PF.PJT.Duet
{
    public class PlayerSkillStateSystem : BaseMonoBehaviour
    {
        [Header(" [ Player Skill System ] ")]
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private GameObject _playerSkillStateUIActor;
        [SerializeField] private InputSkillSlotUI _attackSlot;
        [SerializeField] private InputSkillSlotUI _skillSlot;

        [Header(" Tag Character ")]
        [SerializeField] private TagCharacterSlotUI _tagCharacterSlot;
        [SerializeField] private StatusUIToSimpleAmount _tagCharacterStatusAmount;
        [SerializeField] private InputSkillSlotUI _tagCharacterAppearSlot;

        [Header(" Event ")]
        [SerializeField] private GameObjectListVariable _inActiveVariable;

        private IPlayerController _playerController;

        private readonly Dictionary<ICharacter, IAbilitySystem> _abilitySystems = new();

        private void Awake()
        {
            if(_inActiveVariable)
            {
                _inActiveVariable.OnAdded += _inActiveVariable_OnAdded;
                _inActiveVariable.OnRemoved += _inActiveVariable_OnRemoved;
            }
        }
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

            if(!_abilitySystems.IsNullOrEmpty())
            {
                for (int i = 0; i < _abilitySystems.Keys.Count; i++)
                {
                    var character = _abilitySystems.Keys.ElementAtOrDefault(i);

                    if(character is not null)
                    {
                        character.OnChangedSkillSlot -= Character_OnChangedSkillSlot;
                    }
                }

                _abilitySystems.Clear();
            }


            if (_inActiveVariable)
            {
                _inActiveVariable.OnAdded -= _inActiveVariable_OnAdded;
                _inActiveVariable.OnRemoved -= _inActiveVariable_OnRemoved;
            }
        }
        private void _inActiveVariable_OnRemoved(ListVariableObject<GameObject> variable, GameObject value)
        {
            if (_inActiveVariable.Values.Count != 0)
                return;

            if (_playerSkillStateUIActor.activeSelf)
                return;

            _playerSkillStateUIActor.SetActive(true);
        }

        private void _inActiveVariable_OnAdded(ListVariableObject<GameObject> variable, GameObject value)
        {
            if (_inActiveVariable.Values.Count == 0)
                return;

            if (!_playerSkillStateUIActor.activeSelf)
                return;

            _playerSkillStateUIActor.SetActive(false);
        }



        private void SetPlayerController()
        {
            Log(nameof(SetPlayerController));

            _playerController = _playerManager.PlayerController.gameObject.GetComponent<IPlayerController>();

            if (!_playerController.Characters.IsNullOrEmpty())
            {
                for(int i = 0; i <  _playerController.Characters.Count; i++)
                {
                    var character = _playerController.Characters[i];

                    _abilitySystems.Add(character, character.gameObject.GetAbilitySystem());
                    character.OnChangedSkillSlot += Character_OnChangedSkillSlot;
                }
            }

            _playerController.OnAddedCharacter += _playerController_OnAddedCharacter;
            _playerController.OnRemovedCharacter += _playerController_OnRemovedCharacter;
            _playerController.OnChangedCurrentCharacter += _playerController_OnChangedCurrentCharacter;

            UpdateUI();
        }


        public void ClearSlots()
        {
            Log(nameof(ClearSlots));

            _attackSlot.SetSkill(null, null);
            _skillSlot.SetSkill(null, null);
            _tagCharacterAppearSlot.SetSkill(null, null);
            _tagCharacterSlot.SetCharacter(null);
        }

        private void UpdateUI()
        {
            Log(nameof(UpdateUI));

            ClearSlots();

            UpdateSlotUIs();
        }
        private void UpdateSlotUIs()
        {
            Log(nameof(UpdateSlotUIs));

            if(_playerController.CurrentCharacter is not null)
            {
                var character = _playerController.CurrentCharacter;
                var slotSkills = character.SlotSkills;
                var abilitySystem = _abilitySystems[character];

                for(int i = 0; i < slotSkills.Count; i++)
                {
                    var slotSkill = slotSkills.ElementAt(i);

                    UpdateCurrentCharacterSlot(abilitySystem, slotSkill.Key, slotSkill.Value);
                }
            }

            if(_playerController.NextCharacter is not null)
            {
                var character = _playerController.NextCharacter;
                var slotSkills = character.SlotSkills;
                var abilitySystem = _abilitySystems[character];

                _tagCharacterSlot.SetCharacter(character);

                for (int i = 0; i < slotSkills.Count; i++)
                {
                    var slotSkill = slotSkills.ElementAt(i);

                    UpdateNextCharacterSlot(abilitySystem, slotSkill.Key, slotSkill.Value);
                }
            }
        }

        private void UpdateCurrentCharacterSlot(IAbilitySystem abilitySystem, ESkillSlot skillSlot, Ability skill)
        {
            Log($"{nameof(UpdateCurrentCharacterSlot)} - Character : {abilitySystem.gameObject} || Skill Slot : {skillSlot} || Skill : {skill}");
            switch (skillSlot)
            {
                case ESkillSlot.None:
                    break;
                case ESkillSlot.Attack:
                    _attackSlot.SetSkill(skill, abilitySystem.GetAbilitySpec(skill));
                    break;
                case ESkillSlot.Dash:
                    break;
                case ESkillSlot.Jump:
                    break;
                case ESkillSlot.Appear:
                    break;
                case ESkillSlot.Leave:
                    break;
                case ESkillSlot.Skill_01:
                    _skillSlot.SetSkill(skill, abilitySystem.GetAbilitySpec(skill));
                    break;
                default:
                    break;
            }
        }

        private void UpdateNextCharacterSlot(IAbilitySystem abilitySystem, ESkillSlot skillSlot, Ability skill)
        {
            Log($"{nameof(UpdateNextCharacterSlot)} - Character : {abilitySystem.gameObject} || Skill Slot : {skillSlot} || Skill : {skill}");

            switch (skillSlot)
            {
                case ESkillSlot.None:
                    break;
                case ESkillSlot.Attack:
                    break;
                case ESkillSlot.Dash:
                    break;
                case ESkillSlot.Jump:
                    break;
                case ESkillSlot.Appear:
                    _tagCharacterAppearSlot.SetSkill(skill, abilitySystem.GetAbilitySpec(skill));
                    break;
                case ESkillSlot.Leave:
                    break;
                case ESkillSlot.Skill_01:
                    break;
                default:
                    break;
            }
        }

        private void _playerManager_OnChangedPlayerController(PlayerManager playerManager, IControllerSystem currentController, IControllerSystem prevController = null)
        {
            SetPlayerController();
        }
        private void _playerController_OnAddedCharacter(IPlayerController controller, ICharacter character)
        {
            var abilitySystem = character.gameObject.GetAbilitySystem();

            _abilitySystems.Add(character, abilitySystem);

            UpdateUI();
        }
        private void _playerController_OnRemovedCharacter(IPlayerController controller, ICharacter character)
        {
            _abilitySystems.Remove(character);

            UpdateUI();
        }

        private void _playerController_OnChangedCurrentCharacter(IPlayerController controller, ICharacter currentCharacter, ICharacter prevCharacter)
        {
            UpdateUI();
        }

        private void Character_OnChangedSkillSlot(ICharacter character, ESkillSlot skillSlot)
        {
            var abilitySystem = _abilitySystems[character];
            var slotSkill = character.SlotSkills.GetValueOrDefault(skillSlot);

            if (character == _playerController.CurrentCharacter)
            {
                UpdateCurrentCharacterSlot(abilitySystem, skillSlot, slotSkill);
            }
            else if (character == _playerController.NextCharacter)
            {
                UpdateNextCharacterSlot(abilitySystem, skillSlot, slotSkill);
            }
        }
    }
}
