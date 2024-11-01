using TMPro;
using UnityEngine;

namespace PF.PJT.Duet
{
    public class DisplayNameUIUpdater : DataUpdater
    {
        [Header(" [ Display Name UI Updater ] ")]
        [SerializeField] private TMP_Text _text;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!_text)
                _text = GetComponentInChildren<TMP_Text>();
#endif
        }

        public override void OnUpdateData(object data)
        {
            if(data is not null && data is IDisplayName displayName)
            {
                _text.text = displayName.Name;
            }
            else
            {
                _text.text = "";
            }
        }
    }
}
