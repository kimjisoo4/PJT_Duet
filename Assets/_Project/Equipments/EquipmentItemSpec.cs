using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public interface IEquipment
    {
        public Object Context { get; }
        public bool UseDebug { get; }
    }

    public class EquipmentItemSpec : BaseClass, IEquipmentItemSpec 
    {
        protected readonly IEquipment _equipmentItem;
        private IEquipmentWearer _equipmentWearer;
        private bool _isEquipped;
        public bool IsEquipped => _isEquipped;
        public IEquipmentWearer EquipmentWearer => _equipmentWearer;

        public override Object Context => _equipmentItem.Context;
        public override bool UseDebug => _equipmentItem.UseDebug;

        public EquipmentItemSpec(IEquipment equipmentItemData)
        {
            _equipmentItem = equipmentItemData;
        }

        public override string ToString()
        {
            return _equipmentItem.ToString();
        }

        public virtual bool CanEquipped(IEquipmentWearer wearer)
        {
            if (_isEquipped)
                return false;

            return true;
        }

        public virtual bool CanUnequipped()
        {
            if (!_isEquipped)
                return false;

            return true;
        }

        public void Equip(IEquipmentWearer wearer)
        {
            if (_isEquipped)
                return;

            Log($"{nameof(Equip)} - {wearer}");

            _isEquipped = true;
            _equipmentWearer = wearer;

            OnEquip();
        }

        public void Unequip()
        {
            if (!_isEquipped)
                return;

            Log($"{nameof(Unequip)} - {_equipmentWearer}");

            _isEquipped = false;
            var prevEquipmentWearer = _equipmentWearer;
            _equipmentWearer = null;

            OnUnequip(prevEquipmentWearer);
        }

        protected virtual void OnEquip() { }
        protected virtual void OnUnequip(IEquipmentWearer equipmentWearer) { }
    }
}
