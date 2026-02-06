using System;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.NarrativeDesign;
using Convai.Scripts.RTVI.Outbound;
using Convai.Scripts.Services;
using Convai.Scripts.Services.TranscriptSystem;
using UnityEngine;

namespace Convai.Scripts
{
    public class ConvaiNPC : MonoBehaviour, IConvaiNPCEvents
    {
        [field: SerializeField] public string CharacterName { get; private set; }
        [field: SerializeField] public string CharacterID { get; private set; }
        [field: SerializeField] public ConvaiTranscriptMetaData TranscriptMetaData { get; private set; }

        [Header("Session Resumption")]
        [field: SerializeField]
        public bool EnableSessionResume { get; private set; }

        [Header("Narrative Design")] [SerializeField]
        private ConvaiNarrativeDesignController _narrativeDesignController = new();

        private string _currentMessage = string.Empty;
        private string _currentMessageKey;
        private int _messageSequence;
        private bool _isSpeaking = false;
        private bool _isLLMActive = false;

        public bool IsSpeechMuted =>
            ConvaiRoomManager.Instance != null && ConvaiRoomManager.Instance.IsNpcAudioMuted(this);

        private void OnEnable() => ConvaiServices.CharacterLocatorService.AddNPC(this);

        private void OnDisable() => ConvaiServices.CharacterLocatorService.RemoveNPC(this);

        public void SendTriggerEvent(string triggerName, string triggerMessage = null)
        {
            if (ConvaiRoomManager.Instance.IsConnectedToRoom)
            {
                RTVITriggerMessage trigger = new(triggerName, triggerMessage);
                ConvaiRoomManager.Instance.RTVIHandler.SendData(trigger);
            }
            else
            {
                ConvaiRoomManager.Instance.OnRoomConnectionSuccessful.AddListener(() =>
                {
                    RTVITriggerMessage trigger = new(triggerName, triggerMessage);
                    ConvaiRoomManager.Instance.RTVIHandler.SendData(trigger);
                });
            }
        }

        public void ToggleSpeech()
        {
            if (IsSpeechMuted)
            {
                UnmuteSpeech();
            }
            else
            {
                MuteSpeech();
            }
        }

        public bool MuteSpeech()
        {
            if (ConvaiRoomManager.Instance == null)
            {
                ConvaiUnityLogger.DebugLog($"[{CharacterName}] [{CharacterID}] Cannot mute speech - room manager not available.",
                    LogCategory.SDK);
                return false;
            }

            return ConvaiRoomManager.Instance.MuteNpc(this);
        }

        public bool UnmuteSpeech()
        {
            if (ConvaiRoomManager.Instance == null)
            {
                ConvaiUnityLogger.DebugLog($"[{CharacterName}] [{CharacterID}] Cannot unmute speech - room manager not available.",
                    LogCategory.SDK);
                return false;
            }

            return ConvaiRoomManager.Instance.UnmuteNpc(this);
        }

        #region IConvaiNPCEvents Implementation

        public void OnCharacterTranscriptionReceived(string transcript, bool isFinal)
        {
            if (string.IsNullOrEmpty(transcript))
            {
                ConvaiUnityLogger.DebugLog($"[{CharacterName}] [{CharacterID}] received empty transcript", LogCategory.Character);
                return;
            }

            if (!isFinal)
            {
                return;
            }

            // Process transcriptions during LLM phase (before speaking) or during speaking
            // Ignore transcriptions that arrive outside of these phases (from previous sessions)
            if (!_isSpeaking && !_isLLMActive)
            {
                ConvaiUnityLogger.DebugLog(
                    $"[{CharacterName}] [{CharacterID}] Received transcription outside active session, ignoring: '{transcript}'", 
                    LogCategory.Character);
                return;
            }

            // Merge the new transcription chunk with the current message
            string mergedMessage = MergeTranscriptSegments(_currentMessage, transcript);
            bool hasChanged = !string.Equals(_currentMessage, mergedMessage, StringComparison.Ordinal);
            _currentMessage = mergedMessage;

            if (!hasChanged)
            {
                return;
            }

            ConvaiUnityLogger.DebugLog(
                $"[{CharacterName}] [{CharacterID}] Transcript updated: '{_currentMessage}'", LogCategory.Character);
            
            // Only broadcast to UI if we're currently speaking
            // During LLM phase, we accumulate transcriptions and broadcast them when speaking starts
            if (_isSpeaking)
            {
                EnsureCurrentMessageKey();
                // Update the message in UI (not final - will be finalized when speaking stops)
                ConvaiServices.TranscriptService.BroadcastCharacterMessage(CharacterID, CharacterName, _currentMessage, false,
                    _currentMessageKey);
            }
        }

        public void OnCharacterStartedSpeaking()
        {
            ConvaiUnityLogger.DebugLog($"[{CharacterName}] [{CharacterID}] Started speaking.", LogCategory.SDK);
            
            // If we have an existing message from a previous speaking session, finalize it
            // This ensures each bot-started-speaking event creates a new chat box
            // Note: If _isLLMActive is true, _currentMessage may contain transcriptions from current LLM session
            // which should continue, not be finalized
            if (!_isLLMActive)
            {
                FinalizeCurrentMessage("Previous message finalized on speaking start");
            }
            
            _isSpeaking = true;
            
            // If we have accumulated transcriptions from LLM phase, broadcast them now
            // This creates the new chat box for this speaking session
            if (!string.IsNullOrEmpty(_currentMessage))
            {
                EnsureCurrentMessageKey();
                ConvaiServices.TranscriptService.BroadcastCharacterMessage(CharacterID, CharacterName, _currentMessage, false,
                    _currentMessageKey);
                ConvaiUnityLogger.DebugLog(
                    $"[{CharacterName}] [{CharacterID}] Broadcasting accumulated transcriptions on speaking start: '{_currentMessage}'", 
                    LogCategory.Character);
            }
        }

        public void OnCharacterStoppedSpeaking()
        {
            ConvaiUnityLogger.DebugLog($"[{CharacterName}] [{CharacterID}] Stopped speaking.", LogCategory.SDK);
            _isSpeaking = false;
            FinalizeCurrentMessage("Final transcript delivered");
        }

        public void OnLLMStarted()
        {
            ConvaiUnityLogger.DebugLog($"[{CharacterName}] [{CharacterID}] LLM Started.", LogCategory.SDK);
            _isLLMActive = true;
            _isSpeaking = false;

            if (!string.IsNullOrEmpty(_currentMessage))
            {
                FinalizeCurrentMessage("Pending transcript flushed on LLM start");
            }

            EnsureCurrentMessageKey();
            _currentMessage = string.Empty; // Reset current message when LLM starts
        }

        public void OnLLMStopped()
        {
            ConvaiUnityLogger.DebugLog($"[{CharacterName}] [{CharacterID}] LLM Stopped.", LogCategory.SDK);
            _isLLMActive = false;
        }

        public void OnTTSStarted() => ConvaiUnityLogger.DebugLog($"[{CharacterName}] [{CharacterID}] TTS Started.", LogCategory.SDK);

        public void OnTTSStopped() => ConvaiUnityLogger.DebugLog($"[{CharacterName}] [{CharacterID}] TTS Stopped.", LogCategory.SDK);

        public void OnCurrentNarrativeDesignSectionIDReceived(string sectionID) =>
            _narrativeDesignController.OnNarrativeDesignSectionReceived(sectionID);

        #endregion

        private void EnsureCurrentMessageKey()
        {
            if (!string.IsNullOrEmpty(_currentMessageKey))
            {
                return;
            }

            _messageSequence++;
            _currentMessageKey = $"{CharacterID}:{_messageSequence}";
        }

        private void FinalizeCurrentMessage(string logContext)
        {
            if (string.IsNullOrEmpty(_currentMessage))
            {
                ResetCurrentMessageState();
                return;
            }

            EnsureCurrentMessageKey();
            ConvaiServices.TranscriptService.BroadcastCharacterMessage(CharacterID, CharacterName, _currentMessage, true,
                _currentMessageKey);
            ConvaiUnityLogger.DebugLog(
                $"[{CharacterName}] [{CharacterID}] {logContext}: '{_currentMessage}'", LogCategory.Character);
            ResetCurrentMessageState();
        }

        private void ResetCurrentMessageState()
        {
            _currentMessage = string.Empty;
            _currentMessageKey = null;
        }

        private static string MergeTranscriptSegments(string currentMessage, string newSegment)
        {
            if (string.IsNullOrEmpty(currentMessage))
            {
                return newSegment;
            }

            if (string.IsNullOrEmpty(newSegment))
            {
                return currentMessage;
            }

            // Trim whitespace for better matching
            string trimmedCurrent = currentMessage.TrimEnd();
            string trimmedNew = newSegment.TrimStart();

            // Check if new segment starts with whitespace (likely a continuation)
            bool newStartsWithSpace = newSegment.Length > 0 && char.IsWhiteSpace(newSegment[0]);

            // Try to find overlap between the end of current and start of new
            int maxOverlap = Math.Min(trimmedCurrent.Length, trimmedNew.Length);
            
            // Look for word boundary overlaps first (more reliable)
            for (int overlap = maxOverlap; overlap > 0; overlap--)
            {
                if (overlap > trimmedCurrent.Length || overlap > trimmedNew.Length)
                    continue;

                string currentEnd = trimmedCurrent.Substring(trimmedCurrent.Length - overlap);
                string newStart = trimmedNew.Substring(0, overlap);

                if (string.Equals(currentEnd, newStart, StringComparison.Ordinal))
                {
                    // Found overlap - merge by appending the non-overlapping part
                    string nonOverlapping = trimmedNew.Substring(overlap);
                    // If overlap was found, the segments connect naturally - just append non-overlapping part
                    // Preserve original whitespace from newSegment if it had leading space
                    return trimmedCurrent + (newStartsWithSpace && nonOverlapping.Length > 0 ? " " : "") + nonOverlapping;
                }
            }

            // No overlap found - check if we should concatenate or treat as separate
            // If new segment starts with a capital letter and current ends with punctuation, 
            // they're likely separate sentences - but we'll still merge them as they're part of same response
            if (newStartsWithSpace)
            {
                // New segment starts with space - it's a continuation
                return trimmedCurrent + newSegment;
            }
            else if (trimmedCurrent.EndsWith(".", StringComparison.Ordinal) || 
                     trimmedCurrent.EndsWith("!", StringComparison.Ordinal) || 
                     trimmedCurrent.EndsWith("?", StringComparison.Ordinal))
            {
                // Current ends with punctuation, new doesn't start with space - add space
                return trimmedCurrent + " " + trimmedNew;
            }
            else
            {
                // Default: add space between
                return trimmedCurrent + " " + trimmedNew;
            }
        }
    }
}
