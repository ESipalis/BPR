using System;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DataEFCore
{
    public class DetectionSystemDbContext : DbContext
    {
        protected DetectionSystemDbContext()
        {
        }

        public DetectionSystemDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (IMutableProperty property in entityType.GetProperties())
                {
                    if (property.ClrType.BaseType == typeof(Enum))
                    {
                        Type type = typeof(EnumToStringConverter<>).MakeGenericType(property.ClrType);
                        ValueConverter converter = Activator.CreateInstance(type, new ConverterMappingHints()) as ValueConverter;

                        property.SetValueConverter(converter);
                    }
                }
            }

            modelBuilder.Entity<ObjectDetectionNotification>().HasKey(x => x.NotificationId);
            modelBuilder.Entity<DeviceConfiguration>().HasKey(x => x.DeviceId);
            modelBuilder.Entity<DeviceStatus>().HasKey(x => x.DeviceId);
        }

        public DbSet<Notification> Notification { get; set; }
        public DbSet<Device> Device { get; set; }
    }
}