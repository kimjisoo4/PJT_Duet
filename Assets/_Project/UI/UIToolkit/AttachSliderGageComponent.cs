using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{
    public class AttachSliderGageComponent : BaseMonoBehaviour
    {
        [Header(" [ Attach Slider Gage Component ] ")]
        [SerializeField] private UIDocument _uiDocument;
        [SerializeField] private string _targetSliderName = "Slider";

        [SerializeField] private Color _color = Color.blue;
        [SerializeField][SUnit("%")] private float _height = 100f;

        private const string SLIDER_TRACKER = "unity-tracker";
        private Slider _targetSlider;
        private VisualElement _bar;

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!SUtility.IsPlayingOrWillChangePlaymode)
            {
                if (!_uiDocument)
                {
                    _uiDocument = GetComponent<UIDocument>();
                }
            }
            else
            {
                if (_targetSlider is not null && _bar is not null)
                {
                    if (_bar.style.backgroundColor != _color)
                        _bar.style.backgroundColor = _color;

                    if (_bar.style.height != _height)
                        _bar.style.height = Length.Percent(_height);
                }
            }
#endif
        }

        private void Awake()
        {
            var root = _uiDocument.rootVisualElement;

            _targetSlider = root.Q<Slider>(_targetSliderName);

            var barTransform = _targetSlider.Q<VisualElement>(SLIDER_TRACKER);
            _bar = new VisualElement();
            _bar.style.width = Length.Percent(_targetSlider.value);
            _bar.style.height = Length.Percent(_height);
            _bar.style.backgroundColor = _color;
            _bar.style.position = Position.Absolute;
            barTransform.Add(_bar);

            _targetSlider.RegisterValueChangedCallback(OnSliderValueChanged);
        }
        private void OnDestroy()
        {
            if(_targetSlider is not null)
            {
                _targetSlider.UnregisterValueChangedCallback(OnSliderValueChanged);
            }
        }

        public void SetColor(Color newColor)
        {
            _bar.style.backgroundColor = _color;
        }

        private void OnSliderValueChanged(ChangeEvent<float> changeValue)
        {
            _bar.style.width = new StyleLength(Length.Percent(_targetSlider.value));
        }
    }
}
