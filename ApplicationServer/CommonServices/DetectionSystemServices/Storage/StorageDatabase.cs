using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using DataEFCore;
using Microsoft.EntityFrameworkCore;

namespace CommonServices.DetectionSystemServices.Storage
{
    public class StorageDatabase : IStorage
    {
        private readonly DetectionSystemDbContext _context;

        public StorageDatabase(DetectionSystemDbContext context)
        {
            _context = context;
        }

        public async Task AddNotifications(IEnumerable<Notification> notifications)
        {
            _context.Notification.AddRange(notifications);
            await _context.SaveChangesAsync();
        }


        public async Task<List<Notification>> GetFailedNotifications()
        {
            return await _context.Notification
                .Where(notification => notification.ObjectDetectionNotification != null && notification.ObjectDetectionNotification.SentToKommune == false)
                .ToListAsync();
        }

        public async Task AddDevices(IEnumerable<Device> devices)
        {
            _context.Device.AddRange(devices);
            await _context.SaveChangesAsync();
        }

        public async Task SetNotificationsAsSent(IEnumerable<int> notificationIds)
        {
            List<Notification> notifications = await _context.Notification.Where(notification => notificationIds.Contains(notification.NotificationId)).ToListAsync();
            foreach (Notification notification in notifications)
            {
                notification.ObjectDetectionNotification.SentToKommune = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateDeviceConfigurations(IEnumerable<ConfigureDevice> configurations)
        {
            foreach (ConfigureDevice configureDevice in configurations)
            {
                Device device = await _context.Device.FindAsync(configureDevice.DeviceEui);
                device.Configuration.HeartbeatPeriodDays = configureDevice.Configuration.HeartbeatPeriodDays;
                device.Configuration.ScanMinuteOfTheDay = configureDevice.Configuration.ScanMinuteOfTheDay;
                device.Configuration.Status = configureDevice.Configuration.Status;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateDeviceConfigurationStatus(string deviceEui, ConfigurationStatus configurationStatus)
        {
            Device device = await _context.Device.FindAsync(deviceEui);
            device.Configuration.Status = configurationStatus;
            await _context.SaveChangesAsync();
        }

        public async Task SetDeviceStatusesAsSent(IEnumerable<string> deviceEuis)
        {
            foreach (string deviceEui in deviceEuis)
            {
                Device device = await _context.Device.FindAsync(deviceEui);
                device.Status.SentToKommune = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task RefreshDeviceStatuses()
        {
            List<Device> devices = await _context.Device.ToListAsync();
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (Device device in devices)
            {
                bool deviceHasNotifications = await _context.Notification
                    .Where(notification => notification.DeviceEui == device.DeviceEui)
                    .AnyAsync();
                if (!deviceHasNotifications) // If device has not sent any notifications at all, do not refresh its status.
                {
                    continue;
                }


                double deviceHeartbeatPeriodInSeconds = (device.Configuration.HeartbeatPeriodDays + 1) * 86400;
                bool anyRecentNotifications = await _context.Notification
                    .Where(notification => notification.DeviceEui == device.DeviceEui)
                    .Where(notification => currentTimestamp - notification.Timestamp < deviceHeartbeatPeriodInSeconds)
                    .AnyAsync();
                bool shouldChangeStatus;
                if (device.Status.DeviceWorking)
                {
                    shouldChangeStatus = !anyRecentNotifications;
                }
                else
                {
                    shouldChangeStatus = anyRecentNotifications;
                }

                if (shouldChangeStatus)
                {
                    device.Status.DeviceWorking = !device.Status.DeviceWorking;
                    device.Status.SentToKommune = !device.Status.SentToKommune;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<UnsentDeviceStatus>> GetUnsentDeviceStatuses()
        {
            return await _context.Device
                .Where(device => !device.Status.SentToKommune)
                .Select(device => new UnsentDeviceStatus
                {
                    Address = device.Address,
                    DeviceEui = device.DeviceEui,
                    DeviceUnresponsive = !device.Status.DeviceWorking,
                    NotificationId = 0,
                    Timestamp = 0
                })
                .ToListAsync();
            /*
            .Join(_context.Notification,
                device => device.DeviceEui,
                notification => notification.DeviceEui,
                (device, notification) => new UnsentDeviceStatus
                {
                    Address = device.Address,
                    DeviceEui = device.DeviceEui,
                    DeviceUnresponsive = !device.Status.DeviceWorking,
                    NotificationId = 0,
                    Timestamp = notification.Timestamp
                })
            .ToListAsync();
        unsentDeviceStatusesWithNotifications.GroupBy(x => x.DeviceEui, x => x);
    */
        }
    }
}