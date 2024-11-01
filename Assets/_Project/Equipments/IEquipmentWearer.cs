using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    public interface IEquipmentWearer
    {
        public delegate void EquipmentEventHandler(IEquipmentWearer equipmentWearer, IEquipmentItemSpec equipment);
        public GameObject gameObject { get; }
        public Transform transform { get; }
        
        public IReadOnlyList<IEquipmentItemSpec> Equipments { get; }

        public bool CanEquiping(IEquipmentItemSpec equipment);
        public bool CanUnequiping(IEquipmentItemSpec equipment);

        public void Equip(IEquipmentItemSpec equipment);
        public void Unequip(IEquipmentItemSpec equipment);

        public event EquipmentEventHandler OnEquipped;
        public event EquipmentEventHandler OnUnequipped;
    }
}
