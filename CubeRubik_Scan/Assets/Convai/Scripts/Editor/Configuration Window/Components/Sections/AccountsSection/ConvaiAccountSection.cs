using Convai.Editor.Configuration_Window.Components.Sections.AccountsSection.APIKeySetup;
using Convai.Editor.Configuration_Window.Components.Sections.AccountsSection.UserAccountInformation;
using UnityEngine.UIElements;
using FontStyle = UnityEngine.FontStyle;

namespace Convai.Editor.Configuration_Window.Components.Sections
{
    [UxmlElement]
    public partial class ConvaiAccountSection : ConvaiBaseSection
    {
        public const string SECTION_NAME = "account";
        private const string HELPER_TEXT = "Enter your Convai API key to authenticate your requests.";


        public ConvaiAccountSection()
        {
            AddToClassList("section-card");
            Add(ConvaiVisualElementUtility.CreateLabel("section-header", "Account Settings", "header"));
            CreateAPIKeyCard();
            CreateAccountInformationCard();
            AccountInformationUI accountInformationUI = new(this);
            _ = new ConvaiAccountSectionLogic(this, accountInformationUI);
        }


        public TextField APIInputField { get; private set; }
        public Button ShowHideAPIKeyButton { get; private set; }
        public Button UpdateSaveButton { get; private set; }

        public Label PlanName { get; private set; }
        public Label ExpiryDate { get; private set; }
        public InteractionUI DailyInteractionUI { get; private set; }
        public InteractionUI MonthlyInteractionUI { get; private set; }

        private void CreateAPIKeyCard()
        {
            VisualElement card = new() { name = "api-key-config-card" };

            Label subheader = ConvaiVisualElementUtility.CreateLabel("api-key-config", "API Key Configuration", "subheader");
            Label apiInputFieldHeader = ConvaiVisualElementUtility.CreateLabel("api-field-title", "API Key", "label");

            VisualElement inputFieldContainer = new() { name = "api-input-field-container", style = { marginBottom = 20 } };

            VisualElement row = new() { name = "row", style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };
            APIInputField = new TextField
            {
                style = { width = new StyleLength(new Length(85, LengthUnit.Percent)), fontSize = 25, unityFontStyleAndWeight = FontStyle.Bold },
                isPasswordField = true,
                maskChar = '●'
            };

            ShowHideAPIKeyButton = new Button { name = "api-update-save-btn", text = "Show" };

            Label helperText = ConvaiVisualElementUtility.CreateLabel("helper-text", HELPER_TEXT, "helper-text");

            UpdateSaveButton = new Button { name = "save-update-button", text = "Save API Key" };

            card.AddToClassList("card");
            ShowHideAPIKeyButton.AddToClassList("button");
            ShowHideAPIKeyButton.style.flexGrow = 1;
            ConvaiVisualElementUtility.AddStyles(UpdateSaveButton, "button", "btn-medium", "padding-medium");
            UpdateSaveButton.style.alignSelf = Align.Center;


            row.Add(APIInputField);
            row.Add(ShowHideAPIKeyButton);
            inputFieldContainer.Add(apiInputFieldHeader);
            inputFieldContainer.Add(row);
            inputFieldContainer.Add(helperText);

            card.Add(subheader);
            card.Add(inputFieldContainer);
            card.Add(UpdateSaveButton);

            Add(card);
        }

        private void CreateAccountInformationCard()
        {
            // Creation
            VisualElement card = new() { name = "account-information" };

            Label subheader = ConvaiVisualElementUtility.CreateLabel("sub-header", "Account Information", "subheader");

            VisualElement row = new() { name = "row", style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween } };

            PlanName = ConvaiVisualElementUtility.CreateLabel("plan-name", "{Plan-Name}", "label", "m-p-zero");
            ExpiryDate = ConvaiVisualElementUtility.CreateLabel("expiry-date", "{Expiry-Data}", "m-p-zero");

            DailyInteractionUI = new InteractionUI("daily-interactions", "Daily Usage");
            MonthlyInteractionUI = new InteractionUI("monthly-interactions", "Monthly Usage");


            // Style Addition
            card.AddToClassList("card");
            row.AddToClassList("margin-small");
            PlanName.style.marginBottom = 0;


            // Hierarchy Addition
            Add(card);
            card.Add(subheader);
            row.Add(PlanName);
            row.Add(ExpiryDate);
            card.Add(row);
            card.Add(ConvaiVisualElementUtility.CreateSpacer(20));
            card.Add(DailyInteractionUI.Container);
            card.Add(ConvaiVisualElementUtility.CreateSpacer(10));
            card.Add(MonthlyInteractionUI.Container);
        }

        public class InteractionUI
        {
            public readonly VisualElement Container;
            public readonly Label InteractionLiteral;
            public readonly ProgressBar ProgressBar;

            public InteractionUI(string name, string title)
            {
                Container = new VisualElement { name = $"{name}-container" };

                Label header = ConvaiVisualElementUtility.CreateLabel(name, title, "helper-text");
                ProgressBar = new ProgressBar();
                InteractionLiteral = ConvaiVisualElementUtility.CreateLabel($"{name}-literal", "0/0 interactions", "helper-text");
                ConvaiVisualElementUtility.AddStyles(ProgressBar, "usage-bar");
                Container.Add(header);
                Container.Add(ProgressBar);
                Container.Add(InteractionLiteral);
            }
        }
    }
}
