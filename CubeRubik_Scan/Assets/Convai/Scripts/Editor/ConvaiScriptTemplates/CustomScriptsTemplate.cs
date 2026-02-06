using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Convai.Editor.ScriptTemplates
{
    public static class CustomScriptsTemplate
    {
        private const string CLASS_NAME = "#SCRIPTNAME#";

        [MenuItem("Assets/Create/Convai/Actions/Custom Convai Action")]
        private static void CreateNewActionClass()
        {
            string scriptCreationPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            CreateFile(scriptCreationPath, "NewConvaiAction.txt", "CustomConvaiAction.cs");
        }

        [MenuItem("Assets/Create/Convai/Actions/Custom Convai Action Creator")]
        private static void CreateNewActionCreatorClass()
        {
            string scriptCreationPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            CreateFile(scriptCreationPath, "NewConvaiActionCreator.txt", "CustomConvaiActionCreator.cs");
        }


        private static void CreateFile(string scriptPath, string fileName, string defaultName)
        {
            try
            {
                string templatePath = Path.Combine(FindConvaiScriptTemplateFolder(), fileName);
                string path = EditorUtility.SaveFilePanel($"Create New {Path.GetFileNameWithoutExtension(defaultName)}", scriptPath, defaultName,
                    "cs");
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                string scriptTemplate = File.ReadAllText(templatePath);
                string scriptName = Path.GetFileNameWithoutExtension(path);
                string finalScript = scriptTemplate.Replace(CLASS_NAME, scriptName);

                // Write the script to the specified path
                File.WriteAllText(path, finalScript);
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }


        private static string FindConvaiScriptTemplateFolder()
        {
            string[] guids = AssetDatabase.FindAssets("ConvaiScriptTemplates");
            return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : string.Empty;
        }
    }
}
