using UnityEngine;
using UnityEngine.UI;

namespace PF.PJT.Duet
{
    public class RewordIconUI : RewordDataUIModifier
    {
        [Header(" [ Reword Display UI ] ")]
        [SerializeField] private Image _icon;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!_icon)
            {
                _icon = GetComponentInChildren<Image>();
            }
#endif
        }

        public override void UpdateData(EquipmentItem reword)
        {
            if (reword)
            {
                _icon.sprite = reword.Icon;
            }
            else
            {
                _icon.sprite = null;
            }
        }
    }
}
