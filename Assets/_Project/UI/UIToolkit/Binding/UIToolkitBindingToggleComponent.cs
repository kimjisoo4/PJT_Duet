using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

namespace PF.PJT.Duet.UISystem
{
    public class UIToolkitBindingToggleComponent : BindingComponent<UIDocument>
    {
        [Header(" [ UI Toolkit Binding Toggle ] ")]
        [SerializeField] private string _toggleName = "Toggle_";
        [SerializeField] private SOBoolVariable _boolVariable;

        private Toggle _toggle;

        private void OnEnable()
        {
            _toggle = Component.rootVisualElement.Q<Toggle>(_toggleName);
            _toggle.value = _boolVariable.Value;

            _boolVariable.OnValueChanged += BoolVariable_OnValueChanged;
            _toggle.RegisterValueChangedCallback(OnValueChanged);
        }
        private void OnDisable()
        {
            _boolVariable.OnValueChanged -= BoolVariable_OnValueChanged;
            _toggle?.UnregisterValueChangedCallback(OnValueChanged);

            _toggle = null;
        }

        private void OnValueChanged(ChangeEvent<bool> evt)
        {
            _boolVariable.Value = evt.newValue;
        }
        private void BoolVariable_OnValueChanged(SOVariable<bool> variable, bool currentValue, bool prevValue)
        {
            _toggle.value = currentValue;
        }
    }
}