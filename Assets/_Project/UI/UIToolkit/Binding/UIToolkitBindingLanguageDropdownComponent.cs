using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace PF.PJT.Duet.UISystem
{
    public class UIToolkitBindingLanguageDropdownComponent : BindingComponent<UIDocument>
    {
        [Header(" [ UI Toolkit Binding Toggle ] ")]
        [SerializeField] private string _dropdownName = "Dropdown_";
        [SerializeField] private SOStringVariable _laguageVariable;

        private readonly List<Locale> _locales = new();
        private DropdownField _dropdownField;

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            UpdateField();
        }

        private void OnEnable()
        {
            _dropdownField = Component.rootVisualElement.Q<DropdownField>(_dropdownName);

            if(didStart)
                UpdateField();

            _laguageVariable.OnValueChanged += LaguageVariable_OnValueChanged;
            _dropdownField.RegisterValueChangedCallback(OnDropdownValueChanged);
        }

        private void OnDisable()
        {
            _laguageVariable.OnValueChanged -= LaguageVariable_OnValueChanged;
            _dropdownField?.UnregisterValueChangedCallback(OnDropdownValueChanged);

            _dropdownField = null;
        }

        private void UpdateField()
        {
            Log(nameof(UpdateField));

            _locales.Clear();
            _locales.AddRange(LocalizationSettings.AvailableLocales.Locales);

            var options = _locales.ConvertAll(x => x.Identifier.CultureInfo.NativeName);

            _dropdownField.choices.Clear();
            _dropdownField.choices.AddRange(options);

            int index = _locales.FindIndex(x => x.Identifier.CultureInfo.Name == _laguageVariable.Value);
            _dropdownField.index = index;
        }

        private void LaguageVariable_OnValueChanged(SOVariable<string> variable, string currentValue, string prevValue)
        {
            int index = _locales.FindIndex(x => x.Identifier.CultureInfo.Name == currentValue);
            
            _dropdownField.index = index;
        }
        private void OnDropdownValueChanged(ChangeEvent<string> evt)
        {
            var locale = _locales.Find(x => x.Identifier.CultureInfo.NativeName == evt.newValue);

            if (locale is not null)
            {
                _laguageVariable.Value = locale.Identifier.CultureInfo.Name;
            }
        }
    }
}