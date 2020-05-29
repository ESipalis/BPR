using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonServices.DetectionSystemServices.KommuneService;
using CommonServices.DetectionSystemServices.Storage;
using Data;
using KommuneNotificationModels;
using Microsoft.Extensions.Logging;

namespace CommonServices.DetectionSystemServices
{
    public class DetectionSystemService
    {
        private readonly IStorage _storage;
        private readonly IKommuneService _kommuneService;
        private readonly ILogger<DetectionSystemService> _logger;

        public DetectionSystemService(IStorage storage, IKommuneService kommuneService)
        {
            _storage = storage;
            _kommuneService = kommuneService;
        }

        public async Task ResendFailedNotifications()
        {
            List<Notification> failedNotifications = await _storage.GetFailedNotifications();
            List<NotificationToKommune> notificationsToKommune = DetectionSystemServiceUtil.NotificationsToKommuneNotifications(failedNotifications);
            try
            {
                await _kommuneService.SendNotifications(notificationsToKommune);
                await _storage.SetNotificationsAsSent(notificationsToKommune.Select(x => x.NotificationId));
            }
            catch (KommuneCommunicationException e)
            {
                _logger.LogWarning("Kommune communication exception", e);
            }
        }

        public async Task RefreshAndSendDeviceStatuses()
        {
            await _storage.RefreshDeviceStatuses();
            List<UnsentDeviceStatus> unsentDeviceStatuses = await _storage.GetUnsentDeviceStatuses();
            List<DeviceStatusNotification> deviceStatusNotifications = unsentDeviceStatuses.Select(x => new DeviceStatusNotification
            {
                Address = x.Address,
                Timestamp = x.Timestamp,
                DeviceEui = x.DeviceEui,
                DeviceUnresponsive = x.DeviceUnresponsive,
                NotificationId = x.NotificationId
            }).ToList();
            try
            {
                await _kommuneService.SendNotifications(deviceStatusNotifications);
                await _storage.SetDeviceStatusesAsSent(deviceStatusNotifications.Select(x => x.DeviceEui));
            }
            catch (KommuneCommunicationException e)
            {
                _logger.LogWarning("Kommune communication exception", e);
            }
        }

        public async Task SendAndSaveNotifications(IEnumerable<Notification> notifications)
        {
            List<Notification> notificationList = notifications.ToList();
            List<NotificationToKommune> notificationsToKommune = DetectionSystemServiceUtil.NotificationsToKommuneNotifications(notificationList);
            try
            {
                await _kommuneService.SendNotifications(notificationsToKommune);
                foreach (Notification notification in notificationList)
                {
                    notification.ObjectDetectionNotification.SentToKommune = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("Kommune communication exception", e);
                foreach (Notification notification in notificationList)
                {
                    notification.ObjectDetectionNotification.SentToKommune = false;
                }
            }
            await _storage.AddNotifications(notificationList);
        }

        public async Task RegisterDevices(IEnumerable<Device> devices)
        {
            await _storage.AddDevices(devices);
        }

        public async Task ConfigureDevices(IEnumerable<ConfigureDevice> configureDevices)
        {
            await _storage.UpdateDeviceConfigurations(configureDevices);
        }

        public async Task SetDeviceConfigurationStatus(string deviceEui, ConfigurationStatus configurationStatus)
        {
            await _storage.UpdateDeviceConfigurationStatus(deviceEui, configurationStatus);
        }
    }
}