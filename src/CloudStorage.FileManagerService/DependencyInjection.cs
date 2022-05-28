namespace CloudStorage.FileManagerService
{
    /// <summary>
    /// Набор расширений для конфигурации DI сервисов, необходимых для работы FileManagerService
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Сконфигурировать FileManagerService
        /// </summary>
        /// <param name="services"></param>
        /// <param name="conf"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IServiceCollection AddFileManagerService(this IServiceCollection services,
            IConfiguration conf,
            IWebHostEnvironment env)
        {
            var fileManagerServiceOptions = conf.GetSection("FileManagerServiceOptions");
            services.Configure<FileManagerServiceOptions>(fileManagerServiceOptions);

            return services;
        }
    }
}
