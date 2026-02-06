using Assets.Convai.Scripts.Server;
using Convai.Scripts.Configuration;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.Services;
using Convai.Scripts.Services.TranscriptSystem;
using UnityEngine;

namespace Convai.Scripts.Player
{
    [RequireComponent(typeof(IConvaiNPCFinder))]
    public class ConvaiPlayer : MonoBehaviour, IConvaiPlayerEvents
    {
        [Header("Configuration")]
        [Tooltip("Reference to the main Convai Configuration Asset. All player settings are read from here.")]
        [SerializeField]
        private ConvaiConfigurationDataSO _convaiConfigurationDataSo;

        [field: SerializeField] public ConvaiTranscriptMetaData TranscriptMetaData { get; private set; }
        [field: SerializeField] public float VisionConeAngle { get; private set; }
        private string _currentMessage = string.Empty;
        public string APIKey => _convaiConfigurationDataSo?.APIKey ?? string.Empty;
        public string SpeakerID => _convaiConfigurationDataSo?.SpeakerID ?? string.Empty;
        public string PlayerName => _convaiConfigurationDataSo?.PlayerName ?? "Player";

        private void Awake()
        {
            if (_convaiConfigurationDataSo == null)
            {
                ConvaiConfigurationDataSO.GetData(out _convaiConfigurationDataSo);
            }
        }

        private void Start()
        {
            ConvaiUnityLogger.DebugLog($"[{PlayerName}] Starting Convai Player.", LogCategory.SDK);
            ConvaiRoomManager.Instance.OnRoomConnectionSuccessful.AddListener(() =>
            {
                if (_convaiConfigurationDataSo == null)
                {
                    ConvaiUnityLogger.Error("Convai Configuration Data is not set. Please assign it in the inspector.", LogCategory.SDK);
                    return;
                }

                ConvaiUnityLogger.DebugLog($"[{PlayerName}] Starting voice input with index {_convaiConfigurationDataSo.ActiveVoiceInputIndex}.",
                    LogCategory.SDK);
                ConvaiRoomManager.Instance.StartListening(_convaiConfigurationDataSo.ActiveVoiceInputIndex);
            });
        }

        private void OnEnable() => ConvaiServices.CharacterLocatorService.AddPlayer(this);

        private void OnDisable() => ConvaiServices.CharacterLocatorService.RemovePlayer(this);

        private void OnDestroy() => ConvaiRoomManager.Instance.StopListening();


        public void OnUserTranscriptionReceived(string transcript, TranscriptionPhase transcriptionPhase)
        {
            if (string.IsNullOrEmpty(transcript))
            {
                ConvaiUnityLogger.DebugLog($"[{PlayerName}] received empty transcript", LogCategory.Character);
            }

            switch (transcriptionPhase)
            {
                case TranscriptionPhase.Interim:
                    ConvaiServices.TranscriptService.BroadcastPlayerMessage(SpeakerID, PlayerName, _currentMessage + transcript, false);
                    break;
                case TranscriptionPhase.AsrFinal:
                    _currentMessage += transcript;
                    ConvaiServices.TranscriptService.BroadcastPlayerMessage(SpeakerID, PlayerName, _currentMessage, false);
                    break;
                case TranscriptionPhase.ProcessedFinal:
                    _currentMessage = transcript;
                    ConvaiServices.TranscriptService.BroadcastPlayerMessage(SpeakerID, PlayerName, _currentMessage, true);
                    break;
                case TranscriptionPhase.Completed:
                    ConvaiUnityLogger.DebugLog($"[{PlayerName}] Transcription session completed.", LogCategory.SDK);
                    break;
                case TranscriptionPhase.Listening:
                case TranscriptionPhase.Idle:
                    break;
                default:
                    ConvaiUnityLogger.DebugLog($"[{PlayerName}] Unhandled transcription phase: {transcriptionPhase}.", LogCategory.SDK);
                    break;
            }
        }

        public void OnUserStartedSpeaking(string sessionId)
        {
            _currentMessage = string.Empty; // Reset current message when speaking starts
            ConvaiUnityLogger.DebugLog($"[{PlayerName}] Started speaking. Session: {sessionId}", LogCategory.SDK);
        }

        public void OnUserStoppedSpeaking(string sessionId, bool didProduceFinalTranscript) => ConvaiUnityLogger.DebugLog(
            $"[{PlayerName}] Stopped speaking. Session: {sessionId}, final transcript produced: {didProduceFinalTranscript}", LogCategory.SDK);
    }
}
