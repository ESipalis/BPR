using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Data;
using KommuneNotificationModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KommuneDashboardServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(ILogger<NotificationsController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveNotifications(NotificationDto[] notifications)
        {
            Console.WriteLine(JsonSerializer.Serialize(notifications, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
            return Ok();
        }
    }

    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public long Timestamp { get; set; }
        public string Address { get; set; }
        public string DeviceEui { get; set; }
        public ObjectDetection? ObjectDetection { get; set; }
        public int? WidthCentimeters { get; set; }
        public bool? DeviceUnresponsive { get; set; }
    }
}