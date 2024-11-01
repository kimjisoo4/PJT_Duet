using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public abstract class ItemSO : BaseScriptableObject, IItem
    {
        [Header(" [ Item Data ] ")]
        [SerializeField] private string _id = "ItemData_";

        public string ID => _id;

        [ContextMenu(nameof(NameToID))]
        private void NameToID()
        {
            _id = name;
        }
        public bool TryUseItem(GameObject actor)
        {
            if (!CanUseItem(actor))
                return false;

            UseItem(actor);

            return true;
        }
        public virtual bool CanUseItem(GameObject actor)
        {
            return true;
        }
        public abstract void UseItem(GameObject actor);
        
    }
}
