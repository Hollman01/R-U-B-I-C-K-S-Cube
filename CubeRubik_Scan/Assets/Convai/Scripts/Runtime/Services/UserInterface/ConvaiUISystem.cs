using System;

namespace Convai.Scripts.Services.UserInterface
{
    public class ConvaiUISystem
    {
        public event System.Action OnSettingsOpened = delegate { };
        public event System.Action OnSettingsClosed = delegate { };
        public event Action<int> OnPreviewStyle = delegate { };

        public void ShowSettings() => OnSettingsOpened();
        public void HideSettings() => OnSettingsClosed();

        public void PreviewStyle(int index) => OnPreviewStyle(index);
    }
}
