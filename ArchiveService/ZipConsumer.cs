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

        public ZipConsumer(ILogger<ZipConsumer> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        private IFileStorage GetFileStorage(string userId, string diskName)
        {
            var userDisk = _db.GetUserDisk(userId, diskName);
            return FileStorageFactory.GetFileStorage(userDisk);
        }

        private IFileStorage GetFileStorage2(FileStorageDriver storageDriver, string storageOptions)
        {
            return FileStorageFactory.GetFileStorage(storageDriver, storageOptions);
        }

        public async Task Consume(ConsumeContext<ZipMessage> context)
        {
            _logger.LogInformation("ZipConsumer Run: {0} {1} {2}", context.Message.UserId, context.Message.Path, context.Message.Name);

            // многопоточность
            ThreadPool.QueueUserWorkItem(DoZip, context.Message, false);
        }

        protected virtual void DoZip(ZipMessage message)
        {
            try
            {
                // var storage = GetFileStorage(message.UserId, message.Disk);
                var storage = GetFileStorage2(message.StorageDriver, message.StorageOptions);
                storage.ZipAsync(message.Path, message.Name, message.Directories, message.Files).GetAwaiter().GetResult(); ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ZipConsumer Error");
            }
        }
    }
}
