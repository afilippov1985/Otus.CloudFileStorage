namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// Модель дерева каталога
    /// </summary>
    public sealed class Tree : DirectoryAttributes
    {
        /// <summary>
        /// Свойства
        /// </summary>
        public Property Props;

        public Tree(string diskPath, System.IO.FileSystemInfo info) : base(diskPath, info)
        {
            Props = new Property() { hasSubdirectories = IsAnySubdirectories(info.FullName) };
        }
        
        private bool IsAnySubdirectories(string path)
        {
            return System.IO.Directory.GetDirectories(path).Length > 0;
        }
    }

    /// <summary>
    /// Свойства каталога
    /// </summary>
    public sealed class Property
    {
        /// <summary>
        /// Наличие подкаталогов
        /// </summary>
        public bool hasSubdirectories;
    }
}