namespace CloudStorage.FileManagerService.Dto
{
    public class UnzipRequest
    {
        public string Disk { get; set; }

        // путь к архиву, который нужно распаковать
        public string Path { get; set; }

        // в какую папку распаковать файлы, null если в текущую
        public string? Folder { get; set; }
    }
}
