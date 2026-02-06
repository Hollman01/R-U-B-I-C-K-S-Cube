using UnityEngine.UIElements;

namespace Convai.Editor.Configuration_Window.Components.Sections
{
    public class ConvaiBaseSection : VisualElement
    {
        public void HideSection() => style.display = DisplayStyle.None;

        public void ShowSection() => style.display = DisplayStyle.Flex;
    }
}
