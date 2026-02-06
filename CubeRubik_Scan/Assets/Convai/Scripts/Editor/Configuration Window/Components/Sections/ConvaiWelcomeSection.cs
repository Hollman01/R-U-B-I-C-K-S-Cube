using UnityEngine;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections
{
    [UxmlElement]
    public partial class ConvaiWelcomeSection : ConvaiBaseSection
    {
        public const string SECTION_NAME = "welcome";

        private const string WELCOME_TO_CONVAI_SDK = "Welcome to Convai SDK";
        private const string ABOUT = "About Convai";

        private const string COMPANY_INFO =
            "Convai enables AI characters in games and virtual worlds to have human-like conversation capabilities and more. <br <br With Convai, developers and designers like you can add a backstory, knowledge base, voice, and overall intelligence to your characters to converse naturally with players and carry out actions.                        <br Convai focuses on bringing characters that are as life-like as possible and can be directed by any developer! <br <br This SDK contains the complete Convai conversation pipeline, which includes Speech Recognition, Language Understanding and Generation, Text-to-Speech, Text-to-Action, Character Lipsync, Action capability, Narrative Design and much more. ";

        private const string QUICK_START_GUIDE = "Quick Start Guide";
        private const string GET_TO_KNOW = "Now that you have downloaded the SDK, lets get to know it quickly first";

        private const string INSTRUCTIONS =
            "1. Read the friendly documentation first to get an in-depth overview of the Convai SDK.  <br 2. Go to the 'Account' tab and initialize the SDK with your API key.";

        public ConvaiWelcomeSection()
        {
            AddToClassList("section-card");
            Add(ConvaiVisualElementUtility.CreateLabel("header", WELCOME_TO_CONVAI_SDK, "header"));
            Add(ConvaiVisualElementUtility.CreateLabel("about", ABOUT, "subheader"));
            Add(ConvaiVisualElementUtility.CreateLabel("info", COMPANY_INFO, "mb-4"));
            Add(ConvaiVisualElementUtility.CreateLabel("quick-start-guide", QUICK_START_GUIDE, "subheader"));
            Add(ConvaiVisualElementUtility.CreateLabel("get-to-know", GET_TO_KNOW, "mb-2", "bold"));
            Add(ConvaiVisualElementUtility.CreateLabel("instructions", INSTRUCTIONS, "mb-4"));
            AddDocumentationButton();
        }

        private void AddDocumentationButton()
        {
            Button documentation = new() { text = "The Friendly Documentation" };
            documentation.clicked += () => Application.OpenURL("https://docs.convai.com/api-docs/plugins-and-integrations/unity-plugin");
            ConvaiVisualElementUtility.AddStyles(documentation, "button", "padding-medium");
            documentation.style.alignSelf = Align.FlexStart;
            Add(documentation);
        }
    }
}
