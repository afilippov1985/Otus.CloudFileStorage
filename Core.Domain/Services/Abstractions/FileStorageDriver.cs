namespace Core.Domain.Services.Abstractions
{
    public enum FileStorageDriver: int
    {
        FileSystem,
        AmazonS3,
    }
}
