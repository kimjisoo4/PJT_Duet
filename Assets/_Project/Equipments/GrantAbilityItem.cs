using PF.PJT.Duet.Controller;
using PF.PJT.Duet.Pawn.PawnSkill;
using StudioScor.AbilitySystem;
using UnityEngine;

namespace PF.PJT.Duet
{
    [CreateAssetMenu(menuName = "Project/Duet/EquipmentItem/new Grant Ability Item", fileName = "EquipmentItem_GrantAbility_")]
    public class GrantAbilityItem : EquipmentItem
    {
        [Header(" [ Grant Ability Item ] ")]
        [SerializeField] private Ability _ability;

        public override Sprite Icon => ((IRewordAbility)_ability).Icon;
        public override string Name => ((IRewordAbility)_ability).Name;
        public override string Description => ((IRewordAbility)_ability).Description;

        protected override EquipmentItemSpec GetSpec()
        {
            return new Spec(this);
        }

        public class Spec : EquipmentItemSpec
        {
            protected new readonly GrantAbilityItem _equipmentItem;

            private IPlayerController _playerController;
            private IAbilitySystem _mainCharacterAbilitySystem;
            private IAbilitySystem _subCharacterAbilitySystem;

            public Spec(EquipmentItem equipmentItemData) : base(equipmentItemData)
            {
                _equipmentItem = equipmentItemData as GrantAbilityItem;
            }

            protected override void OnEquip()
            {
                base.OnEquip();

                var owner = EquipmentWearer.gameObject;

                _playerController = owner.GetComponent<IPlayerController>();

                var mainCharacter = _playerController.CurrentCharacter.gameObject;
                var subCharacter = _playerController.NextCharacter.gameObject;

                if (mainCharacter.TryGetAbilitySystem(out _mainCharacterAbilitySystem))
                {
                    _mainCharacterAbilitySystem.TryGrantAbility(_equipmentItem._ability);
                }
                if (subCharacter.TryGetAbilitySystem(out _subCharacterAbilitySystem))
                {
                    _subCharacterAbilitySystem.TryGrantAbility(_equipmentItem._ability);
                }
            }
            protected override void OnUnequip(IEquipmentWearer equipmentWearer)
            {
                base.OnUnequip(equipmentWearer);


                if(_mainCharacterAbilitySystem is not null)
                {
                    _mainCharacterAbilitySystem.TryRemoveAbility(_equipmentItem._ability);
                }

                if(_subCharacterAbilitySystem is not null)
                {
                    _subCharacterAbilitySystem.TryRemoveAbility(_equipmentItem._ability);
                }
            }
        }
    }
}
