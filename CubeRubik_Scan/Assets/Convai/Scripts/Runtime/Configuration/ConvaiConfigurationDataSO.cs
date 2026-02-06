using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Convai.Scripts.Configuration
{
    [Serializable]
    [CreateAssetMenu(fileName = "ConvaiConfigurationData", menuName = "Convai/Configuration Data")]
    public class ConvaiConfigurationDataSO : ScriptableObject
    {
        [field: SerializeField] public string APIKey { get; set; }
        [field: SerializeField] public string PlayerName { get; set; }
        [field: SerializeField] public string SpeakerID { get; set; }
        [field: SerializeField] public int ActiveVoiceInputIndex { get; set; }
        [field: SerializeField] public int ActiveTranscriptStyleIndex { get; set; }
        [field: SerializeField] public bool TranscriptSystemEnabled { get; set; }
        [field: SerializeField] public bool NotificationSystemEnabled { get; set; }
        [SerializeField] private readonly List<SerializableKeyValuePair> _characterSessionIdMapList = new();

        // Expose as a Dictionary for convenience
        public Dictionary<string, string> CharacterSessionIdMap
        {
            get
            {
                Dictionary<string, string> dict = new();
                foreach (SerializableKeyValuePair kvp in _characterSessionIdMapList)
                {
                    if (!dict.ContainsKey(kvp.Key))
                        dict[kvp.Key] = kvp.Value;
                }
                return dict;
            }
            set
            {
                _characterSessionIdMapList.Clear();
                if (value != null)
                {
                    foreach (KeyValuePair<string, string> kvp in value)
                    {
                        _characterSessionIdMapList.Add(new SerializableKeyValuePair { Key = kvp.Key, Value = kvp.Value });
                    }
                }
            }
        }

        [Serializable]
        public struct SerializableKeyValuePair
        {
            public string Key;
            public string Value;
        }
        public void Save()
        {
            ConvaiConfigurationDataSystem.SaveConfigurationData(this);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        public void Load(ConvaiConfigurationDataSO configurationData)
        {
            APIKey = configurationData.APIKey;
            PlayerName = configurationData.PlayerName;
            SpeakerID = configurationData.SpeakerID;
            ActiveVoiceInputIndex = configurationData.ActiveVoiceInputIndex;
            ActiveTranscriptStyleIndex = configurationData.ActiveTranscriptStyleIndex;
            TranscriptSystemEnabled = configurationData.TranscriptSystemEnabled;
            NotificationSystemEnabled = configurationData.NotificationSystemEnabled;
            CharacterSessionIdMap = configurationData.CharacterSessionIdMap ?? new Dictionary<string, string>();
        }

        public static bool GetData(out ConvaiConfigurationDataSO data)
        {
            data = Resources.Load<ConvaiConfigurationDataSO>(nameof(ConvaiConfigurationDataSO));
#if UNITY_EDITOR
            if (data != null)
            {
                try
                {
                    ConvaiConfigurationDataSO loadedData = ConvaiConfigurationDataSystem.LoadConfigurationData();
                    data.Load(loadedData);
                }
                catch (FileNotFoundException)
                {
                    // Configuration file doesn't exist yet, use default values from ScriptableObject
                    Debug.LogWarning("ConvaiConfigurationDataSO: Configuration file not found, using default values.");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"ConvaiConfigurationDataSO: Error loading configuration: {ex.Message}");
                }
            }
#endif
            return data != null;
        }

        public static ConvaiConfigurationDataSO Copy(ConvaiConfigurationDataSO resourceData)
        {
            ConvaiConfigurationDataSO copy = CreateInstance<ConvaiConfigurationDataSO>();
            copy.APIKey = resourceData.APIKey;
            copy.PlayerName = resourceData.PlayerName;
            copy.SpeakerID = resourceData.SpeakerID;
            copy.ActiveVoiceInputIndex = resourceData.ActiveVoiceInputIndex;
            copy.ActiveTranscriptStyleIndex = resourceData.ActiveTranscriptStyleIndex;
            copy.TranscriptSystemEnabled = resourceData.TranscriptSystemEnabled;
            copy.NotificationSystemEnabled = resourceData.NotificationSystemEnabled;
            copy.CharacterSessionIdMap = resourceData.CharacterSessionIdMap ?? new Dictionary<string, string>();
            return copy;
        }

        /// <summary>
        ///     Stores a character session ID for session resumption
        /// </summary>
        /// <param name="characterId">The character ID</param>
        /// <param name="sessionId">The session ID to store</param>
        public void StoreCharacterSessionId(string characterId, string sessionId)
        {
            CharacterSessionIdMap ??= new Dictionary<string, string>();

            CharacterSessionIdMap[characterId] = sessionId;
            Save();
        }

        /// <summary>
        ///     Gets a stored character session ID for resumption
        /// </summary>
        /// <param name="characterId">The character ID</param>
        /// <returns>The stored session ID, or null if not found</returns>
        public string GetCharacterSessionId(string characterId) => CharacterSessionIdMap == null || !CharacterSessionIdMap.ContainsKey(characterId)
            ? null
            : CharacterSessionIdMap[characterId];

        /// <summary>
        ///     Clears a stored character session ID
        /// </summary>
        /// <param name="characterId">The character ID</param>
        public void ClearCharacterSessionId(string characterId)
        {
            if (CharacterSessionIdMap != null && CharacterSessionIdMap.ContainsKey(characterId))
            {
                CharacterSessionIdMap.Remove(characterId);
                Save();
            }
        }

        /// <summary>
        ///     Clears all stored character session IDs
        /// </summary>
        public void ClearAllCharacterSessionIds()
        {
            CharacterSessionIdMap.Clear();
            Save();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ConvaiConfigurationDataSO))]
    internal class ConvaiConfigurationDataSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Save"))
            {
                (target as ConvaiConfigurationDataSO)?.Save();
            }
        }
    }
#endif
}
