using System;
using Convai.Scripts.NotificationSystem;

namespace Convai.Scripts.Services.NotificationSystem
{
    public class ConvaiNotificationService
    {
        public event Action<NotificationType> OnNotificationRequested = delegate { };
        public event Action<SONotification> OnCustomNotificationRequested = delegate { };

        public void RequestNotification(NotificationType notificationType) => OnNotificationRequested?.Invoke(notificationType);

        public void RequestCustomNotification(SONotification notification) => OnCustomNotificationRequested?.Invoke(notification);
    }
}
