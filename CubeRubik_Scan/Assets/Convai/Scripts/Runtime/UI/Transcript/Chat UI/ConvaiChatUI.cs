using System.Collections.Generic;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.Player;
using Convai.Scripts.Services;
using Convai.Scripts.Services.TranscriptSystem;
using Convai.Scripts.TranscriptUI.Filters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Convai.Scripts.TranscriptUI.Chat_UI
{
    public class ConvaiChatUI : ConvaiTranscriptUIBase
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform chatContainer;
        [SerializeField] private ConvaiMessageUI characterChatMessageUI;
        [SerializeField] private ConvaiMessageUI playerChatMessageUI;
        [SerializeField] private TMP_InputField chatInputField;

        private Dictionary<string, ConvaiMessageUI> _activeMessages = new();
        private ConvaiProximityNPCFilter _filter;
        private ConvaiMessageUI _lastCharacterChatMessageUI;
        private string _lastCharacterChatMessageKey;

        public override void OnActivate()
        {
            base.OnActivate();
            // Ensure filter is set up
            if (!TranscriptHandler.gameObject.TryGetComponent(out _filter))
            {
                _filter = TranscriptHandler.gameObject.AddComponent<ConvaiProximityNPCFilter>();
            }

            _activeMessages = new Dictionary<string, ConvaiMessageUI>();
            _lastCharacterChatMessageKey = null;
            chatInputField.onSubmit.AddListener(ChatInputField_OnSubmit);
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            _activeMessages.Clear();
            _lastCharacterChatMessageUI = null;
            _lastCharacterChatMessageKey = null;
            if (_filter != null)
            {
                Destroy(_filter);
                _filter = null;
            }
        }

        private void ChatInputField_OnSubmit(string text)
        {
            if (TranscriptHandler.ConvaiPlayer == null)
            {
                ConvaiUnityLogger.Info("No convai player found", LogCategory.UI);
                return;
            }

            chatInputField.SetTextWithoutNotify(string.Empty);
            //TranscriptHandler.ConvaiPlayer.SendTextMessage(text);
        }


        protected override void OnVisibleCharacterIDChanged(string id, bool newState)
        {
            if (newState)
            {
                if (_lastCharacterChatMessageUI != null && _lastCharacterChatMessageUI.Identifier == id &&
                    !_lastCharacterChatMessageUI.IsCompleted && !string.IsNullOrEmpty(_lastCharacterChatMessageKey))
                {
                    _activeMessages[_lastCharacterChatMessageKey] = _lastCharacterChatMessageUI;
                }
            }
            else
            {
                RemoveMessagesForSender(id);
            }

            HandleFading();
        }

        protected override void OnCharacterMessage(ConvaiTranscriptData transcript)
        {
            if (!TranscriptHandler.visibleCharacterChatIds.Contains(transcript.Identifier))
            {
                return;
            }

            string messageKey = string.IsNullOrEmpty(transcript.MessageKey) ? transcript.Identifier : transcript.MessageKey;

            if (_activeMessages.TryGetValue(messageKey, out ConvaiMessageUI ui))
            {
                ConvaiUnityLogger.DebugLog($"Updating existing message for {transcript.Identifier}", LogCategory.UI);
                if (!string.IsNullOrEmpty(transcript.Message))
                {
                    ConvaiUnityLogger.DebugLog($"Message for {transcript.Identifier} is not empty, updating UI.", LogCategory.UI);
                    ui.SetMessage(transcript.Message);
                }

                RemoveActiveMessage(ref transcript);
                ScrollToBottom();
            }
            else
            {
                if (string.IsNullOrEmpty(transcript.Message))
                {
                    return;
                }

                ConvaiUnityLogger.DebugLog($"Creating new message for {transcript.Identifier}", LogCategory.UI);
                ConvaiMessageUI newChatMessage = CreateNewMessage(characterChatMessageUI, messageKey);
                _lastCharacterChatMessageUI = newChatMessage;
                _lastCharacterChatMessageKey = messageKey;
                if (ConvaiServices.CharacterLocatorService.GetNPC(transcript.Identifier, out ConvaiNPC npc))
                {
                    InitializeMessageUI(newChatMessage, ref transcript, npc.TranscriptMetaData);
                }
                else
                {
                    InitializeMessageUI(newChatMessage, ref transcript, new ConvaiTranscriptMetaData());
                }

                ScrollToBottom();
            }
        }


        protected override void OnPlayerMessage(ConvaiTranscriptData transcript)
        {
            string messageKey = string.IsNullOrEmpty(transcript.MessageKey) ? transcript.Identifier : transcript.MessageKey;

            if (_activeMessages.TryGetValue(messageKey, out ConvaiMessageUI ui))
            {
                ui.SetMessage(transcript.Message);
                RemoveActiveMessage(ref transcript);
                ScrollToBottom();
            }
            else
            {
                ConvaiMessageUI newChatMessage = CreateNewMessage(playerChatMessageUI, messageKey);
                if (ConvaiServices.CharacterLocatorService.GetPlayer(transcript.Identifier, out ConvaiPlayer player))
                {
                    InitializeMessageUI(newChatMessage, ref transcript, player.TranscriptMetaData);
                }
                else
                {
                    InitializeMessageUI(newChatMessage, ref transcript, new ConvaiTranscriptMetaData());
                }

                ScrollToBottom();
            }
        }

        private void RemoveActiveMessage(ref ConvaiTranscriptData transcript)
        {
            if (!transcript.IsLastChunk)
            {
                return;
            }

            string messageKey = string.IsNullOrEmpty(transcript.MessageKey) ? transcript.Identifier : transcript.MessageKey;
            if (_activeMessages.TryGetValue(messageKey, out ConvaiMessageUI messageUI))
            {
                ConvaiUnityLogger.DebugLog($"Removing active message for {transcript.Identifier}", LogCategory.UI);
                messageUI.IsCompleted = true;
                RemoveActiveMessage(messageKey);

                if (_lastCharacterChatMessageKey == messageKey)
                {
                    _lastCharacterChatMessageUI = null;
                    _lastCharacterChatMessageKey = null;
                }
            }
        }

        private void RemoveActiveMessage(string messageKey) => _activeMessages.Remove(messageKey);


        private ConvaiMessageUI CreateNewMessage(ConvaiMessageUI prefab, string messageKey)
        {
            ConvaiMessageUI newChatMessage = Instantiate(prefab, chatContainer.transform);
            newChatMessage.gameObject.SetActive(true);
            _activeMessages[messageKey] = newChatMessage;
            return newChatMessage;
        }

        private void RemoveMessagesForSender(string senderId)
        {
            List<string> keysToRemove = new List<string>();
            foreach (KeyValuePair<string, ConvaiMessageUI> kvp in _activeMessages)
            {
                if (kvp.Value.Identifier == senderId)
                {
                    kvp.Value.IsCompleted = true;
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (string key in keysToRemove)
            {
                _activeMessages.Remove(key);
                if (_lastCharacterChatMessageKey == key)
                {
                    _lastCharacterChatMessageUI = null;
                    _lastCharacterChatMessageKey = null;
                }
            }
        }

        private void InitializeMessageUI(ConvaiMessageUI newChatMessage, ref ConvaiTranscriptData transcript,
            ConvaiTranscriptMetaData transcriptMetaData)
        {
            newChatMessage.Identifier = transcript.Identifier;
            newChatMessage.SetSender(transcript.Name);
            newChatMessage.SetMessage(transcript.Message);
            newChatMessage.SetSenderColor(transcriptMetaData.nameTagColor);
        }

        private void ScrollToBottom() => scrollRect.verticalNormalizedPosition = 0;

        protected override void OnInteractionIDCreated(string characterId, string interactionID)
        {
            if (_activeMessages.TryGetValue(characterId, out ConvaiMessageUI ui))
            {
                ui.SetInteractionID(interactionID);
            }
        }
    }
}
