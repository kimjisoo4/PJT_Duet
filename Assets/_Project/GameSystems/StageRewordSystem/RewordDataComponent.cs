using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordDataComponent : BaseMonoBehaviour
    {
        [Header(" [ Reword Data Component ] ")]
        [SerializeField][SReadOnly] private ItemSO _itemData;
        [SerializeField] private DataUpdater[] _updaters;
        public ItemSO ItemData => _itemData;

        public void SetData(ItemSO reword)
        {
            _itemData = reword;

            foreach (var modifier in _updaters)
            {
                modifier.UpdateData(reword);
            }
        }
    }
}
