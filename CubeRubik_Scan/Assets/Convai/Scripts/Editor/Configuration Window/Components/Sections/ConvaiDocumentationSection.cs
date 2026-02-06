using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections
{
    [UxmlElement]
    public partial class ConvaiDocumentationSection : ConvaiBaseSection
    {
        public const string SECTION_NAME = "documentation";

        private readonly Dictionary<string, List<(string, string)>> _links = new()
        {
            {
                "Getting Started",
                new List<(string, string)>
                {
                    ("Setting Up the Unity Plugin",
                        "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/setting-up-unity-plugin"),
                    ("Quick Start Tutorial", "https://youtu.be/anb9ityi0MQ"),
                    ("Video Tutorials", "https://www.youtube.com/playlist?list=PLn_7tCx0ChipYHtbe8yzdV5kMbozN2EeB")
                }
            },
            {
                "Features",
                new List<(string, string)>
                {
                    ("Narrative Design",
                        "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/adding-narrative-design-to-your-character"),
                    ("Transcript UI System",
                        "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/utilities/transcript-ui-system"),
                    ("Actions", "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/adding-actions-to-your-character"),
                    ("NPC to NPC Interaction",
                        "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/adding-npc-to-npc-conversation"),
                    ("Lip-Sync", "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/adding-lip-sync-to-your-character")
                }
            },
            {
                "Platform-Specific Builds",
                new List<(string, string)>
                {
                    ("MacOS",
                        "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/building-for-supported-platforms/microphone-permission-issue-on-intel-macs-with-universal-builds"),
                    ("iOS",
                        "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/building-for-supported-platforms/building-for-ios-ipados"),
                    ("AR / VR / MR",
                        "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/building-for-supported-platforms/building-for-ar")
                }
            },
            {
                "Troubleshooting Guide",
                new List<(string, string)> { ("FAQ", "https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/troubleshooting-guide") }
            }
        };

        public ConvaiDocumentationSection()
        {
            AddToClassList("section-card");
            Add(ConvaiVisualElementUtility.CreateLabel("header", "Documentation", "header"));
            foreach (KeyValuePair<string, List<(string, string)>> valuePair in _links)
            {
                VisualElement card = new() { name = "card" };
                card.Add(ConvaiVisualElementUtility.CreateLabel("card-title", valuePair.Key, "subheader"));
                VisualElement container = new() { name = "container", style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap } };
                foreach ((string, string) tuple in valuePair.Value)
                {
                    Button button = new() { name = "button", text = tuple.Item1 };
                    button.AddToClassList("button-small");
                    button.clicked += () =>
                    {
                        Application.OpenURL(tuple.Item2);
                    };
                    container.Add(button);
                }

                card.Add(container);
                card.AddToClassList("card");
                Add(card);
            }
        }
    }
}
