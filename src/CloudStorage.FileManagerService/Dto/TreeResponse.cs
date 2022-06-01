namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// Модель ответа на создание нового каталога.
    /// </summary>
    public sealed class TreeResponse
    {
        /// <summary>
        /// Результат выполения
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// Информация о созданном каталоге
        /// </summary>
        public DirectoryAttributes Directory { get; set; }

        /// <summary>
        /// Информация о всех каталогах по указанному пути
        /// </summary>
        public IList<Tree> Tree { get; set; }
    }
}