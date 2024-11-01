using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{

    public abstract class EquipmentSO : BaseScriptableObject, IEquipment
    {
        [Header(" [ Equipment Item Data ] ")]
        [SerializeField] private string _id = "Equipment_";
        public string ID => _id;

        public Object Context => this;
        bool IEquipment.UseDebug => UseDebug;


        [ContextMenu(nameof(NameToID))]
        private void NameToID()
        {
            _id = name;
        }
        public IEquipmentItemSpec CreateSpec()
        {
            Log(nameof(CreateSpec));

            return GetSpec();
        }

        protected abstract IEquipmentItemSpec GetSpec();
    }
}
