namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// ������ ������ ��������
    /// </summary>
    public sealed class Tree : DirectoryAttributes
    {
        /// <summary>
        /// ��������
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
    /// �������� ��������
    /// </summary>
    public sealed class Property
    {
        /// <summary>
        /// ������� ������������
        /// </summary>
        public bool hasSubdirectories;
    }
}