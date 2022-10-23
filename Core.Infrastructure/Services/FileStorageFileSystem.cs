using Core.Domain.Services.Abstractions;
using Core.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Infrastructure.Services
{
    public class FileStorageFileSystem : IFileStorage
    {
        private const char SeparatorChar = '/';
        private readonly string _diskPath;

        public FileStorageFileSystem(string diskPath)
        {
            _diskPath = diskPath;

            if (!Directory.Exists(_diskPath))
            {
                Directory.CreateDirectory(_diskPath);
            }
        }

        private string GetContentPath(string? path)
        {
            return path != null ? Path.Combine(_diskPath, path.Replace(SeparatorChar, Path.DirectorySeparatorChar)) : _diskPath;
        }

        private DirectoryProperties ConvertToProperties(FileSystemInfo info)
        {
            string path = info.FullName.Substring(_diskPath.Length)
                .TrimStart(Path.DirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, SeparatorChar);

            string dirname = new DirectoryInfo(info.FullName)
                .Parent!.FullName.Substring(_diskPath.Length)
                .TrimStart(Path.DirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, SeparatorChar);

            string basename = info.Name;

            long timestamp = info.CreationTimeUtc.ToFileTimeUtc() / 10000000 - 11644473600;

            var fileInfo = info as FileInfo;
            if (fileInfo != null)
            {
                timestamp = fileInfo.LastWriteTimeUtc.ToFileTimeUtc() / 10000000 - 11644473600;

                string filename = fileInfo.Name;
                string extension = fileInfo.Extension.TrimStart('.');
                long size = fileInfo.Length;

                return new FileProperties(path, dirname, basename, timestamp, EntityVisibility.Public, filename, extension, size);
            }

            return new DirectoryProperties(path, dirname, basename, timestamp, EntityVisibility.Public);
        }

        public async Task<IEnumerable<DirectoryProperties>> GetListingAsync(string dirPath)
        {
            string contentPath = GetContentPath(dirPath);

            var dirInfo = new DirectoryInfo(contentPath);
            var list = dirInfo.EnumerateFileSystemInfos().Select(x => ConvertToProperties(x));

            return list;
        }

        public async Task<DirectoryProperties> CreateDirectoryAsync(string dirPath, string dirName)
        {
            string contentPath = GetContentPath(dirPath);

            var dirInfo = new DirectoryInfo(contentPath);

            var newDirInfo = dirInfo.CreateSubdirectory(dirName);

            return ConvertToProperties(newDirInfo);
        }

        public async Task<FileProperties> CreateFileAsync(string dirPath, string fileName)
        {
            string contentPath = GetContentPath(dirPath);

            var fileInfo = new FileInfo(Path.Combine(contentPath, fileName));
            if (!fileInfo.Exists)
            {
                fileInfo.Create().Close();
            }

            return (FileProperties)ConvertToProperties(fileInfo);
        }

        public async Task<(FileProperties, Stream)> ReadFileAsync(string filePath)
        {
            string contentPath = GetContentPath(filePath);

            var fileInfo = new FileInfo(contentPath);
            if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
            {
                throw new InvalidOperationException();
            }

            var fileProperties = (FileProperties)ConvertToProperties(fileInfo);

            return (fileProperties, File.OpenRead(contentPath));
        }

        public async Task<FileProperties> WriteToFileAsync(string dirPath, string fileName, Stream stream, bool overwrite)
        {
            string contentPath = GetContentPath(dirPath);
            string filePath = Path.Combine(contentPath, Path.GetFileName(fileName));

            var fileInfo = new FileInfo(filePath);

            if (overwrite || !fileInfo.Exists)
            {
                using var stream1 = new FileStream(filePath, FileMode.Create);
                await stream.CopyToAsync(stream1);
            }

            return (FileProperties)ConvertToProperties(fileInfo);
        }

        public async Task<bool> RenameAsync(string fileOrDirPathOld, string fileOrDirPathNew)
        {
            string src = Path.Combine(_diskPath, fileOrDirPathOld);
            string dst = Path.Combine(_diskPath, fileOrDirPathNew);

            var di = new DirectoryInfo(src);
            if (!di.Exists)
            {
                return false;
            }

            Directory.Move(src, dst);

            return true;
        }

        public async Task<bool> MoveToDirectoryAsync(string fileOrDirPath, string dirPath)
        {
            string src = Path.Combine(_diskPath, fileOrDirPath);
            string dst = Path.Combine(_diskPath, dirPath);

            var di = new DirectoryInfo(src);
            if (!di.Exists)
            {
                return false;
            }

            Directory.Move(src, Path.Combine(dst, di.Name));

            return true;
        }

        public async Task<bool> CopyToDirectoryAsync(string fileOrDirPath, string dirPath)
        {
            bool overwrite = false;
            string src = Path.Combine(_diskPath, fileOrDirPath);
            string dst = Path.Combine(_diskPath, dirPath);

            var di = new DirectoryInfo(src);
            if (!di.Exists)
            {
                return false;
            }

            if (di.Attributes == FileAttributes.Directory)
            {
                CopyDirectory(src, dst, overwrite);
            }
            else
            {
                File.Copy(src, Path.Combine(dst, di.Name), overwrite);
            }

            return true;
        }

        public async Task<bool> DeleteAsync(string fileOrDirPath)
        {
            string src = Path.Combine(_diskPath, fileOrDirPath);

            var di = new DirectoryInfo(src);

            if (di.Attributes == FileAttributes.Directory)
            {
                Directory.Delete(src, true);
            }
            else
            {
                File.Delete(src);
            }

            return true;
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite);
            }

            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, overwrite);
            }
        }

        public async Task UnzipAsync(string zipFilePath, string? unzipFolder)
        {
            var zipFileInfo = new FileInfo(Path.Combine(_diskPath, zipFilePath));
            string extractTo;
            if (string.IsNullOrEmpty(unzipFolder))
            {
                extractTo = zipFileInfo.DirectoryName!;
            }
            else
            {
                extractTo = Path.Combine(zipFileInfo.DirectoryName!, unzipFolder);
            }

            ZipFile.ExtractToDirectory(zipFileInfo.FullName, extractTo, true);
        }

        public async Task ZipAsync(string? path, string zipFileName, IEnumerable<string> dirs, IEnumerable<string> files)
        {
            string? relativePath = path;
            string zipFilePath;
            if (string.IsNullOrEmpty(relativePath))
            {
                zipFilePath = Path.Combine(_diskPath, zipFileName);
            }
            else
            {
                zipFilePath = Path.Combine(_diskPath, relativePath, zipFileName);
            }

            using var stream = File.Open(zipFilePath, FileMode.CreateNew, FileAccess.Write);
            using var zip = new ZipArchive(stream, ZipArchiveMode.Create);

            foreach (string dir in dirs)
            {
                AddDirToZipRecursively(zip, _diskPath, relativePath, dir);
            }

            foreach (var file in files)
            {
                AddFileToZip(zip, Path.Combine(_diskPath, file), relativePath, file);
            }
        }

        private static void AddFileToZip(ZipArchive zip, string realFilePath, string relativePath, string file)
        {
            if (!string.IsNullOrEmpty(relativePath) && file.StartsWith(relativePath))
            {
                file = file.Substring(relativePath.Length + 1);
            }

            zip.CreateEntryFromFile(realFilePath, file);
        }

        private static void AddDirToZipRecursively(ZipArchive zip, string diskPath, string relativePath, string dir)
        {
            var dirInfo = new DirectoryInfo(Path.Combine(diskPath, dir));
            foreach (var fsInfo in dirInfo.EnumerateFileSystemInfos())
            {
                string relativeFullName = fsInfo.FullName.Substring(diskPath.Length + 1);
                DirectoryInfo dirInfo1 = fsInfo as DirectoryInfo;
                if (dirInfo1 != null)
                {
                    AddDirToZipRecursively(zip, diskPath, relativePath, relativeFullName);
                }
                else
                {
                    AddFileToZip(zip, fsInfo.FullName, relativePath, relativeFullName);
                }
            }
        }

    }
}
