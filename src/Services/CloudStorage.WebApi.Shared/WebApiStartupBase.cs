<<<<<<< HEAD
﻿using CloudStorage.WebApi.Shared.Infrastructure;
using CloudStorage.WebAPi.Shared.Helpers;
using GeoAnalytics.WebApi.Shared.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;

#nullable disable

namespace CloudStorage.WebApi.Shared
{
    /// <summary>
    /// Базовый класс Startup для WebApi
    /// </summary>
    public abstract class WebApiStartupBase
    {
        /// <summary>
        /// Возвращает конфигурацию приложения
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Возвращает окружение 
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// Абстрактное свойство.
        /// 
        /// Возвращает информацию по API, которая отображается в Swagger
        /// </summary>
        protected abstract List<Tuple<string, string, OpenApiInfo>> ApiInfos { get; }

        /// <summary>
        /// Возвращает флаг, необходимо ли использовать полные имена для Swagger. По умолчанию - false
        /// </summary>
        protected virtual bool UseFullNameForSchema => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        public WebApiStartupBase(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        /// <summary>
        /// Производит регистрацию сервисов в рамках DI
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var productVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                foreach (var item in ApiInfos)
                {
                    item.Item3.Description += $" (Версия {productVersion})";
                    options.SwaggerDoc(item.Item1, item.Item3);
                }

                //options.OperationFilter<RemoveVersionFromParameter>();
                //options.DocumentFilter<ReplaceVersionWithExactValueInPath>();

                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                var dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                foreach (var fi in dir.EnumerateFiles("CloudStorage.*.xml"))
                    options.IncludeXmlComments(fi.FullName);

                if (UseFullNameForSchema)
                    options.CustomSchemaIds(x => x.FullName);
            });
            services.AddSwaggerGenNewtonsoftSupport();

            ConfigureServicesCore(services);

            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1
            // https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            services.AddHttpClient();

            services.AddHttpContextAccessor();

            ConfigureControllersCore(services);

            services.AddApiVersioning();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddHealthChecks();

            services.AddDistributedMemoryCache();
            services.AddSession();
        }

        /// <summary>
        /// Перегружаемый метод. 
        /// 
        /// Используется для регистрации локальных сервисов
        /// </summary>
        /// <param name="services"></param>
        protected virtual void ConfigureServicesCore(IServiceCollection services) { }

        /// <summary>
        /// Выполняет первичную конфигурацию приложения
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            ILogger<WebApiStartupBase> logger)
        {
            app.UseExceptionHandler(err => UseCustomExceptionHandling(err, env));
            var pathBase = Configuration.GetBasePath() ?? string.Empty;

            if (!string.IsNullOrEmpty(pathBase))
            {
                logger.LogInformation("Using PATH BASE '{pathBase}'", pathBase);
                app.UsePathBase(pathBase);
            }

            ConfigureCore(app);

            
            app.UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.RoutePrefix = "swagger";
                    foreach (var item in ApiInfos)
                        options.SwaggerEndpoint($"{item.Item1}/swagger.json",
                            item.Item2);
                });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseMiddleware(typeof(ErrorLoggingMiddleware));
            app.UseCors("CorsPolicy");

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseApiVersioning();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/health");
            });
        }

        /// <summary>
        /// Конфигурация обработчиков исключений
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        protected virtual void UseCustomExceptionHandling(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware(typeof(CustomErrorResponseMiddleware));
        }

        /// <summary>
        /// Перегружаемый метод для конфигурации приложения
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        protected virtual IMvcBuilder ConfigureControllersCore(IServiceCollection services)
        {
            var assembly = typeof(WebApiStartupBase).Assembly;

            // Конфигурируем стандартный REST API
            var builder = services.AddControllers(options =>
            {
                options.InputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>();
                options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>();
            })
                .AddNewtonsoftJson(opts =>
                {
                    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    opts.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
                    opts.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

                })
                // .AddXmlSerializerFormatters()
                ;

            // NOTE: добавляем контроллеры из этой сборки
            builder.PartManager.ApplicationParts.Add(new AssemblyPart(assembly));

            return builder;
        }

        /// <summary>
        /// Перегружаемы метод для расширения конфигурации
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureCore(IApplicationBuilder app) { }
    }
}
=======
﻿using CloudStorage.WebApi.Shared.Infrastructure;
using CloudStorage.WebAPi.Shared.Helpers;
using GeoAnalytics.WebApi.Shared.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

#nullable disable

namespace CloudStorage.WebApi.Shared
{
    /// <summary>
    /// Базовый класс Startup для WebApi
    /// </summary>
    public abstract class WebApiStartupBase
    {
        /// <summary>
        /// Возвращает конфигурацию приложения
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Возвращает окружение 
        /// </summary>
        public IWebHostEnvironment Environment { get; }

        /// <summary>
        /// Абстрактное свойство.
        /// 
        /// Возвращает информацию по API, которая отображается в Swagger
        /// </summary>
        protected abstract List<Tuple<string, string, OpenApiInfo>> ApiInfos { get; }

        /// <summary>
        /// Возвращает флаг, необходимо ли использовать полные имена для Swagger. По умолчанию - false
        /// </summary>
        protected virtual bool UseFullNameForSchema => false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="env"></param>
        public WebApiStartupBase(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        /// <summary>
        /// Производит регистрацию сервисов в рамках DI
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var productVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
                foreach (var item in ApiInfos)
                {
                    item.Item3.Description += $" (Версия {productVersion})";
                    options.SwaggerDoc(item.Item1, item.Item3);
                }

                //options.OperationFilter<RemoveVersionFromParameter>();
                //options.DocumentFilter<ReplaceVersionWithExactValueInPath>();

                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                var dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
                foreach (var fi in dir.EnumerateFiles("CloudStorage.*.xml"))
                    options.IncludeXmlComments(fi.FullName);

                if (UseFullNameForSchema)
                    options.CustomSchemaIds(x => x.FullName);
            });
            services.AddSwaggerGenNewtonsoftSupport();

            ConfigureServicesCore(services);

            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1
            // https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
            services.AddHttpClient();

            services.AddHttpContextAccessor();

            ConfigureControllersCore(services);

            services.AddApiVersioning();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .SetIsOriginAllowed((host) => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            });

            services.AddHealthChecks();

            services.AddDistributedMemoryCache();
            services.AddSession();

            // аутентификация с помощью куки
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/login");
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                    options.SlidingExpiration = true;
                    options.AccessDeniedPath = "/Forbidden/";
                });
            services.AddAuthorization();            
        }

        /// <summary>
        /// Перегружаемый метод. 
        /// 
        /// Используется для регистрации локальных сервисов
        /// </summary>
        /// <param name="services"></param>
        protected virtual void ConfigureServicesCore(IServiceCollection services) { }

        /// <summary>
        /// Выполняет первичную конфигурацию приложения
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            ILogger<WebApiStartupBase> logger)
        {
            app.UseExceptionHandler(err => UseCustomExceptionHandling(err, env));
            var pathBase = Configuration.GetBasePath() ?? string.Empty;

            if (!string.IsNullOrEmpty(pathBase))
            {
                logger.LogInformation("Using PATH BASE '{pathBase}'", pathBase);
                app.UsePathBase(pathBase);
            }

            ConfigureCore(app);

            
            app.UseSwagger()
                .UseSwaggerUI(options =>
                {
                    options.RoutePrefix = "swagger";
                    foreach (var item in ApiInfos)
                        options.SwaggerEndpoint($"{item.Item1}/swagger.json",
                            item.Item2);
                });

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseMiddleware(typeof(ErrorLoggingMiddleware));
            app.UseCors("CorsPolicy");

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseApiVersioning();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();

                endpoints.MapHealthChecks("/health");
            });

            var cookiePolicyOptions = new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Strict,
            };
            app.UseCookiePolicy(cookiePolicyOptions);

        }

        /// <summary>
        /// Конфигурация обработчиков исключений
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        protected virtual void UseCustomExceptionHandling(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMiddleware(typeof(CustomErrorResponseMiddleware));
        }

        /// <summary>
        /// Перегружаемый метод для конфигурации приложения
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        protected virtual IMvcBuilder ConfigureControllersCore(IServiceCollection services)
        {
            var assembly = typeof(WebApiStartupBase).Assembly;

            // Конфигурируем стандартный REST API
            var builder = services.AddControllers(options =>
            {
                options.InputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonInputFormatter>();
                options.OutputFormatters.RemoveType<Microsoft.AspNetCore.Mvc.Formatters.SystemTextJsonOutputFormatter>();
            })
                .AddNewtonsoftJson(opts =>
                {
                    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    opts.SerializerSettings.Converters.Add(new StringEnumConverter(new CamelCaseNamingStrategy()));
                    opts.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

                })
                // .AddXmlSerializerFormatters()
                ;

            // NOTE: добавляем контроллеры из этой сборки
            builder.PartManager.ApplicationParts.Add(new AssemblyPart(assembly));

            return builder;
        }

        /// <summary>
        /// Перегружаемы метод для расширения конфигурации
        /// </summary>
        /// <param name="app"></param>
        protected virtual void ConfigureCore(IApplicationBuilder app) { }
    }
}
>>>>>>> origin/feature/add-auth
