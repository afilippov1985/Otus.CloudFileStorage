namespace Common.Queries
{
    public class UnzipQuery
    {
        public string Disk { get; set; }

        // путь к архиву, который нужно распаковать
        public string Path { get; set; }

        // в какую папку распаковать файлы, null если в текущую
        public string? Folder { get; set; }
    }
}
