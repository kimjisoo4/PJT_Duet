using PF.PJT.Duet.Controller;
using PF.PJT.Duet.Pawn;
using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    
    [CreateAssetMenu(menuName = "Project/Duet/EquipmentItem/new Grant Ability Item", fileName = "Item_GrantAbility_")]
    public class GrantAbilityItem : EquipmentItemSO
    {
        [Header(" [ Grant Ability Item ] ")]
        [SerializeField] private Ability _ability;

        public override Sprite Icon => ((IRewordAbility)_ability).Icon;
        public override string Name => ((IRewordAbility)_ability).Name;
        public override string Description => ((IRewordAbility)_ability).Description;

        protected override IEquipmentItemSpec GetSpec()
        {
            return new Spec(this);
        }

        public class Spec : EquipmentItemSpec
        {
            protected new readonly GrantAbilityItem _equipmentItem;

            private IPlayerController _playerController;
            private readonly Dictionary<ICharacter, IAbilitySystem> _abilitySystems = new();

            public Spec(IEquipment equipmentItemData) : base(equipmentItemData)
            {
                _equipmentItem = equipmentItemData as GrantAbilityItem;
            }

            protected override void OnEquip()
            {
                base.OnEquip();

                var owner = EquipmentWearer.gameObject;

                _playerController = owner.GetComponent<IPlayerController>();

                for (int i = 0; i < _playerController.Characters.Count; i++)
                {
                    var character = _playerController.Characters[i];

                    if (character.gameObject.TryGetAbilitySystem(out IAbilitySystem abilitySystem))
                    {
                        abilitySystem.TryGrantAbility(_equipmentItem._ability);
                        _abilitySystems.Add(character, abilitySystem);
                    }
                }

                _playerController.OnAddedCharacter += _playerController_OnAddedCharacter;
                _playerController.OnRemovedCharacter += _playerController_OnRemovedCharacter;
            }
            protected override void OnUnequip(IEquipmentWearer equipmentWearer)
            {
                base.OnUnequip(equipmentWearer);

                if(_abilitySystems is not null)
                {
                    foreach (var abilitySystem in _abilitySystems.Values)
                    {
                        if (abilitySystem is null)
                            continue;

                        abilitySystem.RemoveAbility(_equipmentItem._ability);
                    }

                    _abilitySystems.Clear();
                }
                

                if (_playerController is not null)
                {
                    _playerController.OnAddedCharacter -= _playerController_OnAddedCharacter;
                    _playerController.OnRemovedCharacter -= _playerController_OnRemovedCharacter;
                    
                    _playerController = null;
                }
            }
            private void _playerController_OnAddedCharacter(IPlayerController controller, Pawn.ICharacter character)
            {
                if (character.gameObject.TryGetAbilitySystem(out IAbilitySystem abilitySystem))
                {
                    abilitySystem.TryGrantAbility(_equipmentItem._ability);

                    _abilitySystems.Add(character, abilitySystem);
                }
            }

            private void _playerController_OnRemovedCharacter(IPlayerController controller, Pawn.ICharacter character)
            {
                if(_abilitySystems.TryGetValue(character, out IAbilitySystem abilitySystem))
                {
                    abilitySystem.RemoveAbility(_equipmentItem._ability);
                    _abilitySystems.Remove(character);
                }
            }
        }
    }
}
