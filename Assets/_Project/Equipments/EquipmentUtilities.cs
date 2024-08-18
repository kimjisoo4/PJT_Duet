using StudioScor.Utilities;
using System.Linq;

namespace PF.PJT.Duet
{
    public static class EquipmentUtilities
    {
        public static bool TryEquipItem(this IEquipmentWearer equipmentWearer, IEquipment equipment)
        {
            if (!equipmentWearer.CanEquiping(equipment))
                return false;

            if (!equipment.CanEquipped(equipmentWearer))
                return false;

            equipmentWearer.Equip(equipment);
            equipment.Equip(equipmentWearer);

            return true;
        }
        public static bool TryUnequipItem(this IEquipmentWearer equipmentWearer, IEquipment equipment)
        {
            if (!equipmentWearer.CanUnequiping(equipment))
                return false;

            if (!equipment.CanUnequipped())
                return false;

            equipment.Unequip();
            equipmentWearer.Unequip(equipment);

            return true;
        }

        public static bool Contains(this IEquipmentWearer equipmentWearer, IEquipment equipment)
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
