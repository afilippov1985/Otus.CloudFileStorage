namespace CloudStorage.FileManagerService.Dto
{
    public class DirectoryAttributes
    {
        public const char DirectorySeparatorChar = '/';

        /// <summary>
        /// Тип - папка или файл
        /// </summary>
        public readonly EntityType Type = EntityType.Dir;

        /// <summary>
        /// Полный путь от корня диска
        /// Диском мы называем папку, где хранятся файлы пользователя
        /// То есть из пути который выдает FileSystemInfo нужно отрезать путь до папки пользователя
        /// </summary>
        public string Path;

        /// <summary>
        /// Полный путь от корня диска без имени файла
        /// </summary>
        public string Dirname;

        /// <summary>
        /// Имя файла
        /// </summary>
        public string Basename;

        /// <summary>
        /// Unix timestamp последнего изменения
        /// </summary>
        public Int64 Timestamp;

        /// <summary>
        /// 
        /// </summary>
        public EntityVisibility Visibility = EntityVisibility.Public;

        public DirectoryAttributes(string diskPath, System.IO.FileSystemInfo info)
        {
            Path = info.FullName.Substring(diskPath.Length)
                .TrimStart(System.IO.Path.DirectorySeparatorChar)
                .Replace(System.IO.Path.DirectorySeparatorChar, DirectorySeparatorChar);

            Dirname = new System.IO.DirectoryInfo(info.FullName)
                .Parent.FullName.Substring(diskPath.Length)
                .TrimStart(System.IO.Path.DirectorySeparatorChar)
                .Replace(System.IO.Path.DirectorySeparatorChar, DirectorySeparatorChar);

            Basename = info.Name;
            Timestamp = info.CreationTimeUtc.ToFileTimeUtc() / 10000000 - 11644473600;
        }
    }

    public enum EntityType
    {
        Dir,
        File
    }

    public enum EntityVisibility
    {
        Public,
        Private
    }
}
