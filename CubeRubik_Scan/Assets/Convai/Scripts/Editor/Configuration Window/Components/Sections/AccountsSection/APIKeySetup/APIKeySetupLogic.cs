using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Convai.RestAPI;
using Convai.Scripts.Configuration;
using Convai.Scripts.LoggerSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections.AccountsSection.APIKeySetup
{
    public abstract class APIKeySetupLogic
    {
        private static readonly string _apiKeyAssetPath = Path.Combine("Assets", "Convai", "Resources", $"{nameof(ConvaiConfigurationDataSO)}.asset");


        public static async void BeginButtonTask(string apiKey, Action<bool, bool> callback)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a valid API Key.", "OK");
                callback?.Invoke(false, false);
                return;
            }

            ConvaiModel model = new(apiKey);
            ConvaiREST.ValidateAPIOperation validateAPIOperation = new(model);
            while (!validateAPIOperation.IsCompleted)
            {
                await Task.Delay(100);
            }

            if (validateAPIOperation.WasSuccess)
            {
                string referralStatus = validateAPIOperation.Result.Status;
                if (string.IsNullOrEmpty(referralStatus))
                {
                    EditorUtility.DisplayDialog("Error", "Something went wrong. Please check your API Key. Contact support@convai.com for more help.",
                        "OK");
                    callback?.Invoke(false, false);
                    return;
                }


                if (string.IsNullOrEmpty(referralStatus) || (referralStatus.Trim().ToLower() != "undefined" && referralStatus.Trim().ToLower() != ""))
                {
                    callback?.Invoke(false, false);
                }

                ConvaiConfigurationDataSO aPIKeySetup = LoadOrCreateAPIKeyAsset();
                aPIKeySetup.APIKey = apiKey;
                EditorUtility.SetDirty(aPIKeySetup);
                AssetDatabase.SaveAssets();
                aPIKeySetup.Save();
                EditorUtility.DisplayDialog("Success", "[Step 1/2] API Key loaded successfully!", "OK");
                callback?.Invoke(true, false);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Something went wrong. Please check your API Key. Contact support@convai.com for more help.",
                    "OK");
                callback?.Invoke(false, false);
            }
        }

        public static async Task ContinueEvent(string selectedOption, string otherOption, APIKeyReferralWindow window)
        {
            List<string> attributionSourceOptions = new()
            {
                "Search Engine (Google, Bing, etc.)",
                "Youtube",
                "Social Media (Facebook, Instagram, TikTok, etc.)",
                "Friend Referral",
                "Unity Asset Store",
                "Others"
            };

            int currentChoiceIndex = attributionSourceOptions.IndexOf(selectedOption);

            if (currentChoiceIndex < 0)
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid referral source!", "OK");
                return;
            }

            if (!ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO data))
            {
                ConvaiUnityLogger.Error("ConvaiConfigurationDataSO not found in Resources folder.", LogCategory.Editor);
                return;
            }

            ConvaiREST.UpdateReferralStatusOperation operation =
                new(new UpdateReferralSourceModel(data.APIKey, attributionSourceOptions[currentChoiceIndex]));
            while (!operation.IsCompleted)
            {
                await Task.Delay(100);
            }

            if (operation.WasSuccess)
            {
                EditorUtility.DisplayDialog("Success", "Setup completed successfully!", "OK");
            }

            window.Close();
        }

        private static ConvaiConfigurationDataSO LoadOrCreateAPIKeyAsset()
        {
            // Try to load existing asset first
            ConvaiConfigurationDataSO existingAsset = AssetDatabase.LoadAssetAtPath<ConvaiConfigurationDataSO>(_apiKeyAssetPath);
            
            if (existingAsset != null)
            {
                // Asset exists, return it so we can update it
                return existingAsset;
            }

            // Asset doesn't exist, create a new one
            ConvaiConfigurationDataSO newAsset = ScriptableObject.CreateInstance<ConvaiConfigurationDataSO>();
            
            // Set default values
            newAsset.PlayerName = "Player";
            newAsset.TranscriptSystemEnabled = true;
            newAsset.NotificationSystemEnabled = true;
            
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

            AssetDatabase.CreateAsset(newAsset, _apiKeyAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            return newAsset;
        }

        private static void DeleteAPIKeyAsset()
        {
            if (File.Exists(_apiKeyAssetPath))
            {
                Debug.Log("Deleting API Key asset");
                AssetDatabase.DeleteAsset(_apiKeyAssetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static void LoadExistingApiKey(TextField apiKeyField, Button saveApiKeyButton)
        {
            if (!ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO data))
            {
                saveApiKeyButton.text = "Save API Key";
                return;
            }

            if (string.IsNullOrEmpty(data.APIKey))
            {
                saveApiKeyButton.text = "Save API Key";
                return;
            }

            apiKeyField.value = data.APIKey;
            saveApiKeyButton.text = "Update API Key";
            ConvaiConfigurationWindowEditor.IsApiKeySet = true;
        }
    }
}
