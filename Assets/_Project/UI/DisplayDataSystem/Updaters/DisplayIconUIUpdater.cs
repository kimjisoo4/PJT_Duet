using UnityEngine;
using UnityEngine.UI;

namespace PF.PJT.Duet
{
    public class DisplayIconUIUpdater : DataUpdater
    {
        [Header(" [ Display Icon UI Updater ] ")]
        [SerializeField] private Image _image;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!_image)
                _image = GetComponentInChildren<Image>();
#endif
        }

        public override void OnUpdateData(object data)
        {
            if(data is not null && data is IDisplayIcon displayIcon)
            {
                _image.sprite = displayIcon.Icon;
                _image.color = Color.white;
            }
            else
            {
                _image.sprite = null;
                _image.color = Color.clear;
            }
        }
    }
}
