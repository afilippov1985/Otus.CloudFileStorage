namespace Common.Messages
{
    public class ZipMessage
    {
        public string DiskPath { get; set; }

        public string Disk { get; set; }

        // поместить архив в папку
        public string? Path { get; set; }

        // имя архива
        public string Name { get; set; }

        // список папок для архивирования
        public IList<string> Directories { get; set; }

        // списко файлов для архивирования
        public IList<string> Files { get; set; }
    }
}
