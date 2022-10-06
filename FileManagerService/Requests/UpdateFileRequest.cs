using Microsoft.AspNetCore.Http;

namespace FileManagerService.Requests
{
    public class UpdateFileRequest
    {
        /// <summary>
        /// Название диска-приёмника
        /// </summary>
        public string Disk { get; set; }

        /// <summary>
        /// Путь куда сохранить файл
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Новое содержимое файла
        /// </summary>
        public IFormFile File { get; set; }
    }
}
