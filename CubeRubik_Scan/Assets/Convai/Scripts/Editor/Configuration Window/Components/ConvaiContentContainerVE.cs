using System.Collections.Generic;
using Convai.Editor.Configuration_Window.Components.Sections;
using Convai.Editor.Configuration_Window.Components.Sections.LoggerSettings;
using Convai.Editor.Configuration_Window.Components.Sections.LongTermMemory;
using Convai.Editor.Configuration_Window.Components.Sections.ServerAnimation;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components
{
    [UxmlElement]
    public partial class ConvaiContentContainerVE : VisualElement
    {
        private ScrollView _contentContainer;
        private Dictionary<string, ConvaiBaseSection> _sections;

        public ConvaiContentContainerVE()
        {
            AddToClassList("content-container");
            CreateContentContainer();
            CreateAndInjectSections();
        }

        private void CreateContentContainer()
        {
            _contentContainer = new ScrollView { name = "content-container" };
            Add(_contentContainer);
        }

        private void CreateAndInjectSections()
        {
            _sections = new Dictionary<string, ConvaiBaseSection>
            {
                { ConvaiWelcomeSection.SECTION_NAME, new ConvaiWelcomeSection() },
                { ConvaiAccountSection.SECTION_NAME, new ConvaiAccountSection() },
                // { ConvaiPackageManagementSection.SECTION_NAME, new ConvaiPackageManagementSection() },
                { ConvaiUpdatesSection.SECTION_NAME, new ConvaiUpdatesSection() },
                // { ConvaiDocumentationSection.SECTION_NAME, new ConvaiDocumentationSection() },
                { ConvaiContactSection.SECTION_NAME, new ConvaiContactSection() },
                { ConvaiLongTermMemorySection.SECTION_NAME, new ConvaiLongTermMemorySection() },
                { ConvaiLoggerSettingSection.SECTION_NAME, new ConvaiLoggerSettingSection() },
                { ConvaiServerAnimationSection.SECTION_NAME, new ConvaiServerAnimationSection() }
            };

            foreach (KeyValuePair<string, ConvaiBaseSection> pair in _sections)
            {
                _contentContainer.Add(pair.Value);
            }
        }

        public void OpenSection(string sectionName)
        {
            foreach (KeyValuePair<string, ConvaiBaseSection> pair in _sections)
            {
                if (pair.Key == sectionName)
                {
                    pair.Value.ShowSection();
                }
                else
                {
                    pair.Value.HideSection();
                }
            }
        }
    }
}
