using Core.Domain.Services.Abstractions;

namespace Core.Domain.Messages
{
    public class AbstractMessage
    {
        public string UserId { get; set; }

        public string Disk { get; set; }

        public FileStorageDriver StorageDriver { get; set; }

        public string StorageOptions { get; set; }
    }
}
