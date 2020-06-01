using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationServer.BackgroundServices;
using CommonServices.DetectionSystemServices;
using CommonServices.DetectionSystemServices.KommuneService;
using CommonServices.DetectionSystemServices.Storage;
using CommonServices.EndNodeCommunicator;
using DataEFCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ApplicationServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DetectionSystemDbContext>(builder => { builder.UseSqlite(Configuration.GetConnectionString("SQLite")); });
            services.AddScoped<IStorage, StorageDatabase>();
            services.AddScoped<IKommuneService, KommuneServiceHttp>();
            services.AddSingleton<IEndNodeCommunicator, EndNodeCommunicatorWebSocket>();
            services.AddSingleton<EndNodeCommunicatorWebSocketConfiguration>(services => new EndNodeCommunicatorWebSocketConfiguration
            {
                Url = "wss://echo.websocket.org"
            });
            services.AddHttpClient<KommuneHttpClient>(httpClient =>
            {
                httpClient.BaseAddress = new Uri("https://localhost:44362/");
            });
            services.AddScoped<DetectionSystemService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                using IServiceScope scope = app.ApplicationServices.CreateScope();
                using var context = scope.ServiceProvider.GetService<DetectionSystemDbContext>();
                // context.Database.EnsureCreated();
                context.Database.Migrate();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}