namespace PF.PJT.Duet
{
    public interface IEquipment
    {
        public bool IsEquipped { get; } 
        public IEquipmentWearer EquipmentWearer { get; }
        public bool CanEquipped(IEquipmentWearer wearer);
        public bool CanUnequipped();
        public void Equip(IEquipmentWearer wearer);
        public void Unequip();

        public string ToString();
    }
}
