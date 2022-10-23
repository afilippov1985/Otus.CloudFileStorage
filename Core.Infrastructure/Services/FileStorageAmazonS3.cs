using Core.Domain.Services.Abstractions;
using Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Core.Infrastructure.Services
{
    public class FileStorageAmazonS3 : IFileStorage
    {
        public FileStorageAmazonS3(string options)
        {
        }

        public Task<bool> CopyToDirectoryAsync(string fileOrDirPath, string dirPath)
        {
            throw new NotImplementedException();
        }

        public Task<DirectoryProperties> CreateDirectoryAsync(string dirPath, string dirName)
        {
            throw new NotImplementedException();
        }

        public Task<FileProperties> CreateFileAsync(string dirPath, string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteAsync(string fileOrDirPath)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DirectoryProperties>> GetListingAsync(string dirPath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> MoveToDirectoryAsync(string fileOrDirPath, string dirPath)
        {
            throw new NotImplementedException();
        }

        public Task<(FileProperties, Stream)> ReadFileAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RenameAsync(string fileOrDirPathOld, string fileOrDirPathNew)
        {
            throw new NotImplementedException();
        }

        public Task UnzipAsync(string zipFilePath, string? unzipFolder)
        {
            throw new NotImplementedException();
        }

        public Task<FileProperties> WriteToFileAsync(string dirPath, string fileName, Stream stream, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public Task ZipAsync(string? path, string zipFileName, IEnumerable<string> dirs, IEnumerable<string> files)
        {
            throw new NotImplementedException();
        }
    }
}
