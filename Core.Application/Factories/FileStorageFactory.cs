using Core.Domain.Services.Abstractions;
using Core.Domain.Entities;
using Core.Infrastructure.Services;

namespace Core.Application.Factories
{
    public class FileStorageFactory
    {
        // Паттерн Factory Method
        public static IFileStorage? GetFileStorage(FileStorageDriver storageDriver, string storageOptions)
        {
            if (storageDriver == FileStorageDriver.FileSystem)
            {
                return new FileStorageFileSystem(storageOptions);
            }
            else if (storageDriver == FileStorageDriver.AmazonS3)
            {
                return new FileStorageAmazonS3(storageOptions);
            }

            return null;
        }

        public static IFileStorage? GetFileStorage(UserDisk userDisk)
        {
            return GetFileStorage(userDisk.Driver, userDisk.StorageOptions);
        }
    }
}
