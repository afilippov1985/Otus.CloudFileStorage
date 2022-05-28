namespace CloudStorage.FileManagerService.Dto
{
    public class FileAttributes : DirectoryAttributes
    {
        public readonly new EntityType Type = EntityType.File;

        /// <summary>
        /// Имя файла без расширения
        /// </summary>
        public string Filename;

        /// <summary>
        /// Расширение файла, без точки
        /// </summary>
        public string Extension;

        /// <summary>
        /// Размер файла
        /// </summary>
        public Int64 Size;

        public FileAttributes(string diskPath, System.IO.FileInfo info) : base(diskPath, info)
        {
            Timestamp = info.LastWriteTimeUtc.ToFileTimeUtc() / 10000000 - 11644473600;

            Filename = info.Name;
            Extension = info.Extension.TrimStart('.');
            Size = info.Length;
        }
    }
}
