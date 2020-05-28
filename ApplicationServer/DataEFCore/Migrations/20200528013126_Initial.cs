using Microsoft.EntityFrameworkCore.Migrations;

namespace DataEFCore.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Device",
                columns: table => new
                {
                    DeviceId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DeviceEui = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Device", x => x.DeviceId);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(nullable: false),
                    Timestamp = table.Column<long>(nullable: false),
                    DeviceEui = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceConfiguration",
                columns: table => new
                {
                    DeviceId = table.Column<int>(nullable: false),
                    Status = table.Column<string>(nullable: false),
                    ScanMinuteOfTheDay = table.Column<short>(nullable: false),
                    HeartbeatPeriodDays = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceConfiguration", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_DeviceConfiguration_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceStatus",
                columns: table => new
                {
                    DeviceId = table.Column<int>(nullable: false),
                    DeviceWorking = table.Column<bool>(nullable: false),
                    SentToKommune = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStatus", x => x.DeviceId);
                    table.ForeignKey(
                        name: "FK_DeviceStatus_Device_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Device",
                        principalColumn: "DeviceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ObjectDetectionNotification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(nullable: false),
                    SentToKommune = table.Column<bool>(nullable: false),
                    WidthCentimeters = table.Column<int>(nullable: true),
                    ObjectDetection = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectDetectionNotification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_ObjectDetectionNotification_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "NotificationId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceConfiguration");

            migrationBuilder.DropTable(
                name: "DeviceStatus");

            migrationBuilder.DropTable(
                name: "ObjectDetectionNotification");

            migrationBuilder.DropTable(
                name: "Device");

            migrationBuilder.DropTable(
                name: "Notification");
        }
    }
}
