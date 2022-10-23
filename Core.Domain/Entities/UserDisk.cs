using Core.Domain.Services.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain.Entities
{
    
    public class UserDisk
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Id пользователя-владельца диска
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Имя диска
        /// </summary>
        public string Disk { get; set; }

        /// <summary>
        /// Тип хранилища (файловое, Amazon S3, другое)
        /// </summary>
        public FileStorageDriver Driver { get; set; }

        public string StorageOptions { get; set; }
    }
}
