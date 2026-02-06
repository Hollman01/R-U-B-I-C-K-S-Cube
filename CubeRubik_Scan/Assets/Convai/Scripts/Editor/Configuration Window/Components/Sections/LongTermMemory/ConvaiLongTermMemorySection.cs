using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections.LongTermMemory
{
    [UxmlElement]
    public partial class ConvaiLongTermMemorySection : ConvaiBaseSection
    {
        public const string SECTION_NAME = "ltm";

        public ConvaiLongTermMemorySection()
        {
            AddToClassList("section-card");
            CreateHeader();
            IDContainer = new ScrollView { style = { height = 260, marginBottom = 10 } };
            TableTitle = ConvaiVisualElementUtility.CreateLabel("title", "No Speaker ID Found", "label");
            DeleteButton = new Button { name = "delete-button", text = "Delete" };

            DeleteButton.AddToClassList("button-small");
            DeleteButton.style.alignSelf = Align.FlexStart;
            DeleteButton.SetEnabled(false);

            // Add(new DisclaimerBox()); // Hidden
            Add(TableTitle);
            Add(IDContainer);
            Add(DeleteButton);

            _ = new LongTermMemoryLogic(this);
        }

        public Button RefreshButton { get; private set; }
        public ScrollView IDContainer { get; }
        public Label TableTitle { get; }
        public Button DeleteButton { get; }

        private void CreateHeader()
        {
            //Creation
            VisualElement headerRow = new()
            {
                name = "header-row",
                style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center }
            };
            VisualElement headerLabel = ConvaiVisualElementUtility.CreateLabel("header", "Long Term Memory", "header");
            RefreshButton = new Button { name = "refresh-btn", text = "Refresh" };


            //Style
            RefreshButton.AddToClassList("button-small");
            ConvaiVisualElementUtility.ModifyMargin(headerLabel, 0, 0);
            ConvaiVisualElementUtility.ModifyMargin(RefreshButton, 0, 0);
            ConvaiVisualElementUtility.ModifyMargin(headerRow, 0, 16);

            //Hierarchy addition
            headerRow.Add(headerLabel);
            headerRow.Add(RefreshButton);
            Add(headerRow);
        }
    }

    internal class DisclaimerBox : VisualElement
    {
        private const string DISCLAIMER =
            "The number of Speaker IDs that can be created per API key is limited and varies based on the subscription tier. To optimize your usage and avoid exceeding your limit, it is recommended to regularly delete any unused Speaker IDs.";

        private readonly Dictionary<string, string> _limits = new()
        {
            { "Free", "1" }, { "Gamer / Indie / Professional", "5" }, { "Partner / Enterprise", "100(Can be Customized)" }
        };

        private VisualElement _disclaimerContainer;
        private bool _isOpen;

        private Button _toggleDisclaimerVisibilityButton;

        public DisclaimerBox()
        {
            name = "disclaimer-box";

            // Styling
            AddToClassList("card");

            // Hierarchy Addition
            CreateTitle();
            CreateDisclaimerContainer();
            _isOpen = true;
            Toggle();

            _toggleDisclaimerVisibilityButton.clicked += Toggle;
        }


        private void CreateDisclaimerContainer()
        {
            _disclaimerContainer = new VisualElement { name = "disclaimer-container" };
            Label disclaimerLabel = ConvaiVisualElementUtility.CreateLabel("disclaimer", DISCLAIMER, "helper-text");
            Label tableTitle = ConvaiVisualElementUtility.CreateLabel("table-title", "Per-Tier Limit", "label");

            ConvaiVisualElementUtility.ModifyMargin(_disclaimerContainer, 10, 0);
            disclaimerLabel.style.whiteSpace = WhiteSpace.Normal;


            _disclaimerContainer.Add(disclaimerLabel);
            _disclaimerContainer.Add(ConvaiVisualElementUtility.CreateSpacer(10));
            _disclaimerContainer.Add(tableTitle);
            CreateTable();
            Add(_disclaimerContainer);
        }

        private void CreateTable()
        {
            VisualElement wrapper = new();

            Label tier = ConvaiVisualElementUtility.CreateLabel("tier", "Tier", "label");
            Label limit = ConvaiVisualElementUtility.CreateLabel("tier", "Limit", "label");
            tier.style.borderBottomWidth = limit.style.borderBottomWidth = 2;
            tier.style.borderBottomColor = limit.style.borderBottomColor = new StyleColor(new Color(0, 0, 0, 0.22f));
            wrapper.AddToClassList("m-p-0");
            wrapper.AddToClassList("ltm-table");
            wrapper.Add(CreateRow(tier, limit));


            int counter = 0;
            foreach (KeyValuePair<string, string> valuePair in _limits)
            {
                Label left = ConvaiVisualElementUtility.CreateLabel("tier", valuePair.Key, "helper-text");
                Label right = ConvaiVisualElementUtility.CreateLabel("tier", valuePair.Value, "helper-text");
                if (counter != _limits.Count - 1)
                {
                    left.style.borderBottomWidth = right.style.borderBottomWidth = 2;
                    left.style.borderBottomColor = right.style.borderBottomColor = new StyleColor(new Color(0, 0, 0, 0.22f));
                }

                wrapper.Add(CreateRow(left, right));
                counter++;
            }

            _disclaimerContainer.Add(wrapper);
        }

        private VisualElement CreateRow(VisualElement left, VisualElement right)
        {
            VisualElement row = new()
            {
                name = "row", style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center }
            };
            ConvaiVisualElementUtility.ModifyMargin(left, 0, 0);
            ConvaiVisualElementUtility.ModifyMargin(right, 0, 0);
            ConvaiVisualElementUtility.ModifyPadding(left, 5, 5);
            ConvaiVisualElementUtility.ModifyPadding(right, 5, 5);
            left.style.borderRightWidth = 2;
            left.style.borderRightColor = new StyleColor(new Color(0, 0, 0, 0.22f));
            left.style.width = right.style.width = new Length(50, LengthUnit.Percent);
            right.style.unityTextAlign = TextAnchor.MiddleRight;
            left.style.paddingLeft = right.style.paddingRight = 10;

            row.Add(left);
            row.Add(right);
            return row;
        }

        private void CreateTitle()
        {
            VisualElement headerRow = new()
            {
                name = "header-row",
                style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center }
            };
            Label disclaimerTitle = ConvaiVisualElementUtility.CreateLabel("disclaimer-label", "Disclaimer", "subheader");
            _toggleDisclaimerVisibilityButton = new Button { name = "toggle-disclaimer-btn", text = "Show" };


            _toggleDisclaimerVisibilityButton.AddToClassList("button-small");
            ConvaiVisualElementUtility.ModifyMargin(disclaimerTitle, 0, 0);
            ConvaiVisualElementUtility.ModifyMargin(_toggleDisclaimerVisibilityButton, 0, 0);

            headerRow.Add(disclaimerTitle);
            headerRow.Add(_toggleDisclaimerVisibilityButton);

            Add(headerRow);
        }

        private void Toggle()
        {
            _isOpen = !_isOpen;
            _disclaimerContainer.style.display = _isOpen ? DisplayStyle.Flex : DisplayStyle.None;
            _toggleDisclaimerVisibilityButton.text = _isOpen ? "Hide" : "Show";
        }
    }

    internal class LTMItemUI : VisualElement
    {
        private readonly Action<bool, string> _onToggle;

        public LTMItemUI(string playerName, string speakerID, Action<bool, string> onToggle)
        {
            AddToClassList("card");
            PlayerName = playerName;
            SpeakerID = speakerID;
            _onToggle = onToggle;
            Toggle toggleSelectionButton = new() { name = "selection-btn" };
            toggleSelectionButton.RegisterValueChangedCallback(OnToggleValueChanged);
            VisualElement container = new() { name = "container", style = { marginLeft = 10 } };
            Label nameLabel = ConvaiVisualElementUtility.CreateLabel("name", $"Name: {PlayerName}", "label");
            Label id = ConvaiVisualElementUtility.CreateLabel("id", $"Speaker ID: {SpeakerID}", "helper-text");

            ConvaiVisualElementUtility.ModifyMargin(nameLabel, 0, 0);
            style.flexDirection = FlexDirection.Row;
            style.marginBottom = 5;

            container.Add(nameLabel);
            container.Add(id);
            Add(toggleSelectionButton);
            Add(container);
        }

        private string PlayerName { get; }
        private string SpeakerID { get; }

        private void OnToggleValueChanged(ChangeEvent<bool> evt) => _onToggle?.Invoke(evt.newValue, SpeakerID);
    }
}
