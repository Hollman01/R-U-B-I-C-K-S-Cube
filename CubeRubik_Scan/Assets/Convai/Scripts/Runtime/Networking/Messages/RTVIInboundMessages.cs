#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Convai.Scripts.RTVI.Inbound
{
    // A base class used only to peek at the 'type' field of a message
    public class RTVIBaseMessage
    {
        [JsonProperty("type")] public string? Type { get; set; }
    }

    // A generic wrapper for messages that have a nested 'data' payload
    public class RTVIWrapper<T> : RTVIBaseMessage
    {
        [JsonProperty("data")] public T? Payload { get; set; }
    }

    // A special wrapper for server messages that have a nested data structure.
    public class ServerMessage<T> : RTVIBaseMessage
    {
        [JsonProperty("data")] public T? Data { get; set; }
    }

    // --- Server -> Client Payload Models ---

    public class UserTranscriptionPayload
    {
        [JsonProperty("participant_id")] public string? ParticipantId { get; set; }

        [JsonProperty("text")] public string? Text { get; set; }

        [JsonProperty("final")] public bool IsFinal { get; set; }
    }

    public class BotTranscriptionPayload
    {
        [JsonProperty("text")] public string? Text { get; set; }
    }

    public class BehaviorTreeResponsePayload
    {
        [JsonProperty("bt_code")] public string? BtCode { get; set; }

        [JsonProperty("bt_constants")] public string? BtConstants { get; set; }

        [JsonProperty("narrative_section_id")] public string? NarrativeSectionId { get; set; }
    }

    public class FinalUserTranscriptionPayload
    {
        [JsonProperty("text")] public string? Text { get; set; }
    }

    public class VideoControlPayload
    {
        [JsonProperty("action")] public string? Action { get; set; }

        [JsonProperty("preset")] public string? Preset { get; set; }

        [JsonProperty("width")] public int? Width { get; set; }

        [JsonProperty("height")] public int? Height { get; set; }

        [JsonProperty("fps")] public int? Fps { get; set; }

        [JsonProperty("max_bitrate")] public int? MaxBitrate { get; set; }

        [JsonProperty("simulcast")] public bool? Simulcast { get; set; }

        [JsonProperty("codec")] public string? Codec { get; set; }

        [JsonProperty("track_source")] public string? TrackSource { get; set; }

        [JsonProperty("track_name")] public string? TrackName { get; set; }
    }

    // --- Legacy Server -> Client Message Classes ---
    // Base class for identifying inbound message types
    public class RTVIReceiveMessageBase
    {
        [JsonProperty("type")] public string? Type { get; set; }
    }

    public class RTVIBehaviorTreeResponse : RTVIReceiveMessageBase
    {
        [JsonProperty("bt_code")] public string? BtCode { get; set; }

        [JsonProperty("bt_constants")] public string? BtConstants { get; set; }

        [JsonProperty("narrative_section_id")] public string? NarrativeSectionId { get; set; }
    }

    public class RTVIModerationResponse : RTVIReceiveMessageBase
    {
        [JsonProperty("result")] public bool Result { get; set; }

        [JsonProperty("user_input")] public string? UserInput { get; set; }

        [JsonProperty("reason")] public string? Reason { get; set; }
    }

    public class RTVIUserTranscriptionMessage : RTVIReceiveMessageBase
    {
        [JsonProperty("data")] public RTVIUserTranscriptionMessageData? Data { get; set; }
    }

    public class RTVIUserTranscriptionMessageData
    {
        [JsonProperty("text")] public string? Text { get; set; }

        [JsonProperty("user_id")] public string? UserId { get; set; }

        [JsonProperty("timestamp")] public string? Timestamp { get; set; }

        [JsonProperty("final")] public bool IsFinal { get; set; }
    }

    // --- Additional Server -> Client Message Classes ---

    public class RTVIBotEmotionMessage : RTVIReceiveMessageBase
    {
        [JsonProperty("emotion")] public string? Emotion { get; set; }

        [JsonProperty("scale")] public int Scale { get; set; } // 1-3 intensity scale
    }

    public class RTVILLMFunctionCallMessageData
    {
        [JsonProperty("function_name")] public string? FunctionName { get; set; }

        [JsonProperty("tool_call_id")] public string? ToolCallId { get; set; }

        [JsonProperty("args")] public Dictionary<string, object>? Args { get; set; }
    }

    public class RTVILLMFunctionCallMessage : RTVIReceiveMessageBase
    {
        [JsonProperty("data")] public RTVILLMFunctionCallMessageData? Data { get; set; }
    }

    public class RTVILLMFunctionCallStartMessageData
    {
        [JsonProperty("function_name")] public string? FunctionName { get; set; }
    }

    public class RTVILLMFunctionCallStartMessage : RTVIReceiveMessageBase
    {
        [JsonProperty("data")] public RTVILLMFunctionCallStartMessageData? Data { get; set; }
    }

    public class RTVITextMessageData
    {
        [JsonProperty("text")] public string? Text { get; set; }
    }

    public class RTVIAudioMessageData
    {
        [JsonProperty("audio")] public string? Audio { get; set; } // Base64 encoded audio

        [JsonProperty("sample_rate")] public int SampleRate { get; set; }

        [JsonProperty("num_channels")] public int NumChannels { get; set; }
    }

    public class RTVIBotTTSAudioMessage : RTVIReceiveMessageBase
    {
        [JsonProperty("data")] public RTVIAudioMessageData? Data { get; set; }
    }

    public class RTVIUserLLMTextMessage : RTVIReceiveMessageBase
    {
        [JsonProperty("data")] public RTVITextMessageData? Data { get; set; }
    }

    public class RTVIMetricsMessage : RTVIReceiveMessageBase
    {
        [JsonProperty("data")] public Dictionary<string, object>? Data { get; set; }
    }
}
