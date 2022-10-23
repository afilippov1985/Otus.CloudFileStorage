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

        public UnzipConsumer(ILogger<ZipConsumer> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
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
            try
            {
                var storage = GetFileStorage(message.UserId, message.Disk);
                storage.UnzipAsync(message.Path, message.Folder).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnzipConsumer Error");
            }
        }
    }
}
