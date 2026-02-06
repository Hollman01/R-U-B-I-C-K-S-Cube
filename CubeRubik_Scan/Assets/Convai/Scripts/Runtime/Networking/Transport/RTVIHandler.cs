using System;
using System.Text;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.RTVI.Inbound;
using Convai.Scripts.Services.Core;
using LiveKit;
using LiveKit.Proto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Convai.Scripts.Networking.Transport
{
    public class RTVIHandler
    {
        private readonly ConvaiRoomManager _convaiRoomManager;
        private readonly Room _currentRoom;
        private readonly UserTranscriptionCoordinator _userTranscriptionCoordinator;

        public RTVIHandler(Room currentRoom, ConvaiRoomManager roomManager)
        {
            _currentRoom = currentRoom;
            _convaiRoomManager = roomManager;
            _userTranscriptionCoordinator = new UserTranscriptionCoordinator(
                roomManager.Player,
                RunOnMainThread
            );
            if (_currentRoom != null)
            {
                // Subscribe to the DataReceived event to handle incoming data packets
                _currentRoom.DataReceived += OnDataReceived;
            }
            else
            {
                Debug.LogError("[RTVIHandler] Current room is null. Cannot subscribe to DataReceived event.");
            }
        }

        ~RTVIHandler()
        {
            // Ensure we unsubscribe from the event when the handler is destroyed
            if (_currentRoom != null)
            {
                _currentRoom.DataReceived -= OnDataReceived;
            }
        }

        public void SendData(object data)
        {
            if (_currentRoom == null)
            {
                Debug.LogError("[RTVIHandler] Cannot send data, room is not initialized.");
                return;
            }

            string json = JsonConvert.SerializeObject(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            _currentRoom.LocalParticipant.PublishData(bytes);
            Debug.Log($"[RTVIHandler] Sent data: {json}");
        }

        // This is the core method that processes every incoming data packets and events from server.
        private void OnDataReceived(byte[] data, Participant participant, DataPacketKind kind, string topic)
        {
            string json = Encoding.UTF8.GetString(data);
            Debug.Log($"[RTVIHandler] Received data: {json}");

            try
            {
                // First, deserialize into the base message just to get the 'type' field.
                RTVIBaseMessage baseMessage = JsonConvert.DeserializeObject<RTVIBaseMessage>(json);

                if (baseMessage == null || string.IsNullOrEmpty(baseMessage.Type))
                {
                    Debug.LogWarning("[RTVIHandler] Received invalid message (missing type).");
                    return;
                }

                // Use a switch to handle different message types.
                switch (baseMessage.Type)
                {
                    case "bot-ready":
                        HandleBotReady();
                        break;
                    case "user-started-speaking":
                        HandleUserStartedSpeaking();
                        break;
                    case "user-transcription":
                        HandleUserTranscription(json);
                        break;
                    case "user-stopped-speaking":
                        HandleUserStoppedSpeaking();
                        break;

                    case "bot-llm-started":
                        HandleBotLLM_Started(participant);
                        break;
                    case "bot-llm-text":
                        HandleCharacterInterimTranscription(participant, json);
                        break;
                    case "bot-transcription":
                        HandleCharacterFinalTranscription(participant, json);
                        break;
                    case "bot-llm-stopped":
                        HandleBotLLM_Stopped(participant);
                        break;

                    case "bot-tts-started":
                        HandleBotTTS_Started(participant);
                        break;
                    case "bot-tts-stopped":
                        HandleBotTTS_Stopped(participant);
                        break;

                    case "bot-started-speaking":
                        HandleBotStartSpeaking(participant);
                        break;
                    case "bot-stopped-speaking":
                        HandleBotStopSpeaking(participant);
                        break;

                    case "server-message":
                        ParseServerMessage(participant, json);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[RTVIHandler] Failed to parse message: {e.Message}\nJSON: {json}");
            }
        }

        private void HandleBotReady()
        {
            RunOnMainThread(() => _convaiRoomManager?.OnBotReadyReceived());
        }

        private void HandleUserStartedSpeaking()
        {
            _userTranscriptionCoordinator.HandleStart();
            RunOnMainThread(() => _convaiRoomManager?.OnUserStartedSpeakingReceived());
        }

        private void HandleUserStoppedSpeaking()
        {
            _userTranscriptionCoordinator.HandleStop();
            RunOnMainThread(() => _convaiRoomManager?.OnUserStoppedSpeakingReceived());
        }

        private static void RunOnMainThread(System.Action action)
        {
            if (!MainThreadDispatcher.Post(action))
            {
                Debug.LogWarning("[RTVIHandler] Failed to enqueue work on main thread dispatcher.");
            }
        }

        private void HandleUserTranscription(string json)
        {
            RTVIWrapper<UserTranscriptionPayload> userTransMsg = JsonConvert.DeserializeObject<RTVIWrapper<UserTranscriptionPayload>>(json);
            if (userTransMsg?.Payload != null)
            {
                if (userTransMsg.Payload.IsFinal)
                {
                    _userTranscriptionCoordinator.HandleAsrFinal(userTransMsg.Payload.Text ?? string.Empty);
                }
                else
                {
                    _userTranscriptionCoordinator.HandleInterim(userTransMsg.Payload.Text ?? string.Empty);
                }
            }
        }

        private void HandleBotLLM_Started(Participant participant)
        {
            IConvaiNPCEvents events = GetNPCEvents(participant);
            if (events != null)
            {
                RunOnMainThread(() => events.OnLLMStarted());
            }
            else
            {
                ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPC events handler found for bot-llm-started.",
                    LogCategory.Character);
            }
            RunOnMainThread(() => _convaiRoomManager?.OnBotLLMStartedReceived());
        }

        private void HandleBotLLM_Stopped(Participant participant)
        {
            IConvaiNPCEvents events = GetNPCEvents(participant);
            if (events != null)
            {
                RunOnMainThread(() => events.OnLLMStopped());
            }
            else
            {
                ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPC events handler found for bot-llm-stopped.",
                    LogCategory.Character);
            }
            RunOnMainThread(() => _convaiRoomManager?.OnBotLLMStoppedReceived());
        }

        private void HandleBotTTS_Started(Participant participant)
        {
            IConvaiNPCEvents events = GetNPCEvents(participant);
            if (events != null)
            {
                RunOnMainThread(() => events.OnTTSStarted());
            }
            else
            {
                ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPC events handler found for bot-tts-started.",
                    LogCategory.Character);
            }
            RunOnMainThread(() => _convaiRoomManager?.OnBotTTSStartedReceived());
        }

        private void HandleBotTTS_Stopped(Participant participant)
        {
            IConvaiNPCEvents events = GetNPCEvents(participant);
            if (events != null)
            {
                RunOnMainThread(() => events.OnTTSStopped());
            }
            else
            {
                ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPC events handler found for bot-tts-stopped.",
                    LogCategory.Character);
            }
            RunOnMainThread(() => _convaiRoomManager?.OnBotTTSStoppedReceived());
        }

        private void HandleBotStartSpeaking(Participant participant)
        {
            IConvaiNPCEvents events = GetNPCEvents(participant);
            if (events != null)
            {
                RunOnMainThread(() => events.OnCharacterStartedSpeaking());
            }
            else
            {
                ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPC events handler found for bot-started-speaking.",
                    LogCategory.Character);
            }
            RunOnMainThread(() => _convaiRoomManager?.OnBotStartedSpeakingReceived());
        }

        private void HandleBotStopSpeaking(Participant participant)
        {
            IConvaiNPCEvents events = GetNPCEvents(participant);
            if (events != null)
            {
                RunOnMainThread(() => events.OnCharacterStoppedSpeaking());
            }
            else
            {
                ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPC events handler found for bot-stopped-speaking.",
                    LogCategory.Character);
            }
            RunOnMainThread(() => _convaiRoomManager?.OnBotStoppedSpeakingReceived());
        }

        private void HandleCharacterInterimTranscription(Participant participant, string json)
        {
            RTVIWrapper<BotTranscriptionPayload> botLlmTextMsg =
                JsonConvert.DeserializeObject<RTVIWrapper<BotTranscriptionPayload>>(json);
            if (botLlmTextMsg?.Payload != null)
            {
                //OnBotTranscriptionReceived?.Invoke(botLlmTextMsg.Payload.Text ?? "", false); // Interim
                IConvaiNPCEvents events = GetNPCEvents(participant);
                if (events != null)
                {
                    RunOnMainThread(() => events.OnCharacterTranscriptionReceived(botLlmTextMsg.Payload.Text ?? "", false)); // Interim
                }
                else
                {
                    ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPC events handler found for bot-llm-text.",
                        LogCategory.Character);
                }
            }
        }

        private void HandleCharacterFinalTranscription(Participant participant, string json)
        {
            RTVIWrapper<BotTranscriptionPayload> botTransMsg =
                JsonConvert.DeserializeObject<RTVIWrapper<BotTranscriptionPayload>>(json);
            if (botTransMsg?.Payload != null)
            {
                IConvaiNPCEvents events = GetNPCEvents(participant);
                if (events != null)
                {
                    RunOnMainThread(() => events.OnCharacterTranscriptionReceived(botTransMsg.Payload.Text ?? "", true)); // final
                }
                else
                {
                    ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPC events handler found for bot-transcription.",
                        LogCategory.Character);
                }
            }
        }

        private void ParseServerMessage(Participant participant, string json)
        {
            JObject serverMsg = JObject.Parse(json);
            string innerType = serverMsg["data"]?["type"]?.ToString();
            switch (innerType)
            {
                case "behavior-tree-response":
                {
                    ServerMessage<BehaviorTreeResponsePayload> btWrapper =
                        JsonConvert.DeserializeObject<ServerMessage<BehaviorTreeResponsePayload>>(json);
                    if (btWrapper?.Data != null)
                    {
                        // Trigger narrative section event if section ID is present.
                        if (!string.IsNullOrEmpty(btWrapper.Data.NarrativeSectionId))
                        {
                            IConvaiNPCEvents convaiNPCEvents = GetNPCEvents(participant);
                            if (convaiNPCEvents != null)
                            {
                                RunOnMainThread(() => convaiNPCEvents.OnCurrentNarrativeDesignSectionIDReceived(btWrapper.Data.NarrativeSectionId));
                            }
                            else
                            {
                                ConvaiUnityLogger.DebugLog(
                                    "[RTVIHandler] No NPC events handler found for bot-transcription.",
                                    LogCategory.Character);
                            }
                        }
                    }
                }
                    break;
                case "final-user-transcription":
                {
                    ServerMessage<FinalUserTranscriptionPayload> finalTransWrapper =
                        JsonConvert.DeserializeObject<ServerMessage<FinalUserTranscriptionPayload>>(json);
                    if (finalTransWrapper?.Data != null)
                    {
                        string cleanedText = finalTransWrapper.Data.Text ?? string.Empty;
                        _userTranscriptionCoordinator.HandleProcessedFinal(cleanedText);
                        Debug.Log($"[RTVIHandler] Received final player transcription: {cleanedText}");
                    }
                }
                    break;
                case "video-control":
                {
                    ServerMessage<VideoControlPayload> videoControlWrapper =
                        JsonConvert.DeserializeObject<ServerMessage<VideoControlPayload>>(json);
                    if (videoControlWrapper?.Data != null)
                    {
                        RunOnMainThread(() => _convaiRoomManager?.HandleVideoControl(videoControlWrapper.Data));
                    }
                    else
                    {
                        ConvaiUnityLogger.DebugLog("[RTVIHandler] Video control message missing payload.", LogCategory.SDK);
                    }
                }
                    break;
            }
        }

        private IConvaiNPCEvents GetNPCEvents(Participant participant)
        {
            // Handle case where participant is null (some RTVI events may not have participant context)
            if (participant == null)
            {
                ConvaiUnityLogger.DebugLog("[RTVIHandler] Participant is null, using default NPC (first in list).", LogCategory.Character);

                // Fall back to first NPC if available
                if (_convaiRoomManager.NpcList != null && _convaiRoomManager.NpcList.Count > 0)
                {
                    return _convaiRoomManager.NpcList[0];
                }

                ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPCs available in NpcList.", LogCategory.Character);
                return null;
            }

            // Handle case where participant identity is null
            if (participant.Identity == null)
            {
                ConvaiUnityLogger.DebugLog($"[RTVIHandler] Participant identity is null for participant SID: {participant.Sid}.",
                    LogCategory.Character);
            }

            // Try to find NPC by participant ID
            ConvaiNPC npc = _convaiRoomManager.FindNPCByParticipantId(participant.Sid);
            if (npc != null)
            {
                ConvaiUnityLogger.DebugLog(
                    $"[RTVIHandler] Found NPC {npc.CharacterName} for participant {participant.Identity} (SID: {participant.Sid}).",
                    LogCategory.Character);
                return npc;
            }

            // Fall back to first NPC if mapping not found
            if (_convaiRoomManager.NpcList != null && _convaiRoomManager.NpcList.Count > 0)
            {
                ConvaiUnityLogger.DebugLog(
                    $"[RTVIHandler] No NPC mapping found for participant {participant.Identity} (SID: {participant.Sid}), using default NPC: {_convaiRoomManager.NpcList[0].CharacterName}.",
                    LogCategory.Character);
                return _convaiRoomManager.NpcList[0];
            }

            ConvaiUnityLogger.DebugLog("[RTVIHandler] No NPCs available and no mapping found.", LogCategory.Character);
            return null;
        }
    }
}
