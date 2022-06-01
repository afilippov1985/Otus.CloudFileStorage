namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// ������ �������� ������ ��������.
    /// </summary>
    public sealed class NewDirectory
    {
        /// <summary>
        /// �������� �����.
        /// </summary>
        public string Disk { get; set; }

        /// <summary>
        /// ������������ ������������ ��������.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ���� �� ��������.
        /// </summary>
        public string Path { get; set; }
    }
}