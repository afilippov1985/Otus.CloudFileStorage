using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveService
{
    internal class ZipConsumer : IConsumer<Messages.ZipMessage>
    {
        private readonly ILogger<ZipConsumer> _logger;

        public ZipConsumer(ILogger<ZipConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Messages.ZipMessage> context)
        {
            _logger.LogInformation("ZipConsumer Run: {0} {1}", context.Message.DiskPath, context.Message.Name);

            ThreadPool.QueueUserWorkItem(DoZip, context.Message, false);
        }

        private void DoZip(Messages.ZipMessage message)
        {
            try
            {
                string diskPath = message.DiskPath;
                string? relativePath = message.Path;
                string zipFilePath;
                if (string.IsNullOrEmpty(relativePath))
                {
                    zipFilePath = Path.Combine(diskPath, message.Name);
                }
                else
                {
                    zipFilePath = Path.Combine(diskPath, relativePath, message.Name);
                }

                using var stream = File.Open(zipFilePath, FileMode.CreateNew, FileAccess.Write);
                using var zip = new ZipArchive(stream, ZipArchiveMode.Create);

                foreach (string dir in message.Directories)
                {
                    AddDirRecursively(zip, diskPath, relativePath, dir);
                }

                foreach (var file in message.Files)
                {
                    AddFile(zip, Path.Combine(diskPath, file), relativePath, file);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ZipConsumer Error");
            }
        }

        private static void AddFile(ZipArchive zip, string realFilePath, string relativePath, string file)
        {
            if (!string.IsNullOrEmpty(relativePath) && file.StartsWith(relativePath))
            {
                file = file.Substring(relativePath.Length + 1);
            }

            zip.CreateEntryFromFile(realFilePath, file);
        }

        private void AddDirRecursively(ZipArchive zip, string diskPath, string relativePath, string dir)
        {
            var dirInfo = new DirectoryInfo(Path.Combine(diskPath, dir));
            foreach (var fsInfo in dirInfo.EnumerateFileSystemInfos())
            {
                string relativeFullName = fsInfo.FullName.Substring(diskPath.Length + 1);
                DirectoryInfo dirInfo1 = fsInfo as DirectoryInfo;
                if (dirInfo1 != null)
                {
                    AddDirRecursively(zip, diskPath, relativePath, relativeFullName);
                }
                else
                {
                    AddFile(zip, fsInfo.FullName, relativePath, relativeFullName);
                }
            }
        }
    }
}
