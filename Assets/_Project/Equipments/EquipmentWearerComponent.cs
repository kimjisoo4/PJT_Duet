using StudioScor.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class EquipmentWearerComponent : BaseMonoBehaviour, IEquipmentWearer
    {
        [Header(" [ Equipment Wearer Component ] ")]

        private readonly List<IEquipment> _equipments = new();
        public IReadOnlyList<IEquipment> Equipments => _equipments;

        public event IEquipmentWearer.EquipmentEventHandler OnEquipped;
        public event IEquipmentWearer.EquipmentEventHandler OnUnequipped;

        public virtual bool CanEquiping(IEquipment equipment)
        {
            if (equipment is null)
                return false;

            if (_equipments.Contains(equipment))
                return false;

            return true;
        }

        public virtual bool CanUnequiping(IEquipment equipment)
        {
            if (equipment is null)
                return false;

            if (!_equipments.Contains(equipment))
                return false;

            return true;
        }

        protected virtual void OnEquip(IEquipment equipment) { }
        protected virtual void OnUnequip(IEquipment equipment) { }

        public void Equip(IEquipment equipment)
        {
            if (equipment is null)
                return;

            Log($"{nameof(Equip)} - {equipment}");

            _equipments.Add(equipment);

            OnEquip(equipment);

            Inovke_OnEquipped(equipment);
        }

        public void Unequip(IEquipment equipment)
        {
            if (equipment is null)
                return;

            Log($"{nameof(Unequip)} - {equipment}");

            _equipments.Remove(equipment);

            OnUnequip(equipment);

            Inovke_OnUnequipped(equipment);
        }

        private void Inovke_OnEquipped(IEquipment equipment)
        {
            Log($"{nameof(OnEquipped)} - {equipment}");

            OnEquipped?.Invoke(this, equipment);
        }
        private void Inovke_OnUnequipped(IEquipment equipment)
        {
            Log($"{nameof(OnUnequipped)} - {equipment}");

            OnUnequipped?.Invoke(this, equipment);
        }
    }
}
