using System;
using Convai.RestAPI.Internal;
using Convai.Scripts.Editor.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
// using Convai.SDK.CSharp.RestAPI.Internal;

namespace Convai.Editor.Configuration_Window.Components.Sections.ServerAnimation
{
    [UxmlElement]
    public partial class ConvaiServerAnimationSection : ConvaiBaseSection
    {
        public const string SECTION_NAME = "server-animation";

        public ConvaiServerAnimationSection()
        {
            AddToClassList("section-card");
            CreateHeader();
            CreateAnimationContainer();
            CreateNavigationRow();
            _ = new ServerAnimationLogic(this);
        }

        public Button RefreshButton { get; private set; }
        public Button PreviousButton { get; private set; }
        public Button NextButton { get; private set; }
        public Button ImportButton { get; private set; }
        public ScrollView AnimationContainer { get; private set; }

        private void CreateNavigationRow()
        {
            VisualElement navigationRow = new()
            {
                name = "navigation-row",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.SpaceBetween,
                    alignItems = Align.Center,
                    marginTop = 10,
                    marginBottom = 10
                }
            };

            // Create inner row for Previous and Next buttons
            VisualElement innerRow = new()
            {
                name = "inner-navigation-row",
                style =
                {
                    flexDirection = FlexDirection.Row,
                    justifyContent = Justify.FlexStart,
                    alignItems = Align.Center // Space between Previous and Next buttons
                }
            };

            PreviousButton = new Button { name = "previous-btn", text = "Previous", style = { width = 100 } };

            NextButton = new Button { name = "next-btn", text = "Next", style = { width = 100 } };

            ImportButton = new Button { name = "import-btn", text = "Import", style = { width = 100 } };

            // Style
            PreviousButton.AddToClassList("button");
            NextButton.AddToClassList("button");
            ImportButton.AddToClassList("button");
            PreviousButton.style.width = 150;
            NextButton.style.width = 150;
            ImportButton.style.width = 150;

            // Add Previous and Next to inner row
            innerRow.Add(PreviousButton);
            innerRow.Add(NextButton);

            // Add inner row and Import button to main navigation row
            navigationRow.Add(innerRow);
            navigationRow.Add(ImportButton);

            Add(navigationRow);
        }

        private void CreateAnimationContainer()
        {
            AnimationContainer = new ScrollView { style = { height = 340, marginBottom = 10 } };

            AnimationContainer.contentContainer.style.flexDirection = FlexDirection.Row;
            AnimationContainer.contentContainer.style.flexWrap = Wrap.Wrap;
            Add(AnimationContainer);
        }

        private void CreateHeader()
        {
            // Creation
            VisualElement headerRow = new()
            {
                name = "header-row",
                style = { flexDirection = FlexDirection.Row, justifyContent = Justify.SpaceBetween, alignItems = Align.Center }
            };
            RefreshButton = new Button { name = "refresh-btn", text = "Refresh" };

            // Style
            RefreshButton.AddToClassList("button");
            RefreshButton.style.width = 150;


            // Hierarchy addition
            headerRow.Add(ConvaiVisualElementUtility.CreateLabel("header", "Server Animation", "header"));
            headerRow.Add(RefreshButton);
            Add(headerRow);
        }
    }

    public class ConvaiServerAnimationItem : VisualElement
    {
        private readonly StyleColor _selectedBorderColor = new(new Color(11f / 255, 96f / 255, 73f / 255));
        private readonly StyleColor _unselectedBorderColor = new(new Color(0, 0, 0, 0.25f));

        private bool _isSelected;

        public ConvaiServerAnimationItem(Action<bool, ServerAnimationItemResponse> onSelectedChanged, ServerAnimationItemResponse animation)
        {
            AddToClassList("server-animation-card");
            CreateThumbnail();
            CreateName();
            IsSelected = false;
            CanBeSelected = true;
            RegisterCallback<ClickEvent>(_ =>
            {
                if (!CanBeSelected)
                {
                    return;
                }

                IsSelected = !IsSelected;
                onSelectedChanged?.Invoke(IsSelected, animation);
            });
        }

        public Image Thumbnail { get; private set; }
        public Label Name { get; private set; }

        public bool CanBeSelected { get; set; } = true;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                StyleColor borderColor = _isSelected ? _selectedBorderColor : _unselectedBorderColor;
                style.borderBottomColor = borderColor;
                style.borderTopColor = borderColor;
                style.borderLeftColor = borderColor;
                style.borderRightColor = borderColor;
            }
        }

        private void CreateName()
        {
            Name = ConvaiVisualElementUtility.CreateLabel("name", "Animation Name", "server-animation-card-label");
            Add(Name);
        }

        private void CreateThumbnail()
        {
            Thumbnail = new Image
            {
                style =
                {
                    backgroundImage =
                        new StyleBackground(
                            AssetDatabase.LoadAssetAtPath<Texture2D>(ConvaiImagesDirectory.SERVER_ANIMATION_CARD_THUMBNAIL_PATH))
                }
            };
            Add(Thumbnail);
        }
    }
}
