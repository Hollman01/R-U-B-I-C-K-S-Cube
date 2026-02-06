using Convai.Scripts.Services.CharacterLocator;
using Convai.Scripts.Services.NotificationSystem;
using Convai.Scripts.Services.Permissions;
using Convai.Scripts.Services.TranscriptSystem;
using Convai.Scripts.Services.UserInterface;

namespace Convai.Scripts.Services
{
    public class ConvaiServices
    {
        private static ConvaiServices _instance;
        private readonly ConvaiCharacterLocatorService _characterLocatorService = new();
        private readonly ConvaiNotificationService _notificationService = new();
        private readonly ConvaiPermissionService _permissionService = new();
        private readonly ConvaiTranscriptService _transcriptService = new();
        private readonly ConvaiUISystem _uiSystem = new();

        private static ConvaiServices Instance
        {
            get
            {
                _instance ??= new ConvaiServices();
                return _instance;
            }
        }

        public static ConvaiTranscriptService TranscriptService => Instance._transcriptService;
        public static ConvaiCharacterLocatorService CharacterLocatorService => Instance._characterLocatorService;
        public static ConvaiPermissionService PermissionService => Instance._permissionService;
        public static ConvaiUISystem UISystem => Instance._uiSystem;
        public static ConvaiNotificationService NotificationService => Instance._notificationService;
    }
}
