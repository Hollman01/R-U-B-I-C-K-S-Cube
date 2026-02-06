using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Convai.RestAPI;
using Convai.RestAPI.Internal;
using Convai.Scripts.Configuration;
using Convai.Scripts.LoggerSystem;
using UnityEditor;
using UnityEngine;

namespace Convai.Editor.Configuration_Window.Components.Sections.ServerAnimation
{
    internal static class ServerAnimationService
    {
        private const string DIRECTORY_SAVE_KEY = "CONVAI_SERVER_ANIMATION_SAVE_PATH";
        private static string _savePath;
        private static int _downloadCount;
        private static int _totalDownloads;

        // TODO: Fix it
        public static async void ImportAnimations(List<ServerAnimationItemResponse> animations, Action onSuccess, Action<string> onError)
        {
            if (!ConvaiConfigurationDataSO.GetData(out ConvaiConfigurationDataSO dataSO))
            {
                ConvaiUnityLogger.Error("ConvaiConfigurationDataSO not found in Resources folder.", LogCategory.Editor);
                onError.Invoke("ConvaiConfigurationDataSO not found in Resources folder.");
                return;
            }

            if (string.IsNullOrEmpty(dataSO.APIKey))
            {
                ConvaiUnityLogger.Warn("API Key is null", LogCategory.REST);
                onError.Invoke("API Key is null");
                return;
            }

            if (animations.Count == 0)
            {
                EditorUtility.DisplayDialog("Import Animation Process", "Cannot start import process since no animations are selected", "Ok");
                onError.Invoke("No animations selected");
                return;
            }

            _savePath = UpdateAnimationSavePath();
            if (string.IsNullOrEmpty(_savePath))
            {
                EditorUtility.DisplayDialog("Failed", "Import Operation Cancelled", "Ok");
                onError.Invoke("No animations selected");
                return;
            }

            List<string> allAnimations = animations.Select(x => x.AnimationName).ToList();
            List<string> successfulImports = new();
            _totalDownloads = animations.Count;
            _downloadCount = 0;
            EditorUtility.DisplayProgressBar("Importing Animations", "Downloading Animations", 0f);
            foreach (ServerAnimationItemResponse anim in animations)
            {
                ConvaiREST.GetAnimationDataOperation operation = new(new GetAnimationItemModel(dataSO.APIKey, anim.AnimationID));
                while (!operation.IsCompleted)
                {
                    // Wait for operation to complete
                    await Task.Delay(100);
                }

                if (operation.WasSuccess)
                {
                    ConvaiREST.DownloadFile(operation.Result.Animation.FbxGcpFile, bytes =>
                    {
                        _downloadCount++;
                        EditorUtility.DisplayProgressBar("Importing Animations", $"Downloading Animations {_downloadCount}/{_totalDownloads}",
                            (float)_downloadCount / _totalDownloads);
                        successfulImports.Add(anim.AnimationName);
                        SaveAnimation(bytes, anim.AnimationName);
                        if (_downloadCount == _totalDownloads)
                        {
                            EditorUtility.ClearProgressBar();
                            LogResult(successfulImports, allAnimations);
                            AssetDatabase.Refresh();
                            onSuccess.Invoke();
                        }
                    }, error =>
                    {
                        ConvaiUnityLogger.Error(error, LogCategory.REST);
                    });
                }
            }
        }

        private static void LogResult(List<string> successfulImports, List<string> animPaths)
        {
            string dialogMessage = $"Successfully Imported{Environment.NewLine}";
            successfulImports.ForEach(x => dialogMessage += x + Environment.NewLine);
            List<string> unSuccessFullImports = animPaths.Except(successfulImports).ToList();
            if (unSuccessFullImports.Count > 0)
            {
                dialogMessage += $"Could not import{Environment.NewLine}";
                unSuccessFullImports.ForEach(x => dialogMessage += x + Environment.NewLine);
            }

            EditorUtility.DisplayDialog("Import Animation Result", dialogMessage, "Ok");
        }

        private static string UpdateAnimationSavePath()
        {
            string selectedPath;
            string currentPath = EditorPrefs.GetString(DIRECTORY_SAVE_KEY, Application.dataPath);
            while (true)
            {
                selectedPath = EditorUtility.OpenFolderPanel("Select folder within project", currentPath, "");
                if (string.IsNullOrEmpty(selectedPath))
                {
                    selectedPath = string.Empty;
                    break;
                }

                if (!IsSubfolder(selectedPath, Application.dataPath))
                {
                    EditorUtility.DisplayDialog("Invalid Folder Selected", "Please select a folder within the project", "Ok");
                    continue;
                }

                EditorPrefs.SetString(DIRECTORY_SAVE_KEY, selectedPath);
                break;
            }

            return selectedPath;
        }

        private static bool IsSubfolder(string pathA, string pathB)
        {
            // Get full paths to handle any relative path issues
            string fullPathA = Path.GetFullPath(pathA);
            string fullPathB = Path.GetFullPath(pathB);

            // Create URI objects for the paths
            Uri uriA = new(fullPathA);
            Uri uriB = new(fullPathB);

            // Check if pathA is under pathB
            return uriB.IsBaseOf(uriA);
        }

        private static async void SaveAnimation(byte[] bytes, string newFileName)
        {
            // Ensure the save directory exists
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            // Construct the full file path with the new name and .fbx extension
            string filePath = Path.Combine(_savePath, $"{newFileName}.fbx");
            int counter = 1;

            while (File.Exists(filePath))
            {
                filePath = Path.Combine(_savePath, $"{newFileName}_{counter}.fbx");
                counter++;
            }

            // Write the file
            await File.WriteAllBytesAsync(filePath, bytes);
            string relativePath = filePath.Substring(Application.dataPath.Length + 1).Replace('\\', '/');
            relativePath = "Assets/" + relativePath;
            //AssetDatabase.ImportAsset(relativePath);
            AssetDatabase.Refresh();
            ModelImporter importer = AssetImporter.GetAtPath(relativePath) as ModelImporter;
            if (importer != null)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.importAnimatedCustomProperties = true;
                importer.materialLocation = ModelImporterMaterialLocation.External;
                importer.SaveAndReimport();
            }
            else
            {
                Debug.LogError("Failed to get importer for " + filePath);
            }
        }
    }
}
