using FileManagerService.Data;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileManagerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("FileManager")));

            builder.Services.AddIdentity<User, IdentityRole>((options) => {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 5; // минимальная длина
                options.Password.RequireNonAlphanumeric = false; // требуются ли не алфавитно-цифровые символы
                options.Password.RequireLowercase = false; // требуются ли символы в нижнем регистре
                options.Password.RequireUppercase = false; // требуются ли символы в верхнем регистре
                options.Password.RequireDigit = false; // требуются ли цифры
            })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddMassTransit((x) => {
                var RabbitSection = builder.Configuration.GetSection("Rabbit");

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(RabbitSection["Host"], "/", h => {
                        h.Username(RabbitSection["Username"]);
                        h.Password(RabbitSection["Password"]);
                    });

                    cfg.ConfigureEndpoints(context);
                });
            });

            // Add services to the container.
            // builder.Services.AddControllersWithViews();
            builder.Services.AddControllers().AddJsonOptions((options) => {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
