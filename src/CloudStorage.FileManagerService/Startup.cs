using CloudStorage.WebApi.Shared;
using Microsoft.OpenApi.Models;

namespace CloudStorage.FileManagerService
{
    internal class Startup : WebApiStartupBase
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
        {
        }

        protected override List<Tuple<string, string, OpenApiInfo>> ApiInfos => new()
        {
            new("v1.0", "File Manager Service API v1.0", new OpenApiInfo
            {
                Title = "CloudStorage File Manager Service HTTP API",
                Version = "v1.0",
                Description = "Сервис для управления файлами облачного хранилища. Пример работы <a href = '../example.html'>Демо</a>"
            })
        };

        protected override void ConfigureServicesCore(IServiceCollection services)
        {
            base.ConfigureServicesCore(services);
            services.AddFileManagerService(Configuration, Environment);
        }
    }
}
