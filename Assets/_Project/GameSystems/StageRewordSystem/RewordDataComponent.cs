using StudioScor.Utilities;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordDataComponent : BaseMonoBehaviour
    {
        [Header(" [ Reword Data Component ] ")]
        [SerializeField][SReadOnly] private EquipmentItem _equipmentItem;
        [SerializeField] private RewordDataUIModifier[] _modifires;
        public EquipmentItem EquipmentItem => _equipmentItem;

        public void SetData(EquipmentItem reword)
        {
            _equipmentItem = reword;

            foreach (var modifier in _modifires)
            {
                modifier.UpdateData(reword);
            }
        }
    }
}
