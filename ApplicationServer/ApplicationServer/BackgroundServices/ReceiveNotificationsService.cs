using System;
using System.Threading;
using System.Threading.Tasks;
using CommonServices.DetectionSystemServices;
using CommonServices.EndNodeCommunicator;
using CommonServices.EndNodeCommunicator.Models;
using Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApplicationServer.BackgroundServices
{
    public class ReceiveNotificationsService : BackgroundService
    {
        private static readonly string Connection = "connectionstring";

        private readonly IServiceProvider _serviceProvider;

        public ReceiveNotificationsService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                    if (uplinkMessage.Ack)
                    {
                        await detectionSystemService.SetDeviceConfigurationStatus(message.DeviceEui, ConfigurationStatus.Acknowledged);
                    }
                    
                    await detectionSystemService.SendAndSaveNotifications(new []
                    {
                        new UplinkMessage
                        {
                            Data = uplinkMessage.Data,
                            DeviceEui = uplinkMessage.DeviceEui,
                            Timestamp = uplinkMessage.Timestamp
                        }
                    });
                    break;
            }
        }
        
        
    }
}