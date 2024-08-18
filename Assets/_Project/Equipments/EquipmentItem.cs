using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public abstract class EquipmentItem : BaseScriptableObject, IItem, IDisplayItem
    {
        [Header(" [ Equipment Item Data ] ")]
        [SerializeField] private string _id = "EquipmentItemData_";

        public string ID => _id;
        public abstract Sprite Icon { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }


        [ContextMenu(nameof(NameToID))]
        private void NameToID()
        {
            _id = name;
        }

        public EquipmentItemSpec CreateSpec()
        {
            Log(nameof(CreateSpec));

            return GetSpec();
        }

        protected abstract EquipmentItemSpec GetSpec();
    }
}
