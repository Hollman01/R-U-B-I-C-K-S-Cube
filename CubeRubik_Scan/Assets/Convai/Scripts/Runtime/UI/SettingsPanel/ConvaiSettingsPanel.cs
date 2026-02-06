using System.Collections;
using System.Linq;
using Convai.Scripts.Configuration;
using Convai.Scripts.Services;
using Convai.Scripts.Services.TranscriptSystem;
using Convai.Scripts.TranscriptUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Convai.Scripts.SettingsPanelUI
{
    public class ConvaiSettingsPanel : MonoBehaviour
    {
        [Header("Data")] [SerializeField] private ConvaiTranscriptStyleData transcriptStyleData;

        [SerializeField] private ConvaiConfigurationDataSO configurationDataSO;
        [SerializeField] private float fadeDuration = 0.5f;

        [Header("Visuals")] [SerializeField] private TMP_Dropdown transcriptStyleDropdown;

        [SerializeField] private TMP_Dropdown voiceInputDropdown;
        [SerializeField] private TMP_InputField playerNameInputField;
        [SerializeField] private Toggle transcriptToggle;
        [SerializeField] private Toggle notificationToggle;
        [SerializeField] private Button saveButton;
        [SerializeField] private GameObject microphoneDropContainer;
        [SerializeField] private Button closeButton;

        private CanvasGroup _canvasGroup;
        private FadeCanvas _fadeCanvas;

        private void Awake()
        {
            InitializeTranscriptDropdown();
            InitializeMicrophoneDropdown();
            GetConfigurationData();
            saveButton.onClick.AddListener(SaveConfigurationData);
            closeButton.onClick.AddListener(CloseButtonClicked);
            _canvasGroup = GetComponent<CanvasGroup>();
            _fadeCanvas = GetComponent<FadeCanvas>();
            transcriptStyleDropdown.onValueChanged.AddListener(TranscriptStyleChanged);
        }


        private void OnEnable()
        {
            playerNameInputField.text = configurationDataSO.PlayerName;
            transcriptToggle.isOn = configurationDataSO.TranscriptSystemEnabled;
            notificationToggle.isOn = configurationDataSO.NotificationSystemEnabled;
            SafetyCheckForIndexes();
            transcriptStyleDropdown.SetValueWithoutNotify(configurationDataSO.ActiveTranscriptStyleIndex);
            voiceInputDropdown.value = configurationDataSO.ActiveVoiceInputIndex;
        }


        public void Show() => _fadeCanvas.StartFadeIn(_canvasGroup, fadeDuration);

        public void Hide() => _fadeCanvas.StartFadeOut(_canvasGroup, fadeDuration);

        private void SafetyCheckForIndexes()
        {
            if (transcriptStyleDropdown.options.Count <= configurationDataSO.ActiveTranscriptStyleIndex)
            {
                configurationDataSO.ActiveTranscriptStyleIndex = 0;
            }

            if (voiceInputDropdown.options.Count <= configurationDataSO.ActiveVoiceInputIndex)
            {
                configurationDataSO.ActiveVoiceInputIndex = 0;
            }

            microphoneDropContainer.SetActive(voiceInputDropdown.options.Count > 0);
        }

        private void GetConfigurationData()
        {
            if (configurationDataSO == null)
            {
                ConvaiConfigurationDataSO.GetData(out configurationDataSO);
            }
        }

        private void InitializeMicrophoneDropdown()
        {
            voiceInputDropdown.ClearOptions();
            voiceInputDropdown.AddOptions(Microphone.devices.ToList());
        }

        private void InitializeTranscriptDropdown()
        {
            transcriptStyleDropdown.ClearOptions();
            transcriptStyleDropdown.AddOptions(transcriptStyleData.styleList.ConvertAll(style => style.name));
        }

        private void SaveConfigurationData()
        {
            configurationDataSO.PlayerName = playerNameInputField.text;
            configurationDataSO.TranscriptSystemEnabled = transcriptToggle.isOn;
            configurationDataSO.NotificationSystemEnabled = notificationToggle.isOn;
            configurationDataSO.ActiveTranscriptStyleIndex = transcriptStyleDropdown.value;
            configurationDataSO.ActiveVoiceInputIndex = voiceInputDropdown.value;

            ConvaiConfigurationDataSystem.SaveConfigurationData(configurationDataSO);
        }

        private void CloseButtonClicked() => ConvaiServices.UISystem.HideSettings();

        private void TranscriptStyleChanged(int index) => StartCoroutine(PreviewStyle(index));

        private IEnumerator PreviewStyle(int index)
        {
            ConvaiServices.UISystem.PreviewStyle(index);
            ConvaiServices.UISystem.HideSettings();
            yield return new WaitForSeconds(1.9f);
            ConvaiServices.UISystem.ShowSettings();
        }
    }
}
