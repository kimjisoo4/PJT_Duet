using System.Collections.Generic;
using UnityEngine;

namespace PF.PJT.Duet
{
    public interface IEquipmentWearer
    {
        public delegate void EquipmentEventHandler(IEquipmentWearer equipmentWearer, IEquipment equipment);
        public GameObject gameObject { get; }
        public Transform transform { get; }
        
        public IReadOnlyList<IEquipment> Equipments { get; }

        public bool CanEquiping(IEquipment equipment);
        public bool CanUnequiping(IEquipment equipment);

        public void Equip(IEquipment equipment);
        public void Unequip(IEquipment equipment);

        public event EquipmentEventHandler OnEquipped;
        public event EquipmentEventHandler OnUnequipped;
    }
}
