using System.Collections.Generic;

namespace FileManagerService.Requests
{
    public class ZipRequest
    {
        public class ZipElements
        {
            // список папок для архивирования
            public IList<string> Directories { get; set; }

            // списко файлов для архивирования
            public IList<string> Files { get; set; }
        }

        public string Disk { get; set; }

        // поместить архив в папку
        public string? Path { get; set; }

        // имя архива
        public string Name { get; set; }

        public ZipElements Elements { get; set; }
    }
}
