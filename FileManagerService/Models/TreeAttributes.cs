namespace FileManagerService.Models
{
    public class TreeAttributes : DirectoryAttributes
    {
        /// <summary>
        /// Свойства
        /// </summary>
        public Property Props { get; set; }

        public TreeAttributes(string diskPath, System.IO.FileSystemInfo info) : base(diskPath, info)
        {
            Props = new Property() { hasSubdirectories = IsAnySubdirectories(info.FullName) };
        }

        private bool IsAnySubdirectories(string path)
        {
            return System.IO.Directory.GetDirectories(path).Length > 0;
        }

        public class Property
        {
            /// <summary>
            /// Наличие подкаталогов
            /// </summary>
            public bool hasSubdirectories { get; set; }
        }
    }
}
