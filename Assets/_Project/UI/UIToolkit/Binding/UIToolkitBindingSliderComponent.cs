using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{
    public class UIToolkitBindingSliderComponent : BindingComponent<UIDocument>
    {
        [Header(" [ UI Toolkit Binding Slider ] ")]
        [SerializeField] private string _sliderName = "Slider_";
        [SerializeField] private SOFloatVariable _floatVariable;

        private Slider _slider;

        private void OnEnable()
        {
            _slider = Component.rootVisualElement.Q<Slider>(_sliderName);
            _slider.value = _floatVariable.Value;

            _floatVariable.OnValueChanged += FloatVariable_OnValueChanged;
            _slider.RegisterValueChangedCallback(OnValueChanged);
        }
        private void OnDisable()
        {
            _floatVariable.OnValueChanged -= FloatVariable_OnValueChanged;
            _slider?.UnregisterValueChangedCallback(OnValueChanged);

            _slider = null;
        }

        private void OnValueChanged(ChangeEvent<float> evt)
        {
            _floatVariable.Value = evt.newValue;
        }
        private void FloatVariable_OnValueChanged(SOVariable<float> variable, float currentValue, float prevValue)
        {
            _slider.value = currentValue;
        }
    }
}