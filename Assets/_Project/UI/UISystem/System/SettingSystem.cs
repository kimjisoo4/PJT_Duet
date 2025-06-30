using StudioScor.Utilities;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace PF.PJT.Duet.UISystem
{
    public class SettingSystem : BaseMonoBehaviour
    {
        [Header(" [ Setting System ] ")]
        [Header(" Gameplay Setting ")]
        [SerializeField] private SOStringVariable _language;
        [SerializeField] private SOBoolVariable _viewDevLog;

        [Header(" Sound Setting ")]
        [Header(" Volume ")]
        [SerializeField] private SOFloatVariable _masterVolume;
        [SerializeField] private SOFloatVariable _bgmVolume;
        [SerializeField] private SOFloatVariable _sfxVolume;
        [Header(" Mute ")]
        [SerializeField] private SOBoolVariable _masterMute;
        [SerializeField] private SOBoolVariable _bgmMute;
        [SerializeField] private SOBoolVariable _sfxMute;

        private bool _isValueChanged;
        public bool IsValueChanged => _isValueChanged;

        private void Awake()
        {
            var locale = LocalizationSettings.AvailableLocales.Locales.Find(x => x.Identifier.CultureInfo.Name == _language.Value);
            LocalizationSettings.SelectedLocale = locale;

            _language.OnValueChanged += OnLanguageChanged;
            _viewDevLog.OnValueChanged += (variable, current, prev) => OnValueChanged();
            _masterVolume.OnValueChanged += (variable, current, prev) => OnValueChanged();
            _bgmVolume.OnValueChanged += (variable, current, prev) => OnValueChanged();
            _sfxVolume.OnValueChanged += (variable, current, prev) => OnValueChanged();
            _masterMute.OnValueChanged += (variable, current, prev) => OnValueChanged();
            _bgmMute.OnValueChanged += (variable, current, prev) => OnValueChanged();
            _sfxMute.OnValueChanged += (variable, current, prev) => OnValueChanged();

        }

        private void OnDestroy()
        {
            _language.OnValueChanged -= OnLanguageChanged;
            _viewDevLog.OnValueChanged -= (variable, current, prev) => OnValueChanged();
            _masterVolume.OnValueChanged -= (variable, current, prev) => OnValueChanged();
            _bgmVolume.OnValueChanged -= (variable, current, prev) => OnValueChanged();
            _sfxVolume.OnValueChanged -= (variable, current, prev) => OnValueChanged();
            _masterMute.OnValueChanged -= (variable, current, prev) => OnValueChanged();
            _bgmMute.OnValueChanged -= (variable, current, prev) => OnValueChanged();
            _sfxMute.OnValueChanged -= (variable, current, prev) => OnValueChanged();
        }
        private void OnLanguageChanged(SOVariable<string> variable, string currentValue, string prevValue)
        {
            _isValueChanged = true;

            var locale = LocalizationSettings.AvailableLocales.Locales.Find(x => x.Identifier.CultureInfo.Name == _language.Value);
            LocalizationSettings.SelectedLocale = locale;
        }

        private void OnValueChanged()
        {
            if (_isValueChanged)
                return;

            _isValueChanged = true;
        }


        public void Save()
        {
            _language.Save();
            _viewDevLog.Save();

            _masterVolume.Save();
            _bgmVolume.Save();
            _sfxVolume.Save();

            _masterMute.Save();
            _bgmMute.Save();
            _sfxMute.Save();

            _isValueChanged = false;
        }

        public void Load()
        {
            _language.Load();
            _viewDevLog.Load();

            _masterVolume.Load();
            _bgmVolume.Load();
            _sfxVolume.Load();

            _masterMute.Load();
            _bgmMute.Load();
            _sfxMute.Load();

            _isValueChanged = false;
        }

        public void ResetSetting()
        {
            _language.ResetValue();
            _viewDevLog.ResetValue();

            _masterVolume.ResetValue();
            _bgmVolume.ResetValue();
            _sfxVolume.ResetValue();

            _masterMute.ResetValue();
            _bgmMute.ResetValue();
            _sfxMute.ResetValue();
        }
    }
}
