
using Imperium.Common;
using Imperium.Models;
using Imperium.Server.Background;
using Mekatrol.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;

namespace Imperium.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<IDevice, SimpleOutputBoard>();

            // Bind http client options
            var httpClientOptions = new HttpClientOptions();
            builder.Configuration.Bind(HttpClientOptions.SectionName, httpClientOptions);
            builder.Services.AddSingleton(httpClientOptions);

            // Bind background service options        
            var backgroundServiceOptions = new BackgroundServiceOptions();
            builder.Configuration.Bind(BackgroundServiceOptions.SectionName, backgroundServiceOptions);
            builder.Services.AddSingleton(backgroundServiceOptions);

            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = httpClientOptions.ConnectionLifeTime
            };
            var client = new HttpClient(handler);
            builder.Services.AddSingleton(client);

            builder.Services.AddHostedService<DeviceControllerBackgroundService>();

            builder.Services.AddSerilog(config => {
                config
                    .WriteTo.Console()
                    .ReadFrom.Configuration(builder.Configuration);
                });
            //builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

            IList<string> urls = [];
            builder.Configuration.Bind("AppUrls", urls);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.EnableTryItOutByDefault();
            });
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Listening on URLs: {urls}", $"{string.Join(',', urls)}");

            foreach (var url in urls.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                logger.LogInformation("Adding URL: {url}", $"{url}");
                app.Urls.Add(url);
            }

            app.Run();
        }
    }
}
