using Microsoft.Extensions.Configuration;

namespace GeoAnalytics.WebApi.Shared.Helpers
{
    /// <summary>
    /// Набор вспомогательных методов для работы с конфигурацией
    /// </summary>
    public static class ConfigurationExtensions
    {

        /// <summary>
        /// Возвращает заданный в настройках базовый путь API 
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static string GetBasePath(this IConfiguration configuration)
        {
            return configuration.GetValue<string>("PATH_BASE");
        }
    }
}
