using System;
using System.IO;
using Convai.Scripts.Configuration;
using Convai.Scripts.LoggerSystem;
using UnityEditor;
using UnityEngine;

namespace Convai.Scripts
{
    public static class ConvaiCoreInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            Debug.Log("Convai Bootstrapper: Initializing...");
            LoadConfigData();
            ConvaiUnityLogger.Initialize();
            SaveConfigDataCallbacks();

            Debug.Log("Convai Bootstrapper: Initialization complete.");
        }

        private static void SaveConfigDataCallbacks()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.ExitingPlayMode)
                {
                    return;
                }

                if (!ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO data))
                {
                    return;
                }

                Debug.Log("Saving configuration data...");
                data.Save();
            };
#else
            Application.quitting += () =>
            {
                if (!ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO data))
                {
                    return;
                }

                Debug.Log("Saving configuration data...");
                data.Save();
            };
#endif
        }

        private static void LoadConfigData()
        {
            try
            {
                ConvaiConfigurationDataSO loadedData = ConvaiConfigurationDataSystem.LoadConfigurationData();
                if (ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO data))
                {
                    data.Load(loadedData);
                }
            }
            catch (FileNotFoundException)
            {
                Debug.LogWarning("Convai Bootstrapper: Configuration file not found. Using default settings.");
                // This is fine - the system will use default values from the ScriptableObject
            }
            catch (Exception ex)
            {
                Debug.LogError($"Convai Bootstrapper: Error loading configuration data: {ex.Message}");
                // Continue with default settings
            }
        }
    }
}
