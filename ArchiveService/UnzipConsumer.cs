using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveService
{
    internal class UnzipConsumer : IConsumer<Messages.UnzipMessage>
    {
        private readonly ILogger<ZipConsumer> _logger;

        public UnzipConsumer(ILogger<ZipConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Messages.UnzipMessage> context)
        {
            _logger.LogInformation("UnzipConsumer Run: {0} {1}", context.Message.DiskPath, context.Message.Folder);

            ThreadPool.QueueUserWorkItem(DoUnzip, context.Message, false);
        }

        public void DoUnzip(Messages.UnzipMessage message)
        {
            try
            {
                string baseDir = message.DiskPath;
                var zipFileInfo = new FileInfo(Path.Combine(baseDir, message.Path));
                string extractTo;
                if (string.IsNullOrEmpty(message.Folder))
                {
                    extractTo = zipFileInfo.DirectoryName;
                }
                else
                {
                    extractTo = Path.Combine(zipFileInfo.DirectoryName, message.Folder);
                }

                ZipFile.ExtractToDirectory(zipFileInfo.FullName, extractTo, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UnzipConsumer Error");
            }
        }
    }
}
