
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using NLog.Web;
using System.Net;
using System.Reflection;

#nullable disable

namespace CloudStorage.WebApi.Shared
{
    /// <summary>
    /// Набор расширений для формирования Program для запуска приложения ASP.NET Core 
    /// </summary>
    public static class ProgramExtensions
    {

        /// <summary>
        /// Обертка над инициализацией веб-приложения. Выполняет предварительную конфигурацию, собирает IWebHost
        /// и выполняет необходимые миграции при необходимости
        /// </summary>
        /// <typeparam name="TStartup"></typeparam>
        /// <param name="appName"></param>
        /// <param name="args"></param>
        /// <param name="migrate"></param>
        /// <returns></returns>
        public static int InitWrap<TStartup>(string appName, string[] args, Action<IHost, NLog.Logger> migrate)
                    where TStartup : class
        {
            var configuration = GetConfiguration();
            ConfigSettingLayoutRenderer.DefaultConfiguration = configuration;
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();

            try
            {
                logger?.Info($"Configuring web host ({appName})...");
                var host = CreateHostBuilder<TStartup>(configuration, args).Build();

                host.Run();
                return 0;
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "An error occurred seeding the DB.");
                return 1;
            }
        }

        /// <summary>
        /// Производит сборку IWebHost для запуска приложения
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="config"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder<T>(IConfiguration config, string[] args) where T : class
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseConfiguration(config);
                var useHttps = config.GetValue<int?>("https:port").HasValue;
                if (useHttps)
                {
                    var port = config.GetValue<int>("https:port");
                    var cert = config.GetValue<string>("https:cert");
                    var pwd = config.GetValue<string>("https:pwd");

                    var httpPort = config.GetValue<int?>("https:httpPort");

                    webBuilder.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, port, listenOptions => listenOptions.UseHttps(cert, pwd));
                        // Включаем опционально http
                        // TODO: по идее его можно из urls читать
                        if (httpPort.HasValue)
                            options.Listen(IPAddress.Any, httpPort.Value);
                    });
                }
                webBuilder.UseStartup<T>()
                    .UseNLog()
                    .UseIISIntegration();
            });

            return builder;
        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder().ConfigureBuilder(null, new string[0]);
            return builder.Build();
        }

        private static IConfigurationBuilder ConfigureBuilder(this IConfigurationBuilder builder, string environmentName, string[] args)
        {
            builder.SetBasePath(AssemblyDirectory);
            builder.AddJsonFile("appsettings.json", optional: true);
            builder.AddJsonFile("hosting.json", optional: false);
            builder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
            builder.AddEnvironmentVariables();
            builder.AddCommandLine(args);
            return builder;
        }

        internal static string AssemblyDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }
    }
}
