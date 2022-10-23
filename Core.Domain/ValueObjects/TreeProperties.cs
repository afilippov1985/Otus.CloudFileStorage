namespace Core.Domain.ValueObjects
{
    public class TreeProperties
    {
        /// <summary>
        /// Свойства
        /// </summary>
        public Property Props { get; set; }

        
        public TreeProperties(string diskPath, System.IO.FileSystemInfo info)
        {
            Props = new Property() { hasSubdirectories = IsAnySubdirectories(info.FullName) };
        }
        


        private static bool IsAnySubdirectories(string path)
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
