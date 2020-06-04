using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
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

        public DetectionSystemService(IStorage storage, IKommuneService kommuneService, ILogger<DetectionSystemService> logger)
        {
            _storage = storage;
            _kommuneService = kommuneService;
            _logger = logger;
        }

        public async Task ResendFailedNotifications()
        {
            List<Notification> failedNotifications = await _storage.GetFailedNotifications();
            if (!failedNotifications.Any())
            {
                return;
            }

            List<NotificationToKommune> notificationsToKommune = DetectionSystemServiceUtil.NotificationsToKommuneNotifications(failedNotifications);
            try
            {
                await _kommuneService.SendNotifications(notificationsToKommune);
                await _storage.SetNotificationStatuses(notificationsToKommune.Select(x => x.NotificationId), true);
            }
            catch (KommuneCommunicationException e)
            {
                _logger.LogWarning("Kommune communication exception", e);
            }
        }

        public async Task<Device> GetDevice(string deviceEui)
        {
            return await _storage.GetDevice(deviceEui);
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
            _logger.LogInformation("Sending: " + JsonSerializer.Serialize(deviceStatusNotifications));
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

        public async Task SendAndSaveNotifications(IEnumerable<UplinkMessage> uplinkMessageEnumerable)
        {
            List<UplinkMessage> uplinkMessages = uplinkMessageEnumerable.ToList();
            List<Notification> notifications = uplinkMessages.Select(async uplinkMessage =>
            {
                NotificationType notificationType;
                ObjectDetectionNotification objectDetectionNotification = null;
                if (uplinkMessage.Data.Length == 0)
                {
                    notificationType = NotificationType.Heartbeat;
                }
                else
                {
                    notificationType = NotificationType.ObjectDetection;
                    ushort? widthCentimeters = ushort.Parse(uplinkMessage.Data, NumberStyles.HexNumber);
                    ObjectDetection objectDetection = widthCentimeters switch
                    {
                        0 => ObjectDetection.Removed,
                        ushort.MaxValue => ObjectDetection.Detected,
                        _ => ObjectDetection.DetectedWithSize
                    };
                    if (objectDetection != ObjectDetection.DetectedWithSize)
                    {
                        widthCentimeters = null;
                    }

                    objectDetectionNotification = new ObjectDetectionNotification
                    {
                        ObjectDetection = objectDetection,
                        WidthCentimeters = widthCentimeters,
                        SentToKommune = false
                    };
                }

                string address = (await _storage.GetDevice(uplinkMessage.DeviceEui))?.Address ?? "";
                return new Notification
                {
                    Address = address,
                    Timestamp = uplinkMessage.Timestamp,
                    Type = notificationType,
                    DeviceEui = uplinkMessage.DeviceEui,
                    ObjectDetectionNotification = objectDetectionNotification
                };
            })
                .Select(task => task.Result)
                .ToList();
            _logger.LogInformation("Saving notifications: " + JsonSerializer.Serialize(notifications));
            await _storage.AddNotifications(notifications);
            

            List<NotificationToKommune> notificationsToKommune = DetectionSystemServiceUtil.NotificationsToKommuneNotifications(notifications);
            try
            {
                await _kommuneService.SendNotifications(notificationsToKommune);
                foreach (Notification notification in notifications)
                {
                    notification.ObjectDetectionNotification.SentToKommune = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("Kommune communication exception", e);
                foreach (Notification notification in notifications)
                {
                    notification.ObjectDetectionNotification.SentToKommune = false;
                }
            }

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