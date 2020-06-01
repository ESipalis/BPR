using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationServer.Models;
using CommonServices.DetectionSystemServices;
using CommonServices.DetectionSystemServices.Storage;
using CommonServices.EndNodeCommunicator;
using CommonServices.EndNodeCommunicator.Models;
using Data;
using DataEFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApplicationServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DetectionSystemController : ControllerBase
    {
        private readonly ILogger<DetectionSystemController> _logger;
        private readonly DetectionSystemService _detectionSystemService;
        private readonly IEndNodeCommunicator _endNodeCommunicator;

        public DetectionSystemController(ILogger<DetectionSystemController> logger, DetectionSystemService detectionSystemService, IEndNodeCommunicator endNodeCommunicator)
        {
            _logger = logger;
            _detectionSystemService = detectionSystemService;
            _endNodeCommunicator = endNodeCommunicator;
        }

        [HttpPost("configurations")]
        public async Task<IActionResult> ConfigureDevices(ConfigurationDto[] configurations)
        {
            List<ConfigureDevice> configureDevices = new List<ConfigureDevice>();
            foreach (ConfigurationDto configurationDto in configurations)
            {
                foreach (string deviceEui in configurationDto.DeviceEuis)
                {
                    configureDevices.Add(new ConfigureDevice
                    {
                        DeviceEui = deviceEui,
                        Configuration = new DeviceConfiguration
                        {
                            HeartbeatPeriodDays = configurationDto.Configuration.HeartbeatPeriodDays,
                            ScanMinuteOfTheDay = configurationDto.Configuration.ScanMinuteOfTheDay,
                            Status = ConfigurationStatus.NotSent
                        }
                    });
                }
            }

            await _detectionSystemService.ConfigureDevices(configureDevices);
            foreach (ConfigureDevice configureDevice in configureDevices)
            {
                _endNodeCommunicator.SendMessage(new DownlinkDataMessage
                {
                    Confirmed = true,
                    DeviceEui = configureDevice.DeviceEui,
                    Data = ConfigurationToDataString(configureDevice.Configuration.ScanMinuteOfTheDay, configureDevice.Configuration.HeartbeatPeriodDays)
                });
                await _detectionSystemService.SetDeviceConfigurationStatus(configureDevice.DeviceEui, ConfigurationStatus.SentToNetwork);
            }

            return Ok();
        }

        [HttpPost("devices")]
        public async Task<IActionResult> RegisterDevices(RegisterDeviceDto[] deviceDtos)
        {
            List<Device> devices = deviceDtos.Select(dto => new Device
            {
                DeviceEui = dto.DeviceEui,
                Address = dto.Address,
                Status = new DeviceStatus
                {
                    DeviceWorking = true,
                    SentToKommune = true
                },
                Configuration = new DeviceConfiguration
                {
                    HeartbeatPeriodDays = dto.Configuration.HeartbeatPeriodDays,
                    ScanMinuteOfTheDay = dto.Configuration.ScanMinuteOfTheDay,
                    Status = ConfigurationStatus.NotSent
                }
            }).ToList();
            await _detectionSystemService.RegisterDevices(devices);

            foreach (Device device in devices)
            {
                _endNodeCommunicator.SendMessage(new DownlinkDataMessage
                {
                    Confirmed = true,
                    DeviceEui = device.DeviceEui,
                    Data = ConfigurationToDataString(device.Configuration.ScanMinuteOfTheDay, device.Configuration.HeartbeatPeriodDays)
                });
                await _detectionSystemService.SetDeviceConfigurationStatus(device.DeviceEui, ConfigurationStatus.SentToNetwork);
            }

            return Ok();
        }

        private static string ConfigurationToDataString(short scanMinuteOfTheDay, byte heartbeatPeriodDays)
        {
            return scanMinuteOfTheDay.ToString("X") + heartbeatPeriodDays.ToString("X");
        }
    }
}