using Core.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Domain.Services.Abstractions
{
    public interface IFileStorage
    {
        public Task<IEnumerable<DirectoryProperties>> GetListingAsync(string dirPath);
        public Task<DirectoryProperties> CreateDirectoryAsync(string dirPath, string dirName);
        public Task<FileProperties> CreateFileAsync(string dirPath, string fileName);
        public Task<(FileProperties, System.IO.Stream)> ReadFileAsync(string filePath);
        public Task<FileProperties> WriteToFileAsync(string dirPath, string fileName, System.IO.Stream stream, bool overwrite);
        public Task<bool> RenameAsync(string fileOrDirPathOld, string fileOrDirPathNew);
        public Task<bool> MoveToDirectoryAsync(string fileOrDirPath, string dirPath);
        public Task<bool> CopyToDirectoryAsync(string fileOrDirPath, string dirPath);
        public Task<bool> DeleteAsync(string fileOrDirPath);
        public Task Unzip(string zipFilePath, string? unzipFolder);
        public Task Zip(string? path, string zipFileName, IEnumerable<string> dirs, IEnumerable<string> files);
    }
}
