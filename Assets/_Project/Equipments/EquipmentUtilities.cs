using StudioScor.Utilities;
using System.Linq;
using UnityEngine;

namespace PF.PJT.Duet
{
    public static class EquipmentUtilities
    {
        public static IEquipmentWearer GetEquipmnentWearer(this GameObject gameObject)
        {
            return gameObject.GetComponent<IEquipmentWearer>();
        }
        public static IEquipmentWearer GetEquipmnentWearer(this Component component)
        {
            return component.gameObject.GetComponent<IEquipmentWearer>();
        }

        public static bool HasEquipmnentWearer(this GameObject gameObject)
        {
            return gameObject.TryGetComponent(out IEquipmentWearer _);
        }
        public static bool HasEquipmnentWearer(this Component component)
        {
            return component.gameObject.TryGetComponent(out IEquipmentWearer _);
        }

        public static bool TryGetEquipmnentWearer(this GameObject gameObject, out IEquipmentWearer equipmentWearer)
        {
            return gameObject.TryGetComponent(out equipmentWearer);
        }
        public static bool TryGetEquipmnentWearer(this Component component, out IEquipmentWearer equipmentWearer)
        {
            return component.gameObject.TryGetComponent(out equipmentWearer);
        }


        public static bool TryEquipItem(this IEquipmentWearer equipmentWearer, IEquipmentItemSpec equipment)
        {
            if (!equipmentWearer.CanEquiping(equipment))
                return false;

            if (!equipment.CanEquipped(equipmentWearer))
                return false;

            equipmentWearer.Equip(equipment);
            equipment.Equip(equipmentWearer);

            return true;
        }
        public static bool TryUnequipItem(this IEquipmentWearer equipmentWearer, IEquipmentItemSpec equipment)
        {
            if (!equipmentWearer.CanUnequiping(equipment))
                return false;

            if (!equipment.CanUnequipped())
                return false;

            equipment.Unequip();
            equipmentWearer.Unequip(equipment);

            return true;
        }

        public static bool Contains(this IEquipmentWearer equipmentWearer, IEquipmentItemSpec equipment)
        {
            if (equipmentWearer is null)
                return false;

            if (equipment is null)
                return false;

            return equipmentWearer.Equipments.Contains(equipment);
        }

        public static void Clear(this IEquipmentWearer equipmentWearer)
        {
            if (equipmentWearer is null)
                return;

            var equipments = equipmentWearer.Equipments;

            if (equipments is null || equipments.Count == 0)
                return;

            for (int i = equipments.LastIndex(); i >= 0; i--)
            {
                var item = equipments.ElementAtOrDefault(i);

                equipmentWearer.Unequip(item);
            }
        }
    }
}
