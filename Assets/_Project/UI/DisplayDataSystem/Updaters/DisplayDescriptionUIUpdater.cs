using TMPro;
using UnityEngine;

namespace PF.PJT.Duet
{

    public class DisplayDescriptionUIUpdater : DataUpdater
    {
        [Header(" [ Display Description UI Updater ] ")]
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
            if(data is not null && data is IDisplayDescription displayDescription)
            {
                _text.text = displayDescription.Description;
            }
            else
            {
                _text.text = "";
            }
        }
    }
}
