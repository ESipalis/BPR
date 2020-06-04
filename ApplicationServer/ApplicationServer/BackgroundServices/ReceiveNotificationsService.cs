using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CommonServices.DetectionSystemServices;
using CommonServices.EndNodeCommunicator;
using CommonServices.EndNodeCommunicator.Models;
using Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApplicationServer.BackgroundServices
{
    public class ReceiveNotificationsService : BackgroundService
    {
        private static readonly string Connection = "connectionstring";

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReceiveNotificationsService> _logger;

        public ReceiveNotificationsService(IServiceProvider serviceProvider, ILogger<ReceiveNotificationsService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var endNodeCommunicator = scope.ServiceProvider.GetRequiredService<IEndNodeCommunicator>();
            endNodeCommunicator.AddListener(HandleReceivedMessage);
            await endNodeCommunicator.Start(stoppingToken);
        }

        private async Task HandleReceivedMessage(EndNodeMessage message)
        {
            IServiceScope scope = _serviceProvider.CreateScope();
            var detectionSystemService = scope.ServiceProvider.GetRequiredService<DetectionSystemService>();
            var endNodeCommunicator = scope.ServiceProvider.GetRequiredService<IEndNodeCommunicator>();
            _logger.LogInformation("Handing received message: " + JsonSerializer.Serialize(message));

            switch (message.MessageType)
            {
                case EndNodeMessageType.SendRequestAck:
                    var newStatus = ((SendRequestAckMessage) message).Successful ? ConfigurationStatus.AcknowledgedByNetwork : ConfigurationStatus.ErrorByNetwork;
                    await detectionSystemService.SetDeviceConfigurationStatus(message.DeviceEui, newStatus);
                    break;
                case EndNodeMessageType.GatewayConfirmation:
                    await detectionSystemService.SetDeviceConfigurationStatus(message.DeviceEui, ConfigurationStatus.SentToGateway);
                    break;
                case EndNodeMessageType.UplinkMessage:
                    var uplinkMessage = (UplinkDataMessage) message;
                    await detectionSystemService.SendAndSaveNotifications(new[]
                    {
                        new UplinkMessage
                        {
                            Data = uplinkMessage.Data,
                            DeviceEui = uplinkMessage.DeviceEui,
                            Timestamp = uplinkMessage.Timestamp
                        }
                    });
                    Device device = await detectionSystemService.GetDevice(message.DeviceEui);
                    if (device.Configuration.Status == ConfigurationStatus.SentToGateway)
                    {
                        await detectionSystemService.SetDeviceConfigurationStatus(message.DeviceEui, ConfigurationStatus.SentToDevice);
                    }
                    else if (device.Configuration.Status == ConfigurationStatus.SentToDevice)
                    {
                        if (uplinkMessage.Ack)
                        {
                            await detectionSystemService.SetDeviceConfigurationStatus(message.DeviceEui, ConfigurationStatus.Acknowledged);
                        }
                        else
                        {
                            endNodeCommunicator.SendMessage(new DownlinkDataMessage
                            {
                                Confirmed = true,
                                DeviceEui = device.DeviceEui,
                                Data = DetectionSystemServiceUtil.ConfigurationToDataString(device.Configuration.ScanMinuteOfTheDay, device.Configuration.HeartbeatPeriodDays)
                            });
                            await detectionSystemService.SetDeviceConfigurationStatus(device.DeviceEui, ConfigurationStatus.SentToNetwork);
                        }
                    }
                    break;
            }
        }
    }
}