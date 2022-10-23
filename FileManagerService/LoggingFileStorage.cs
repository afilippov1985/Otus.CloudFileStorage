using Core.Domain.Services.Abstractions;
using Core.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileManagerService
{
    // паттерн Decorator
    // используем композицию. Оборачиваем вызовы методов IFileStorage, чтобы добавить логирование
    public class LoggingFileStorage : IFileStorage
    {
        private IFileStorage _storage;
        private ILogger _logger;

        public LoggingFileStorage(IFileStorage storage, ILogger logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public Task<bool> CopyToDirectoryAsync(string fileOrDirPath, string dirPath)
        {
            _logger.LogInformation("CopyToDirectoryAsync '{0}' '{1}'", fileOrDirPath, dirPath);
            return _storage.CopyToDirectoryAsync(fileOrDirPath, dirPath);
        }

        public Task<DirectoryProperties> CreateDirectoryAsync(string dirPath, string dirName)
        {
            _logger.LogInformation("CreateDirectoryAsync '{0}' '{1}'", dirPath, dirName);
            return _storage.CreateDirectoryAsync(dirPath, dirName);
        }

        public Task<FileProperties> CreateFileAsync(string dirPath, string fileName)
        {
            _logger.LogInformation("CreateFileAsync '{0}' '{1}'", dirPath, fileName);
            return _storage.CreateFileAsync(dirPath, fileName);
        }

        public Task<bool> DeleteAsync(string fileOrDirPath)
        {
            _logger.LogInformation("DeleteAsync '{0}'", fileOrDirPath);
            return _storage.DeleteAsync(fileOrDirPath);
        }

        public Task<IEnumerable<DirectoryProperties>> GetListingAsync(string dirPath)
        {
            _logger.LogInformation("GetListingAsync '{0}'", dirPath);
            return _storage.GetListingAsync(dirPath);
        }

        public Task<bool> MoveToDirectoryAsync(string fileOrDirPath, string dirPath)
        {
            _logger.LogInformation("MoveToDirectoryAsync '{0}' '{1}'", fileOrDirPath, dirPath);
            return _storage.MoveToDirectoryAsync(fileOrDirPath, dirPath);
        }

        public Task<(FileProperties, Stream)> ReadFileAsync(string filePath)
        {
            _logger.LogInformation("GetListingAsync '{0}'", filePath);
            return _storage.ReadFileAsync(filePath);
        }

        public Task<bool> RenameAsync(string fileOrDirPathOld, string fileOrDirPathNew)
        {
            _logger.LogInformation("RenameAsync '{0}' '{1}'", fileOrDirPathOld, fileOrDirPathNew);
            return _storage.RenameAsync(fileOrDirPathOld, fileOrDirPathNew);
        }

        public Task UnzipAsync(string zipFilePath, string? unzipFolder)
        {
            _logger.LogInformation("UnzipAsync '{0}' '{1}'", zipFilePath, unzipFolder);
            return _storage.UnzipAsync(zipFilePath, unzipFolder);
        }

        public Task<FileProperties> WriteToFileAsync(string dirPath, string fileName, Stream stream, bool overwrite)
        {
            _logger.LogInformation("WriteToFileAsync '{0}' '{1}'", dirPath, fileName);
            return _storage.WriteToFileAsync(dirPath, fileName, stream, overwrite);
        }

        public Task ZipAsync(string? path, string zipFileName, IEnumerable<string> dirs, IEnumerable<string> files)
        {
            _logger.LogInformation("ZipAsync '{0}' '{1}'", path, zipFileName);
            return _storage.ZipAsync(path, zipFileName, dirs, files);
        }
    }
}
