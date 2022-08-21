using Npgsql;
using System.Data.Entity;

namespace CloudStorage.FileManagerService
{
    public class ApplicationDbConfiguration : DbConfiguration
    {
        public ApplicationDbConfiguration()
        {
            string name = "Npgsql";

            SetProviderFactory(name, NpgsqlFactory.Instance);

            SetProviderServices(name, NpgsqlServices.Instance);

            SetDefaultConnectionFactory(new NpgsqlConnectionFactory());
        }
    }
}
