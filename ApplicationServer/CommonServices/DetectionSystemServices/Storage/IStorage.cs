﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Data;

namespace CommonServices.DetectionSystemServices.Storage
{
    public interface IStorage
    {
        Task AddNotifications(IEnumerable<Notification> notifications);
        Task SetNotificationsAsSent(IEnumerable<int> notificationIds);
        Task<List<Notification>> GetFailedNotifications();
        Task AddDevices(IEnumerable<Device> devices);
        Task UpdateDeviceConfigurations(IEnumerable<ConfigureDevice> configurations);
        Task UpdateDeviceConfigurationStatus(string deviceEui, ConfigurationStatus configurationStatus);
        Task RefreshDeviceStatuses();
        Task<List<UnsentDeviceStatus>> GetUnsentDeviceStatuses();
        Task SetDeviceStatusesAsSent(IEnumerable<string> deviceEuis);
    }
}