using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class EquipmentWearerComponent : BaseMonoBehaviour, IEquipmentWearer
    {
        [Header(" [ Equipment Wearer Component ] ")]

        private readonly List<IEquipmentItemSpec> _equipments = new();
        public IReadOnlyList<IEquipmentItemSpec> Equipments => _equipments;

        public event IEquipmentWearer.EquipmentEventHandler OnEquipped;
        public event IEquipmentWearer.EquipmentEventHandler OnUnequipped;

        public virtual bool CanEquiping(IEquipmentItemSpec equipment)
        {
            if (equipment is null)
                return false;

            if (_equipments.Contains(equipment))
                return false;

            return true;
        }

        public virtual bool CanUnequiping(IEquipmentItemSpec equipment)
        {
            if (equipment is null)
                return false;

            if (!_equipments.Contains(equipment))
                return false;

            return true;
        }

        protected virtual void OnEquip(IEquipmentItemSpec equipment) { }
        protected virtual void OnUnequip(IEquipmentItemSpec equipment) { }

        public void Equip(IEquipmentItemSpec equipment)
        {
            if (equipment is null)
                return;

            Log($"{nameof(Equip)} - {equipment}");

            _equipments.Add(equipment);

            OnEquip(equipment);

            Inovke_OnEquipped(equipment);
        }

        public void Unequip(IEquipmentItemSpec equipment)
        {
            if (equipment is null)
                return;

            Log($"{nameof(Unequip)} - {equipment}");

            _equipments.Remove(equipment);

            OnUnequip(equipment);

            Inovke_OnUnequipped(equipment);
        }

        private void Inovke_OnEquipped(IEquipmentItemSpec equipment)
        {
            Log($"{nameof(OnEquipped)} - {equipment}");

            OnEquipped?.Invoke(this, equipment);
        }
        private void Inovke_OnUnequipped(IEquipmentItemSpec equipment)
        {
            Log($"{nameof(OnUnequipped)} - {equipment}");

            OnUnequipped?.Invoke(this, equipment);
        }
    }
}
