using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components
{
    public static class ConvaiVisualElementUtility
    {
        public static void AddStyles(VisualElement visualElement, params string[] styles)
        {
            foreach (string s in styles)
            {
                visualElement.AddToClassList(s);
            }
        }

        public static Label CreateLabel(string labelName, string content, params string[] styles)
        {
            Label label = new() { text = content, name = labelName };
            AddStyles(label, styles);
            return label;
        }


        public static void ModifyMargin(VisualElement element, float up, float down)
        {
            element.style.marginTop = up;
            element.style.marginBottom = down;
        }

        public static void ModifyPadding(VisualElement element, float up, float down)
        {
            element.style.paddingTop = up;
            element.style.paddingBottom = down;
        }

        public static VisualElement CreateSpacer(float height) =>
            new() { name = "spacer", style = { height = height } };
    }
}
