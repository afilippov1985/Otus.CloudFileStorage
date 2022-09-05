namespace FileManagerService.Models
{
    public class FileAttributes : DirectoryAttributes
    {
        /// <summary>
        /// Имя файла без расширения
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Расширение файла, без точки
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Размер файла
        /// </summary>
        public long Size { get; set; }

        public FileAttributes(string diskPath, System.IO.FileInfo info) : base(diskPath, info)
        {
            Type = EntityType.File;
            Timestamp = info.LastWriteTimeUtc.ToFileTimeUtc() / 10000000 - 11644473600;

            Filename = info.Name;
            Extension = info.Extension.TrimStart('.');
            Size = info.Length;
        }
    }
}
