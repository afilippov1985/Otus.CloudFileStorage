namespace Core.Domain.Messages
{
    public class UnzipMessage : AbstractMessage
    {
        // путь к архиву, который нужно распаковать
        public string Path { get; set; }

        // в какую папку распаковать файлы, null если в текущую
        public string? Folder { get; set; }
    }
}
