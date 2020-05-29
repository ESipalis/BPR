using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommonServices.DetectionSystemServices;
using CommonServices.DetectionSystemServices.Storage;
using Data;
using DataEFCore;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using Xunit;

// ReSharper disable xUnit2004

namespace CommonServices.Test
{
    public class StorageDatabaseUnitTest
    {
        private readonly DetectionSystemDbContext _context;
        private readonly StorageDatabase _storageDatabase;

        public StorageDatabaseUnitTest()
        {
            // _context = new DetectionSystemDbContext(new DbContextOptionsBuilder().UseSqlite("Data Source = test.db").Options);
            _context = new DetectionSystemDbContext(new DbContextOptionsBuilder().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _context.Database.EnsureCreated();
            _storageDatabase = new StorageDatabase(_context);
        }

        [Fact]
        public async Task AddNotifications_Test()
        {
            IEnumerable<Notification> notificationsToInsert = new[]
            {
                new Notification
                {
                    Address = "Address1",
                    DeviceEui = "DeviceEui1",
                    Timestamp = 123451,
                    Type = NotificationType.ObjectDetection,
                    ObjectDetectionNotification = new ObjectDetectionNotification
                    {
                        ObjectDetection = ObjectDetection.DetectedWithSize,
                        SentToKommune = true,
                        WidthCentimeters = 15
                    }
                },
                new Notification
                {
                    Address = "Address2",
                    DeviceEui = "DeviceEui2",
                    Timestamp = 123451,
                    Type = NotificationType.Heartbeat
                },
                new Notification
                {
                    Address = "Address3",
                    DeviceEui = "DeviceEui3",
                    Timestamp = 123453,
                    Type = NotificationType.Heartbeat
                },
                new Notification
                {
                    Address = "Address4",
                    DeviceEui = "DeviceEui4",
                    Timestamp = 123452,
                    Type = NotificationType.ObjectDetection,
                    ObjectDetectionNotification = new ObjectDetectionNotification
                    {
                        ObjectDetection = ObjectDetection.Detected,
                        SentToKommune = false
                    }
                },
            };
            await _storageDatabase.AddNotifications(notificationsToInsert);
            List<Notification> notifications = await _context.Notification.ToListAsync();

            Assert.Equal("Address1", notifications[0].Address);
            Assert.Equal("DeviceEui1", notifications[0].DeviceEui);
            Assert.Equal(123451, notifications[0].Timestamp);
            Assert.Equal(NotificationType.ObjectDetection, notifications[0].Type);
            Assert.Equal(ObjectDetection.DetectedWithSize, notifications[0].ObjectDetectionNotification.ObjectDetection);
            Assert.True(notifications[0].ObjectDetectionNotification.SentToKommune);
            Assert.Equal(15, notifications[0].ObjectDetectionNotification.WidthCentimeters);


            Assert.Equal("Address2", notifications[1].Address);
            Assert.Equal("DeviceEui2", notifications[1].DeviceEui);
            Assert.Equal(123451, notifications[1].Timestamp);
            Assert.Equal(NotificationType.Heartbeat, notifications[1].Type);

            Assert.Equal("Address3", notifications[2].Address);
            Assert.Equal("DeviceEui3", notifications[2].DeviceEui);
            Assert.Equal(123453, notifications[2].Timestamp);
            Assert.Equal(NotificationType.Heartbeat, notifications[2].Type);


            Assert.Equal("Address4", notifications[3].Address);
            Assert.Equal("DeviceEui4", notifications[3].DeviceEui);
            Assert.Equal(123452, notifications[3].Timestamp);
            Assert.Equal(NotificationType.ObjectDetection, notifications[3].Type);
            Assert.Equal(ObjectDetection.Detected, notifications[3].ObjectDetectionNotification.ObjectDetection);
            Assert.False(notifications[3].ObjectDetectionNotification.SentToKommune);
            Assert.Null(notifications[3].ObjectDetectionNotification.WidthCentimeters);
        }


        [Fact]
        public async Task RefreshDeviceStatuses_Test()
        {
            _context.Device.AddRange(new[]
            {
                new Device
                {
                    Address = "testAddress1",
                    DeviceEui = "DeviceEui1",
                    Configuration = new DeviceConfiguration
                    {
                        HeartbeatPeriodDays = 7,
                        ScanMinuteOfTheDay = 101,
                        Status = ConfigurationStatus.NotSent
                    },
                    Status = new DeviceStatus
                    {
                        DeviceWorking = true,
                        SentToKommune = true
                    }
                },
                new Device
                {
                    Address = "testAddress2",
                    DeviceEui = "DeviceEui2",
                    Configuration = new DeviceConfiguration
                    {
                        HeartbeatPeriodDays = 7,
                        ScanMinuteOfTheDay = 101,
                        Status = ConfigurationStatus.NotSent
                    },
                    Status = new DeviceStatus
                    {
                        DeviceWorking = true,
                        SentToKommune = true
                    }
                },
                
                new Device
                {
                    Address = "testAddress3",
                    DeviceEui = "DeviceEui3",
                    Configuration = new DeviceConfiguration
                    {
                        HeartbeatPeriodDays = 7,
                        ScanMinuteOfTheDay = 101,
                        Status = ConfigurationStatus.NotSent
                    },
                    Status = new DeviceStatus
                    {
                        DeviceWorking = false,
                        SentToKommune = true
                    }
                },
                new Device
                {
                    Address = "testAddress4",
                    DeviceEui = "DeviceEui4",
                    Configuration = new DeviceConfiguration
                    {
                        HeartbeatPeriodDays = 7,
                        ScanMinuteOfTheDay = 101,
                        Status = ConfigurationStatus.NotSent
                    },
                    Status = new DeviceStatus
                    {
                        DeviceWorking = false,
                        SentToKommune = true
                    }
                },
            });
            _context.Notification.AddRange(new []
            {
                new Notification // Device still working
                {
                    Address = "testAddress1",
                    DeviceEui = "DeviceEui1",
                    Type = NotificationType.Heartbeat,
                    Timestamp = DateTimeOffset.Now.AddDays(-3).ToUnixTimeSeconds()
                },
                new Notification // Device not working anymore
                {
                    Address = "testAddress2",
                    DeviceEui = "DeviceEui2",
                    Type = NotificationType.Heartbeat,
                    Timestamp = DateTimeOffset.Now.AddDays(-10).ToUnixTimeSeconds()
                },
                
                new Notification // Device working again
                {
                    Address = "testAddress3",
                    DeviceEui = "DeviceEui3",
                    Type = NotificationType.Heartbeat,
                    Timestamp = DateTimeOffset.Now.AddDays(-3).ToUnixTimeSeconds()
                },
                new Notification // Device still not working
                {
                    Address = "testAddress4",
                    DeviceEui = "DeviceEui4",
                    Type = NotificationType.Heartbeat,
                    Timestamp = DateTimeOffset.Now.AddDays(-10).ToUnixTimeSeconds()
                },
            });
            await _context.SaveChangesAsync();

            await _storageDatabase.RefreshDeviceStatuses();

            List<Device> devices = await _context.Device.ToListAsync();
            Assert.True(devices[0].Status.DeviceWorking);
            Assert.False(devices[1].Status.DeviceWorking);
            Assert.True(devices[2].Status.DeviceWorking);
            Assert.False(devices[3].Status.DeviceWorking);
        }


        [Fact]
        public async Task GetUnsentDeviceStatuses_Test()
        {
            _context.Device.AddRange(new[]
            {
                new Device
                {
                    Address = "testAddress1",
                    DeviceEui = "DeviceEui1",
                    Configuration = new DeviceConfiguration
                    {
                        HeartbeatPeriodDays = 7,
                        ScanMinuteOfTheDay = 101,
                        Status = ConfigurationStatus.NotSent
                    },
                    Status = new DeviceStatus
                    {
                        DeviceWorking = true,
                        SentToKommune = false
                    }
                },
                new Device
                {
                    Address = "testAddress2",
                    DeviceEui = "DeviceEui2",
                    Configuration = new DeviceConfiguration
                    {
                        HeartbeatPeriodDays = 7,
                        ScanMinuteOfTheDay = 101,
                        Status = ConfigurationStatus.NotSent
                    },
                    Status = new DeviceStatus
                    {
                        DeviceWorking = true,
                        SentToKommune = true
                    }
                },
                
                new Device
                {
                    Address = "testAddress3",
                    DeviceEui = "DeviceEui3",
                    Configuration = new DeviceConfiguration
                    {
                        HeartbeatPeriodDays = 7,
                        ScanMinuteOfTheDay = 101,
                        Status = ConfigurationStatus.NotSent
                    },
                    Status = new DeviceStatus
                    {
                        DeviceWorking = false,
                        SentToKommune = true
                    }
                },
                new Device
                {
                    Address = "testAddress4",
                    DeviceEui = "DeviceEui4",
                    Configuration = new DeviceConfiguration
                    {
                        HeartbeatPeriodDays = 7,
                        ScanMinuteOfTheDay = 101,
                        Status = ConfigurationStatus.NotSent
                    },
                    Status = new DeviceStatus
                    {
                        DeviceWorking = false,
                        SentToKommune = false
                    }
                },
            });
            await _context.SaveChangesAsync();
            
            List<UnsentDeviceStatus> unsentDeviceStatuses = await _storageDatabase.GetUnsentDeviceStatuses();
            Assert.Equal(2, unsentDeviceStatuses.Count);
            Assert.Equal("DeviceEui1", unsentDeviceStatuses[0].DeviceEui);
            Assert.Equal("DeviceEui4", unsentDeviceStatuses[1].DeviceEui);
        }
        
    }
}