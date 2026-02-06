using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections
{
    [UxmlElement]
    public partial class ConvaiPackageManagementSection : ConvaiBaseSection
    {
        public const string SECTION_NAME = "package-management";

        private readonly Dictionary<string, string> _buttonNames = new()
        {
            { "ar-btn", "Install AR Package" },
            { "vr-btn", "Install VR Package" },
            { "mr-btn", "Install MR Package" },
            { "ios-btn", "Install iOS Build Package" },
            { "urp-btn", "Install URP Converter" },
            { "rtl-btn", "Convai Custom TMP Package" }
        };


        public ConvaiPackageManagementSection()
        {
            AddToClassList("section-card");
            Add(ConvaiVisualElementUtility.CreateLabel("section-header", "Package Management", "header"));

            VisualElement container = new() { name = "container", style = { flexWrap = Wrap.Wrap, flexDirection = FlexDirection.Row } };

            foreach (KeyValuePair<string, string> keyValuePair in _buttonNames)
            {
                Button button = new() { name = keyValuePair.Key, text = keyValuePair.Value };
                ConvaiVisualElementUtility.AddStyles(button, "button-small");
                button.style.width = new StyleLength(new Length(30, LengthUnit.Percent));
                button.style.flexGrow = 1;
                button.style.fontSize = 12;
                button.clicked += () => { InstallPackage(keyValuePair.Key); };
                container.Add(button);
            }

            Add(container);
        }


        private void InstallPackage(string pkgName) => Debug.Log($"Installing package: {pkgName}");
        // TODO: Implement package installation
    }
}
