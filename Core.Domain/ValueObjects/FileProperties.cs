namespace Core.Domain.ValueObjects
{
    public class FileProperties : DirectoryProperties
    {
        /// <summary>
        /// Имя файла без расширения
        /// </summary>
        public string Filename { get; protected set; }

        /// <summary>
        /// Расширение файла, без точки
        /// </summary>
        public string Extension { get; protected set; }

        /// <summary>
        /// Размер файла
        /// </summary>
        public long Size { get; protected set; }

        public FileProperties(string path, string dirname, string basename, long timestamp, EntityVisibility visibility, string filename, string extension, long size) : base(path, dirname, basename, timestamp, visibility)
        {
            Type = EntityType.File;
            Filename = filename;
            Extension = extension;
            Size = size;
        }
    }
}
