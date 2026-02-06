using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Convai.Scripts.RTVI.Outbound
{
    // Client -> Server Messages
    public abstract class RTVISendMessageBase
    {
        [JsonProperty("label")] public string Label { get; protected set; } = "rtvi-ai";

        [JsonProperty("type")] public string Type { get; protected set; }

        [JsonProperty("id")] public string Id { get; protected set; } = Guid.NewGuid().ToString();

        [JsonProperty("data")] public object Data { get; protected set; }
    }

    public class DynamicInfo
    {
        [JsonProperty("text")] public string Text { get; set; }
    }

    public class RTVITriggerMessage : RTVISendMessageBase
    {
        public RTVITriggerMessage(string triggerName, string triggerMessage = null)
        {
            Type = "trigger-message";
            Data = new Dictionary<string, string> { { "trigger_name", triggerName }, { "trigger_message", triggerMessage } };
        }
    }

    public class RTVIUpdateTemplateKeys : RTVISendMessageBase
    {
        public RTVIUpdateTemplateKeys(Dictionary<string, string> templateKeys)
        {
            Type = "update-template-keys";
            Data = new { template_keys = templateKeys };
        }
    }

    public class RTVIUpdateDynamicInfo : RTVISendMessageBase
    {
        public RTVIUpdateDynamicInfo(DynamicInfo dynamicInfo)
        {
            Type = "update-dynamic-info";
            Data = new { dynamic_info = dynamicInfo };
        }
    }

    public class RTVIUserTextMessage : RTVISendMessageBase
    {
        public RTVIUserTextMessage(string text)
        {
            Type = "user_text_message";
            Data = new { text };
        }
    }

    public class RTVIUpdateSceneMetadata : RTVISendMessageBase
    {
        public RTVIUpdateSceneMetadata(List<SceneMetadata> sceneMetadata)
        {
            Type = "update-scene-metadata";
            Data = new { metadata = sceneMetadata };
        }
    }

    [Serializable]
    public class SceneMetadata
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
    }
}
