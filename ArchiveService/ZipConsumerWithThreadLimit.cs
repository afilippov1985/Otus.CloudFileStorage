using Core.Domain.Messages;
using Core.Infrastructure.DataAccess;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace ArchiveService
{
    internal class ZipConsumerWithThreadLimit : ZipConsumer
    {
        private readonly Semaphore _semaphore;

        public ZipConsumerWithThreadLimit(ILogger<ZipConsumerWithThreadLimit> logger, ApplicationDbContext db) : base(logger, db)
        {
            _semaphore = new Semaphore(0, 2);
        }

        // паттерн Proxy
        // к методу добавляется ограничение на количество потоков, которое может выполняться одновременно
        protected override void DoZip(ZipMessage message)
        {
            _semaphore.WaitOne();

            try
            {
                base.DoZip(message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
