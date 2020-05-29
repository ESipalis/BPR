using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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
        public IActionResult ReceiveNotifications(NotificationToKommune[] notifications)
        {
            Console.WriteLine(JsonSerializer.Serialize(notifications,
                new JsonSerializerOptions {WriteIndented = true}
            ));
            return Ok();
        }
    }
}