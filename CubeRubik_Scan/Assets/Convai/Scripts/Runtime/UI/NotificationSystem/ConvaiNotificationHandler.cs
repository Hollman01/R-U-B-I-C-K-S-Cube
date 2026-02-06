using System.Linq;
using Convai.Scripts.Configuration;
using Convai.Scripts.LoggerSystem;
using Convai.Scripts.Services;
using UnityEngine;

namespace Convai.Scripts.NotificationSystem
{
    /// <summary>
    ///     Handles the notification system's behavior and interactions.
    /// </summary>
    public class ConvaiNotificationHandler : MonoBehaviour
    {
        [SerializeField] private ConvaiConfigurationDataSO configurationDataSO;

        /// <summary>
        ///     Array containing predefined notification configurations.
        ///     This array can be modified in the Unity Editor to define different types of notifications.
        /// </summary>
        [SerializeField] private SONotificationGroup notificationGroup;

        [SerializeField] private UINotificationController notificationControllerPrefab;

        private UINotificationController _spawnedController;

        private void Awake()
        {
            ConvaiConfigurationDataSO.GetData(out configurationDataSO);
            SONotificationGroup.GetGroup(out notificationGroup);
            _spawnedController = Instantiate(notificationControllerPrefab, transform);
        }


        /// <summary>
        ///     This function is called when the object becomes enabled and active.
        ///     It is used to subscribe to the OnNotificationRequested event.
        /// </summary>
        private void OnEnable()
        {
            ConvaiServices.NotificationService.OnNotificationRequested += NotificationRequest;
            ConvaiServices.NotificationService.OnCustomNotificationRequested += OnCustomNotificationRequest;
        }

        /// <summary>
        ///     This function is called when the behaviour becomes disabled or inactive.
        ///     It is used to unsubscribe from the OnNotificationRequested event.
        /// </summary>
        private void OnDisable()
        {
            ConvaiServices.NotificationService.OnNotificationRequested -= NotificationRequest;
            ConvaiServices.NotificationService.OnCustomNotificationRequested -= OnCustomNotificationRequest;
        }

        /// <summary>
        ///     Requests a notification of the specified type.
        /// </summary>
        /// <param name="notificationType">The type of notification to request.</param>
        private void NotificationRequest(NotificationType notificationType)
        {
            // Check if the notification system is currently active.
            if (!configurationDataSO.NotificationSystemEnabled)
            {
                ConvaiUnityLogger.Info("Cannot sent notification, it's disabled in the config file", LogCategory.UI);
                return;
            }

            // Search for the requested notification type in the predefined array.
            SONotification requestedSONotification =
                notificationGroup.soNotifications.FirstOrDefault(notification => notification.notificationType == notificationType);
            // If the requested notification is not found, log an error.
            if (requestedSONotification == null)
            {
                ConvaiUnityLogger.Error("There is no Notification defined for the selected Notification Type!", LogCategory.UI);
                return;
            }

            _spawnedController.Notify(requestedSONotification);
        }

        private void OnCustomNotificationRequest(SONotification requestedSONotification)
        {
            if (!configurationDataSO.NotificationSystemEnabled)
            {
                ConvaiUnityLogger.Info("Cannot sent notification, it's disabled in the config file", LogCategory.UI);
                return;
            }

            _spawnedController.Notify(requestedSONotification);
        }
    }
}
