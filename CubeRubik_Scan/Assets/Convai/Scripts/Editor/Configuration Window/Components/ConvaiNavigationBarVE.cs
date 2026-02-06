using System;
using System.Collections.Generic;
using Convai.Editor.Configuration_Window.Components.Sections;
using Convai.Editor.Configuration_Window.Components.Sections.LoggerSettings;
using Convai.Editor.Configuration_Window.Components.Sections.LongTermMemory;
using Convai.Editor.Configuration_Window.Components.Sections.ServerAnimation;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components
{
    [UxmlElement]
    public partial class ConvaiNavigationBarVE : VisualElement
    {
        private readonly Dictionary<string, string> _navButtonNames = new()
        {
            { ConvaiWelcomeSection.SECTION_NAME, "Welcome" },
            { ConvaiAccountSection.SECTION_NAME, "Account" },
            // { ConvaiPackageManagementSection.SECTION_NAME, "Package Management" }, // Hidden
            { ConvaiLoggerSettingSection.SECTION_NAME, "Logger Settings" },
            // { ConvaiDocumentationSection.SECTION_NAME, "Documentation" }, // Hidden
            { ConvaiUpdatesSection.SECTION_NAME, "Updates" },
            { ConvaiContactSection.SECTION_NAME, "Contact Us" },
            { ConvaiLongTermMemorySection.SECTION_NAME, "Long Term Memory" },
            { ConvaiServerAnimationSection.SECTION_NAME, "Server Animation" }
        };


        private readonly Dictionary<string, Button> _navButtons = new();
        private ScrollView _buttonsContainer;
        private VisualElement _logo;
        public Action<string> OnNavigationButtonClicked;

        public ConvaiNavigationBarVE()
        {
            AddToClassList("nav-bar");
            CreateLogo();
            CreateNavigationButtons();
        }

        private void CreateLogo()
        {
            _logo = new VisualElement
            {
                name = "convai-logo",
                style =
                {
                    backgroundImage =
                        new StyleBackground(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Convai/Art/UI/Logos/Convai Logo.png"))
                }
            };
            _logo.AddToClassList("convai-logo");
            Add(_logo);
        }

        private void CreateNavigationButtons()
        {
            _buttonsContainer = new ScrollView { name = "buttons-container" };
            foreach (KeyValuePair<string, string> navButtonName in _navButtonNames)
            {
                Button button = new() { text = navButtonName.Value, name = $"{navButtonName.Key}-btn" };
                button.AddToClassList("nav-bar-btn");
                button.clicked += () =>
                {
                    OnNavigationButtonClicked?.Invoke(navButtonName.Key);
                };
                _buttonsContainer.Add(button);
                _navButtons.Add(navButtonName.Key, button);
            }

            Add(_buttonsContainer);
        }

        public void NavigateTo(string section)
        {
            foreach (KeyValuePair<string, Button> pair in _navButtons)
            {
                if (pair.Key == section)
                {
                    pair.Value.AddToClassList("nav-bar-btn--active");
                }
                else
                {
                    pair.Value.RemoveFromClassList("nav-bar-btn--active");
                }
            }
        }
    }
}
