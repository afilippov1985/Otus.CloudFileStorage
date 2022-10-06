namespace Common.Models
{
    public class DirectoryAttributes
    {
        public const char DirectorySeparatorChar = '/';

        /// <summary>
        /// Тип - папка или файл
        /// </summary>
        public EntityType Type { get; set; }

        /// <summary>
        /// Полный путь от корня диска
        /// Диском мы называем папку, где хранятся файлы пользователя
        /// То есть из пути который выдает FileSystemInfo нужно отрезать путь до папки пользователя
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Полный путь от корня диска без имени файла
        /// </summary>
        public string Dirname { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string Basename { get; set; }

        /// <summary>
        /// Unix timestamp последнего изменения
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Видимость
        /// </summary>
        public EntityVisibility Visibility { get; set; }

        public DirectoryAttributes(string diskPath, System.IO.FileSystemInfo info)
        {
            Type = EntityType.Dir;
            Visibility = EntityVisibility.Public;
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
}
