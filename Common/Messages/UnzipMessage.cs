namespace Common.Messages
{
    public class UnzipMessage
    {
        public string DiskPath { get; set; }

        public string Disk { get; set; }

        // путь к архиву, который нужно распаковать
        public string Path { get; set; }

        // в какую папку распаковать файлы, null если в текущую
        public string? Folder { get; set; }
    }
}
