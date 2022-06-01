namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// ������ ������ �� �������� ������ ��������.
    /// </summary>
    public sealed class TreeResponse
    {
        /// <summary>
        /// ��������� ���������
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// ���������� � ��������� ��������
        /// </summary>
        public DirectoryAttributes Directory { get; set; }

        /// <summary>
        /// ���������� � ���� ��������� �� ���������� ����
        /// </summary>
        public IList<Tree> Tree { get; set; }
    }
}