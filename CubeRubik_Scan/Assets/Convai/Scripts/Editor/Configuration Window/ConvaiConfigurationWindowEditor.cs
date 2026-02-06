using System;
using System.Collections.Generic;
using Convai.Editor.Configuration_Window.Components;
using Convai.Editor.Configuration_Window.Components.Sections;
using Convai.Editor.Configuration_Window.Components.Sections.LoggerSettings;
using Convai.Editor.Configuration_Window.Components.Sections.LongTermMemory;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window
{
    public class ConvaiConfigurationWindowEditor : EditorWindow
    {
        private const string UNITY_DEFAULT = "Assets/Convai/Scripts/Editor/Configuration Window/Style/UnityThemes/UnityDefaultRuntimeTheme.tss";
        private const string STYLE_SHEET_PATH = "Assets/Convai/Scripts/Editor/Configuration Window/Style/Convai Configuration Window Stylesheet.uss";

        private static ConvaiConfigurationWindow _configurationWindow;

        private static readonly HashSet<string> _apiKeyDependentSections = new()
        {
            ConvaiLoggerSettingSection.SECTION_NAME, /* ConvaiPackageManagementSection.SECTION_NAME, */ ConvaiLongTermMemorySection.SECTION_NAME
        };

        private static bool _isApiKeySet;

        public static bool IsApiKeySet
        {
            get => _isApiKeySet;
            set
            {
                _isApiKeySet = value;
                if (_isApiKeySet)
                {
                    OnAPIKeySet?.Invoke();
                }
            }
        }

        private void CreateGUI()
        {
            _configurationWindow = new ConvaiConfigurationWindow();
            rootVisualElement.Add(_configurationWindow);
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_SHEET_PATH);
            StyleSheet unityStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(UNITY_DEFAULT);
            rootVisualElement.styleSheets.Clear();
            rootVisualElement.styleSheets.Add(unityStyle);
            rootVisualElement.styleSheets.Add(styleSheet);
        }

        public static event Action OnAPIKeySet;

        [MenuItem("Convai/Welcome", priority = 1)]
        public static void OpenWelcomeWindow() => OpenSection(ConvaiWelcomeSection.SECTION_NAME);

        [MenuItem("Convai/Account", priority = 2)]
        public static void OpenAccountWindow() => OpenSection(ConvaiAccountSection.SECTION_NAME);

        // [MenuItem("Convai/Package Management", priority = 3)] // Hidden
        // public static void OpenPackageWindow() => OpenSection(ConvaiPackageManagementSection.SECTION_NAME);

        [MenuItem("Convai/Logger Settings", priority = 4)]
        public static void OpenLoggerWindow() => OpenSection(ConvaiLoggerSettingSection.SECTION_NAME);

        // [MenuItem("Convai/Documentation", priority = 5)] // Hidden
        // public static void OpenDocumentationWindow() => OpenSection(ConvaiDocumentationSection.SECTION_NAME);

        [MenuItem("Convai/Updates", priority = 6)]
        public static void OpenUpdateWindow() => OpenSection(ConvaiUpdatesSection.SECTION_NAME);

        [MenuItem("Convai/Contact Us", priority = 7)]
        public static void OpenContactUsWindow() => OpenSection(ConvaiContactSection.SECTION_NAME);

        [MenuItem("Convai/Long Term Memory", priority = 8)]
        public static void OpenLTMWindow() => OpenSection(ConvaiLongTermMemorySection.SECTION_NAME);


        private static void OpenSection(string sectionName)
        {
            _configurationWindow = new ConvaiConfigurationWindow();
            IsApiKeySet = false;
            Rect rect = new(100, 100, 1200, 550);
            ConvaiConfigurationWindowEditor window = GetWindowWithRect<ConvaiConfigurationWindowEditor>(rect, true, "Convai SDK Setup", true);
            window.minSize = window.maxSize = rect.size;
            window.Show();
            if (!IsApiKeySet && _apiKeyDependentSections.Contains(sectionName))
            {
                EditorUtility.DisplayDialog("API Key Required", "Please set up your API Key to access this section.", "OK");
                return;
            }

            _configurationWindow.OpenSection(sectionName);
        }
    }
}
