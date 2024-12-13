using UnityEngine;
using UnityEngine.UI;

namespace PF.PJT.Duet
{
    public class DescriptionImageUpdater : DataUpdater
    {
        [Header(" [ Description Image Updater ] ")]
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
            if (data is not null && data is IDisplayDescriptionImage displayDescription && displayDescription.DescriptionImage)
            {
                _image.sprite = displayDescription.DescriptionImage;
                _image.color = Color.white;
                _image.SetNativeSize();
            }
            else
            {
                _image.sprite = null;
                _image.color = Color.clear;
            }
        }
    }
}
