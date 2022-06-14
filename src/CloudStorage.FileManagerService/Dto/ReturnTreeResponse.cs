namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// Модель ответа на запрос дерева каталогов.
    /// </summary>
    public sealed class ReturnTreeResponse
    {
        /// <summary>
        /// Результат выполения
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// Информация о всех каталогах по указанному пути
        /// </summary>
        public IList<Tree> Directories { get; set; }
    }
}