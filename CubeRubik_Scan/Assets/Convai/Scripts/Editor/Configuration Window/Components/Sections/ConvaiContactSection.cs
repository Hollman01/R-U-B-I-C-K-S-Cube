using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections
{
    [UxmlElement]
    public partial class ConvaiContactSection : ConvaiBaseSection
    {
        public const string SECTION_NAME = "contact-us";
        private const string SUBHEADER = "Have questions or need assistance?";

        private const string INSTRUCTIONS =
            "Visit the Convai Developer Forum – our dedicated community and support team are here to help with bug reports and technical queries. Get the support you need and connect with other developers to enhance your experience.";

        public ConvaiContactSection()
        {
            AddToClassList("section-card");
            Add(ConvaiVisualElementUtility.CreateLabel("header", "Need Help?", "header"));
            Add(ConvaiVisualElementUtility.CreateLabel("subheader", SUBHEADER, "subheader"));
            Label instruction = ConvaiVisualElementUtility.CreateLabel("instruction", INSTRUCTIONS, "helper-text");
            Add(instruction);
            Add(new VisualElement { style = { height = 15 } });
            Button button = new() { name = "developer-forum-btn", text = "Visit Developer Forum" };
            button.AddToClassList("button-small");
            button.AddToClassList("margin-small");
            button.style.alignSelf = Align.FlexStart;
            Add(button);

            instruction.style.whiteSpace = WhiteSpace.Normal;
        }
    }
}
