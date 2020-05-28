using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using DataEFCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ApplicationServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly DetectionSystemDbContext _context;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DetectionSystemDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            // _context.Device.Add(new Device
            // {
            //     Address = "testAddress",
            //     DeviceEui = "1234567890ABCDEF",
            //     Configuration = new DeviceConfiguration
            //     {
            //         HeartbeatPeriodDays = 7,
            //         ScanMinuteOfTheDay = 101,
            //         Status = ConfigurationStatus.NotSent
            //     },
            //     Status = new DeviceStatus
            //     {
            //         DeviceWorking = true,
            //         SentToKommune = true
            //     }
            // });

            _context.Notification.Add(new Notification
            {
                Address = "fadfasd",
                DeviceEui = "fadsfasdfas",
                Timestamp = 123451,
                Type = NotificationType.ObjectDetection,
                ObjectDetectionNotification = new ObjectDetectionNotification
                {
                    ObjectDetection = ObjectDetection.DetectedWithSize,
                    SentToKommune = true,
                    WidthCentimeters = 15
                }
            });
            _context.SaveChanges();
            
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();
        }
    }
}