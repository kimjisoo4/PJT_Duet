using TMPro;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordDescriptionUI : RewordDataUIModifier
    {
        [Header(" [ Reword Information UI ] ")]
        [SerializeField] public TMP_Text _description;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if(!_description)
            {
                _description = GetComponentInChildren<TMP_Text>();
            }
#endif
        }

        public override void UpdateData(EquipmentItem reword)
        {
            if (reword)
            {
                _description.text = reword.Description;
            }
            else
            {
                _description.text = "";
            }
        }
    }
}
