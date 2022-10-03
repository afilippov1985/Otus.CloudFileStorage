using MassTransit;
using Microsoft.Extensions.Hosting;

namespace ArchiveService
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit((x) => {
                        var RabbitSection = hostContext.Configuration.GetSection("Rabbit");

                        x.SetKebabCaseEndpointNameFormatter();

                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host(RabbitSection["Host"], "/", h => {
                                h.Username(RabbitSection["Username"]);
                                h.Password(RabbitSection["Password"]);
                            });

                            cfg.ConfigureEndpoints(context);
                        });

                        x.AddConsumer<ZipConsumer>();
                        x.AddConsumer<UnzipConsumer>();
                    });

                    // services.AddHostedService<Worker>();
                })
                .Build();

            host.Run();
        }
    }
}