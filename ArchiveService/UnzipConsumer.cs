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
    internal class UnzipConsumer : IConsumer<UnzipMessage>
    {
        private readonly ILogger<ZipConsumer> _logger;
        private readonly ApplicationDbContext _db;
        private readonly Semaphore _semaphore;

        public UnzipConsumer(ILogger<ZipConsumer> logger, ApplicationDbContext db)
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

        public async Task Consume(ConsumeContext<UnzipMessage> context)
        {
            _logger.LogInformation("UnzipConsumer Run: {0} {1} {2}", context.Message.UserId, context.Message.Path, context.Message.Folder);

            // многопоточность
            ThreadPool.QueueUserWorkItem(DoUnzip, context.Message, false);
        }

        public void DoUnzip(UnzipMessage message)
        {
            // ограничение количества потоков, которое может выполняться одновременно
            _semaphore.WaitOne();

            try
            {
                var storage = GetFileStorage(message.UserId, message.Disk);
                storage.Unzip(message.Path, message.Folder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnzipConsumer Error");
            }

            _semaphore.Release();
        }
    }
}
