using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FileManagerService.Requests
{
    public class UploadRequest
    {
        /// <summary>
        /// Название диска-приёмника
        /// </summary>
        public string Disk { get; set; }

        /// <summary>
        /// Путь куда сохранить файл
        /// </summary>
        public string Path { get; set; }

        public int Overwrite { get; set; }

        /// <summary>
        /// Файлы
        /// </summary>
        [BindProperty(Name = "files[]")]
        public IList<IFormFile> Files { get; set; }
    }
}
