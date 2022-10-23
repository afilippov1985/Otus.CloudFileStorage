using System.Collections.Generic;

namespace Core.Domain.Messages
{
    public class ZipMessage : AbstractMessage
    {
        // поместить архив в папку
        public string Path { get; set; }

        // имя архива
        public string Name { get; set; }

        // список папок для архивирования
        public IEnumerable<string> Directories { get; set; }

        // списко файлов для архивирования
        public IEnumerable<string> Files { get; set; }
    }
}
