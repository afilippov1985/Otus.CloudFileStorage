using Core.Domain.Entities;
using Core.Infrastructure.DataAccess;
using MassTransit;
//using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseNpgsql(hostContext.Configuration.GetConnectionString("FileManager")));

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