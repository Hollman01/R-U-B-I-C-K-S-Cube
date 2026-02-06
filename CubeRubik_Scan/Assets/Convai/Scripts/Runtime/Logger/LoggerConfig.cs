using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Convai.Scripts.LoggerSystem
{
    public static class LoggerConfig
    {
        private static LoggerSettings _settings;

        public static readonly ConvaiUnityLogger.LogLevel LipsyncLogFlag = Settings.LipSync;
        public static readonly ConvaiUnityLogger.LogLevel CharacterResponseFlag = Settings.Character;
        public static readonly ConvaiUnityLogger.LogLevel ActionFlag = Settings.Actions;
        public static readonly ConvaiUnityLogger.LogLevel UIFlag = Settings.UI;
        public static readonly ConvaiUnityLogger.LogLevel SDK = Settings.SDK;
        public static readonly ConvaiUnityLogger.LogLevel EditorFlag = Settings.Editor;
        public static readonly ConvaiUnityLogger.LogLevel RestAPIFlag = Settings.RestAPI;
        public static readonly ConvaiUnityLogger.LogLevel PlayerFlag = Settings.Player;


        public static LoggerSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = Resources.Load<LoggerSettings>(nameof(LoggerSettings));
                }
#if UNITY_EDITOR
                if (_settings == null)
                {
                    _settings = CreateAssetFile();
                }
#endif
                return _settings;
            }
        }

#if UNITY_EDITOR
        private static LoggerSettings CreateAssetFile()
        {
            LoggerSettings newSettings = ScriptableObject.CreateInstance<LoggerSettings>();

            string[] foldersGuid = AssetDatabase.FindAssets("Convai");
            if (foldersGuid.Length == 0)
            {
                throw new Exception("Failed to find Convai folder.");
            }

            string convaiFolder = AssetDatabase.GUIDToAssetPath(foldersGuid[0]);

            if (!AssetDatabase.IsValidFolder(Path.Combine(convaiFolder, "Resources")))
            {
                AssetDatabase.CreateFolder(convaiFolder, "Resources");
            }

            string path = Path.Combine(convaiFolder, "Resources", "LoggerSettings.asset");

            AssetDatabase.CreateAsset(newSettings, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return newSettings;
        }
#endif
    }
}
