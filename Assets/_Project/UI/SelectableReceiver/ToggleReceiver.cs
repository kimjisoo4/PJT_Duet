using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace PF.PJT.Duet.UISystem
{
    public class ToggleReceiver : BaseMonoBehaviour
    {
        [Header(" [ Toggle Receiver ] ")]
        [SerializeField] private Toggle _toggle;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (SUtility.IsPlayingOrWillChangePlaymode)
                return;

            if(!_toggle)
            {
                _toggle = GetComponentInChildren<Toggle>();
            }
#endif
        }

        public void Toggle()
        {
            _toggle.isOn = !_toggle.isOn;
        }

        public void SetToggle(bool isOn)
        {
            _toggle.isOn = isOn;
        }
    }
}
