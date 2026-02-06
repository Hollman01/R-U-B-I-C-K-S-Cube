using Convai.Editor.Configuration_Window.Components.Sections;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components
{
    [UxmlElement]
    public partial class ConvaiConfigurationWindow : VisualElement
    {
        private readonly ConvaiContentContainerVE _contentContainer;

        private readonly ConvaiNavigationBarVE _navigation;

        [UxmlAttribute]
        public string InitialSection { get; set; } = ConvaiWelcomeSection.SECTION_NAME;


        public ConvaiConfigurationWindow()
        {
            _navigation = new ConvaiNavigationBarVE();
            _contentContainer = new ConvaiContentContainerVE();
            AddToClassList("root");
            Add(_navigation);
            Add(_contentContainer);
            OpenSection(InitialSection);
            _navigation.OnNavigationButtonClicked += OpenSection;
        }

        ~ConvaiConfigurationWindow() => _navigation.OnNavigationButtonClicked -= OpenSection;

        public void OpenSection(string sectionName)
        {
            _navigation.NavigateTo(sectionName);
            _contentContainer.OpenSection(sectionName);
        }
    }
}
