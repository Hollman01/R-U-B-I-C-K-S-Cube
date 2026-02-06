using System.Collections.Generic;
using Convai.Scripts;
using UnityEngine;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections
{
    [UxmlElement]
    public partial class ConvaiUpdatesSection : ConvaiBaseSection
    {
        public const string SECTION_NAME = "updates";


        private readonly List<Changelog> _changelogs = new()
        {
            new Changelog
            {
                Version = "v3.2.2",
                Date = "January 23, 2025",
                Sections =
                    new List<Changelog.Section>
                    {
                        new() { Title = "Hot Fixes", Changes = new List<string> { "Character Importer blocked issue fixed" } }
                    }
            },
            new Changelog
            {
                Version = "v3.2.1",
                Date = "January 19, 2025",
                Sections =
                    new List<Changelog.Section>
                    {
                        new()
                        {
                            Title = "Hot Fixes",
                            Changes =
                                new List<string>
                                {
                                    "Fixed Ready Player Me package auto-update issue in Character Importer",
                                    "Resolved script state handling in NPC Editor components",
                                    "Improved Long Term Memory UI stability"
                                }
                        }
                    }
            },
            new Changelog
            {
                Version = "v3.2.0",
                Date = "October 31, 2024",
                Sections = new List<Changelog.Section>
                {
                    new()
                    {
                        Title = "New Features",
                        Changes = new List<string>
                        {
                            "Implemented Dynamic Config Feature - This feature allows you to dynamically pass variables to NPCs. For example, you can update NPCs with the player's current health, inventory items, or information about the world, enhancing interactivity and immersion.",
                            "Implemented Narrative Design Keys - This feature enables dynamic variable passing within the Narrative Design section and triggers. For instance, you can use placeholders like {TimeOfDay} to create personalized dialogues, such as \"Welcome, player! How is your {TimeOfDay} going?\"",
                            "Added MR Demo Scene",
                            "Added MR Automatic Installation and Manual Installation",
                            "Added Convai XR Package (compatibility with Meta SDK and other XR SDKs provided)"
                        }
                    },
                    new()
                    {
                        Title = "Improvements",
                        Changes = new List<string>
                        {
                            "Added Long Term Memory API(s) to View and Delete Speaker ID(s)",
                            "Improved VR Manual Installation",
                            "Improved Custom Package Installation"
                        }
                    }
                }
            }
        };

        public ConvaiUpdatesSection()
        {
            AddToClassList("section-card");
            Add(ConvaiVisualElementUtility.CreateLabel("header", "Updates", "header"));
            Add(ConvaiVisualElementUtility.CreateLabel("subheader", "Current SDK Version", "subheader"));
            Add(ConvaiVisualElementUtility.CreateLabel("current-sdk-version", ConvaiConstants.SDK_VERSION, "label"));
            Add(ConvaiVisualElementUtility.CreateLabel("subheader", "Changelog", "subheader"));
            foreach (Changelog changelog in _changelogs)
            {
                VisualElement card = new() { name = "card" };
                card.Add(ConvaiVisualElementUtility.CreateLabel("sdk-version", changelog.Version, "label"));
                card.Add(ConvaiVisualElementUtility.CreateLabel("release-date", "Released: " + changelog.Date, "helper-text"));
                foreach (Changelog.Section section in changelog.Sections)
                {
                    VisualElement sectionUI = new() { name = "section" };
                    sectionUI.Add(ConvaiVisualElementUtility.CreateLabel("section-title", section.Title + ":", "m-p-zero", "changelog-section"));
                    for (int index = 0; index < section.Changes.Count; index++)
                    {
                        string sectionChange = section.Changes[index];
                        Label label = ConvaiVisualElementUtility.CreateLabel("changes", "•  " + sectionChange, "changelog-item");
                        if (index != section.Changes.Count - 1)
                        {
                            label.style.marginBottom = 5;
                        }

                        sectionUI.Add(label);
                    }

                    card.Add(sectionUI);
                }

                card.AddToClassList("card");

                Add(card);
            }

            Button viewFullChangelogButton = new() { name = "view-full-changelog", text = "View Full Changelog" };
            viewFullChangelogButton.AddToClassList("button-small");
            viewFullChangelogButton.style.alignSelf = Align.Center;
            Add(viewFullChangelogButton);
            viewFullChangelogButton.clicked += () =>
                Application.OpenURL("https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin/changelogs");
        }

        public class Changelog
        {
            public string Date;
            public List<Section> Sections;

            public string Version;

            public class Section
            {
                public List<string> Changes;
                public string Title;
            }
        }
    }
}
