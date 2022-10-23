namespace Core.Domain.ValueObjects
{
    public class DirectoryProperties
    {
        /// <summary>
        /// Тип - папка или файл
        /// </summary>
        public EntityType Type { get; protected set; }

        /// <summary>
        /// Полный путь от корня диска
        /// Диском мы называем папку, где хранятся файлы пользователя
        /// То есть из пути который выдает FileSystemInfo нужно отрезать путь до папки пользователя
        /// </summary>
        public string Path { get; protected set; }

        /// <summary>
        /// Полный путь от корня диска без имени файла
        /// </summary>
        public string Dirname { get; protected set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string Basename { get; protected set; }

        /// <summary>
        /// Unix timestamp последнего изменения
        /// </summary>
        public long Timestamp { get; protected set; }

        /// <summary>
        /// Видимость
        /// </summary>
        public EntityVisibility Visibility { get; protected set; }

        public DirectoryProperties(string path, string dirname, string basename, long timestamp, EntityVisibility visibility)
        {
            Type = EntityType.Dir;
            Path = path;
            Dirname = dirname;
            Basename = basename;
            Timestamp = timestamp;
            Visibility = visibility;
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
