using System;
using System.Text;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.RTVI.Outbound;
using LiveKit;
using LiveKit.Proto;
using Newtonsoft.Json;
using UnityEngine;

namespace Convai.Scripts
{
    public class ConvaiNPCBrain
    {
        private readonly ConvaiNPC _npc;
        private readonly GameObject _player;
        private AudioStream _audioStream;
        private MicrophoneSource _microphoneSource;

        public ConvaiNPCBrain(ConvaiNPC npc, GameObject player)
        {
            _npc = npc;
            _player = player;
        }

        private string RoomName { get; set; }
        private string SessionID { get; set; }
        public Room Room { get; private set; }


        public void SendData(RTVISendMessageBase message)
        {
            if (Room?.LocalParticipant == null)
            {
                ConvaiUnityLogger.Error("[RTVI] Cannot send data, not connected to a room.", LogCategory.SDK);
                return;
            }

            try
            {
                string json = JsonConvert.SerializeObject(message);
                byte[] data = Encoding.UTF8.GetBytes(json);
                Room.LocalParticipant.PublishData(data);
                ConvaiUnityLogger.Info($"[RTVI] Sent Data: {json}", LogCategory.SDK);
            }
            catch (Exception ex)
            {
                ConvaiUnityLogger.Error($"[RTVI] Error sending data: {ex.Message}", LogCategory.SDK);
            }
        }

        //public void DebugAudioState()
        //{
        //    ConvaiUnityLogger.Info("=== AUDIO DEBUG STATE ===", LogCategory.SDK);
        //    ConvaiUnityLogger.Info($"Room connected: {Room?.IsConnected}", LogCategory.SDK);
        //    ConvaiUnityLogger.Info($"Local participant: {Room?.LocalParticipant?.Identity}", LogCategory.SDK);
        //    ConvaiUnityLogger.Info($"Remote participants: {Room?.RemoteParticipants?.Count}", LogCategory.SDK);

        //    if (Room?.RemoteParticipants is { Count: > 0 })
        //    {
        //        foreach (RemoteParticipant participant in Room.RemoteParticipants.Values)
        //        {
        //            ConvaiUnityLogger.Info(
        //                $"  - Participant: {participant.Identity}, Tracks: {participant.Tracks.Count}",
        //                LogCategory.SDK);
        //            foreach (RemoteTrackPublication track in participant.Tracks.Values)
        //            {
        //                ConvaiUnityLogger.Info(
        //                    $"    - Track: {track.Name}, Kind: {track.Kind}, Subscribed: {track.Subscribed}",
        //                    LogCategory.SDK);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        ConvaiUnityLogger.Warn("⚠️  NO BOT PARTICIPANT FOUND! Bot may not have joined the room.",
        //            LogCategory.SDK);
        //        ConvaiUnityLogger.Info("   - Check server logs for bot startup errors", LogCategory.SDK);
        //        ConvaiUnityLogger.Info($"   - Verify bot is connecting to room: {RoomName}", LogCategory.SDK);
        //        ConvaiUnityLogger.Info($"   - Session ID: {SessionID}", LogCategory.SDK);
        //    }

        //    ConvaiUnityLogger.Info($"Microphone source active: {_microphoneSource != null}", LogCategory.SDK);
        //    ConvaiUnityLogger.Info($"Audio stream active: {_audioStream != null}", LogCategory.SDK);
        //    ConvaiUnityLogger.Info(
        //        $"Microphone permission: {Application.HasUserAuthorization(UserAuthorization.Microphone)}",
        //        LogCategory.SDK);
        //    ConvaiUnityLogger.Info("=== END AUDIO DEBUG ===", LogCategory.SDK);
        //}
    }
}
