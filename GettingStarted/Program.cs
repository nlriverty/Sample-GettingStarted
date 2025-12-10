using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MassTransit;

namespace GettingStarted
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHealthChecks();

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<MessageConsumer>();

                        x.ConfigureHealthCheckOptions(options =>
                        {
                            options.Tags.Add("health");
                        });

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddHostedService<Worker>();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                            {
                                Predicate = x => x.Tags.Contains("health"),
                                ResponseWriter = async (context, report) =>
                                {
                                    context.Response.ContentType = "application/json";
                                    var filteredReport = new
                                    {
                                        Status = report.Status.ToString(),
                                        TotalDuration = report.TotalDuration,
                                        Entries = report.Entries.ToDictionary(
                                            e => e.Key,
                                            e => new
                                            {
                                                Status = e.Value.Status.ToString(),
                                                Description = e.Value.Description,
                                                Duration = e.Value.Duration,
                                                Data = e.Value.Data,
                                                Exception = e.Value.Exception?.Message // Only include the exception message
                                            }
                                        )
                                    };
                                    await JsonSerializer.SerializeAsync(context.Response.Body, filteredReport, new JsonSerializerOptions
                                    {
                                        WriteIndented = true,
                                        Converters =
                                        {
                                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                                        }
                                    });
                                }
                            });
                        });
                    });
                });
    }
}
