using Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileManagerService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFileSystemStorageOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<FileSystemStorageOptions>().
                Bind(configuration.GetSection("FileSystemStorageOptions"));
            return services;
        }
    }
}
