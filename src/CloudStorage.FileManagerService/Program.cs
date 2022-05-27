#nullable disable

using CloudStorage.WebApi.Shared;

namespace CloudStorage.FileManagerService
{
    internal static class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Split('.').LastOrDefault();

        public static int Main(string[] args)
        {
            return ProgramExtensions.InitWrap<Startup>(AppName, args, (host, logger) =>
            {

            });
        }
    }
}