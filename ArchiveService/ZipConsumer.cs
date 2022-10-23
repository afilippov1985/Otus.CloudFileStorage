using Core.Application.Factories;
using Core.Domain.Messages;
using Core.Domain.Services.Abstractions;
using Core.Infrastructure.DataAccess;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveService
{
    internal class ZipConsumer : IConsumer<ZipMessage>
    {
        private readonly ILogger<ZipConsumer> _logger;
        private readonly ApplicationDbContext _db;
        private readonly Semaphore _semaphore;

        public ZipConsumer(ILogger<ZipConsumer> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
            _semaphore = new Semaphore(0, 2);
        }

        private IFileStorage GetFileStorage(string userId, string diskName)
        {
            var userDisk = _db.GetUserDisk(userId, diskName);
            return FileStorageFactory.GetFileStorage(userDisk);
        }

        public async Task Consume(ConsumeContext<ZipMessage> context)
        {
            _logger.LogInformation("ZipConsumer Run: {0} {1} {2}", context.Message.UserId, context.Message.Path, context.Message.Name);

            // многопоточность
            ThreadPool.QueueUserWorkItem(DoZip, context.Message, false);
        }

        private void DoZip(ZipMessage message)
        {
            // ограничение количества потоков, которое может выполняться одновременно
            _semaphore.WaitOne();

            try
            {
                var storage = GetFileStorage(message.UserId, message.Disk);
                storage.Zip(message.Path, message.Name, message.Directories, message.Files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ZipConsumer Error");
            }

            _semaphore.Release();
        }
    }
}
