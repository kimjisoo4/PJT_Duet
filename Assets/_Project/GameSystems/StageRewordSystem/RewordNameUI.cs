using TMPro;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class RewordNameUI : RewordDataUIModifier
    {
        [Header(" [ Reword Name UI ] ")]
        [SerializeField] public TMP_Text _name;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!_name)
            {
                _name = GetComponentInChildren<TMP_Text>();
            }
#endif
        }

        public override void UpdateData(EquipmentItem reword)
        {
            if (reword)
            {
                _name.text = reword.Name;
            }
            else
            {
                _name.text = "";
            }
        }
    }
}
