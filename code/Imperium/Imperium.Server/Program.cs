
using Imperium.Common;
using Mekatrol.Devices;
using Microsoft.Extensions.Configuration;

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

            foreach (var url in urls.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                app.Urls.Add(url);
            }

            app.Run();
        }
    }
}
