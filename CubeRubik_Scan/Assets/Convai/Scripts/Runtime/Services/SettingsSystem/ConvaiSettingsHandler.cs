using Convai.Scripts.SettingsPanelUI;
using UnityEngine;

namespace Convai.Scripts.Services.SettingsSystem
{
    public class ConvaiSettingsHandler : MonoBehaviour
    {
        [SerializeField] private ConvaiSettingsPanel convaiSettingsPanelPrefab;

        private ConvaiSettingsPanel _panel;

        private void Awake()
        {
            _panel = Instantiate(convaiSettingsPanelPrefab, transform);
            ConvaiServices.UISystem.OnSettingsOpened += ShowSettings;
            ConvaiServices.UISystem.OnSettingsClosed += HideSettings;
        }

        private void ShowSettings() => _panel.Show();
        private void HideSettings() => _panel.Hide();
    }
}
