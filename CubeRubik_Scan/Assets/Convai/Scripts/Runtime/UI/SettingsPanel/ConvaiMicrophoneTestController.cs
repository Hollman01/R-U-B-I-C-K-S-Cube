using System;
using System.Collections;
using Convai.Scripts.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Convai.Scripts.SettingsPanelUI
{
    public class ConvaiMicrophoneTestController : MonoBehaviour
    {
        private const string WAITING_FOR_RECORDING = "Waiting for recording...";
        private const string RECORDING = "Recording...";
        private const string PLAYING = "Playing...";
        private const string RECORD = "Record";
        private const string STOP = "Stop";
        private const string NO_MICROPHONE_DETECTED = "No Microphone Detected";
        private const string MICROPHONE_PERMISSION_DENIED = "Microphone Permission Denied";
        private const int RECORDING_LENGTH = 10;
        private const int FREQUENCY = 44100;


        [SerializeField] private TMP_Dropdown microphoneDropdown;
        [SerializeField] private TextMeshProUGUI recordStatusText;
        [SerializeField] private Button recordButton;
        [SerializeField] private RectTransform waveVisualizerUI;
        [SerializeField] private RectTransform waveVisualizerBackground;

        private readonly float[] _clipSampleData = new float[1024];
        private readonly float _waveMultiplier = 500;
        private AudioSource _audioSource;
        private bool _isRecording;
        private Coroutine _playAudioCoroutine;

        private AudioClip _recording;


        private void OnEnable()
        {
            recordButton.onClick.AddListener(OnRecordButtonClicked);
            InitializeRecordStatusText();
            InitializeAudioSource();
        }

        private void InitializeAudioSource()
        {
            if (!TryGetComponent(out _audioSource))
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void InitializeRecordStatusText()
        {
            if (Microphone.devices.Length == 0)
            {
                recordStatusText.text = NO_MICROPHONE_DETECTED;
                return;
            }

            if (ConvaiServices.PermissionService.HasMicrophonePermission())
            {
                recordStatusText.text = WAITING_FOR_RECORDING;
            }
            else
            {
                ConvaiServices.PermissionService.RequestMicrophonePermission(hasPermission =>
                {
                    recordStatusText.text = hasPermission ? WAITING_FOR_RECORDING : MICROPHONE_PERMISSION_DENIED;
                });
            }
        }

        private void OnRecordButtonClicked()
        {
            if (_isRecording)
            {
                StopRecording();
            }
            else
            {
                StartRecording();
            }
        }

        private void StartRecording()
        {
            if (!ConvaiServices.PermissionService.HasMicrophonePermission())
            {
                return;
            }

            if (_playAudioCoroutine != null)
            {
                StopCoroutine(_playAudioCoroutine);
            }

            waveVisualizerUI.sizeDelta = new Vector2(2, waveVisualizerUI.sizeDelta.y);
            _audioSource.Stop();
            _audioSource.clip = null;
            _isRecording = true;
            recordStatusText.text = RECORDING;

            recordButton.GetComponent<Image>().color = Color.red;

            TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
            
            buttonText.text = STOP;
            buttonText.color = Color.white;

            _recording = Microphone.Start(Microphone.devices[microphoneDropdown.value], false, RECORDING_LENGTH, FREQUENCY);
        }

        private void StopRecording()
        {
            _isRecording = false;
            int position = Microphone.GetPosition(Microphone.devices[microphoneDropdown.value]);
            _audioSource.clip = _recording;
            Microphone.End(Microphone.devices[microphoneDropdown.value]);
            TrimAudio(position);
            recordStatusText.text = PLAYING;

            recordButton.GetComponent<Image>().color = Color.green;

            TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = RECORD;
            buttonText.color = new Color(0.14f, 0.14f, 0.14f);

            _playAudioCoroutine = StartCoroutine(PlayAudio());
        }

        private IEnumerator PlayAudio()
        {
            _audioSource.Play();
            while (_audioSource.isPlaying)
            {
                ShowAudioSourceAudioWaves();
                yield return null;
            }

            recordStatusText.text = WAITING_FOR_RECORDING;
            waveVisualizerUI.sizeDelta = new Vector2(2, waveVisualizerUI.sizeDelta.y);
        }

        private void TrimAudio(int micRecordLastPosition)
        {
            if (_audioSource.clip == null || micRecordLastPosition <= 0)
            {
                return;
            }

            AudioClip tempAudioClip = _audioSource.clip;
            int channels = tempAudioClip.channels;
            int position = micRecordLastPosition;
            float[] samplesArray = new float[position * channels];
            tempAudioClip.GetData(samplesArray, 0);

            // Calculate the number of samples to remove (0.5 seconds)
            int samplesToRemove = Mathf.RoundToInt(0.5f * FREQUENCY) * channels;

            // Ensure we don't try to remove more samples than we have
            if (samplesToRemove >= samplesArray.Length)
            {
                Debug.LogWarning("Recording is too short to remove 0.5 seconds. Keeping original clip.");
                return;
            }

            // Create a new array with the first 0.5 seconds removed
            float[] trimmedSamplesArray = new float[samplesArray.Length - samplesToRemove];
            Array.Copy(samplesArray, samplesToRemove, trimmedSamplesArray, 0, trimmedSamplesArray.Length);

            // Create a new AudioClip with the trimmed samples
            AudioClip newClip = AudioClip.Create("RecordedSound", trimmedSamplesArray.Length / channels, channels, FREQUENCY, false);
            newClip.SetData(trimmedSamplesArray, 0);
            _audioSource.clip = newClip;
        }

        private void ShowAudioSourceAudioWaves()
        {
            _audioSource.GetSpectrumData(_clipSampleData, 0, FFTWindow.Rectangular);
            float sum = 0;
            Array.ForEach(_clipSampleData, sample => sum += sample);
            float currentAverageVolume = sum * _waveMultiplier;
            Vector2 size = waveVisualizerUI.sizeDelta;
            size.x = currentAverageVolume;
            size.x = Mathf.Clamp(size.x, 2, waveVisualizerBackground.sizeDelta.x);
            waveVisualizerUI.sizeDelta = size;
        }
    }
}
