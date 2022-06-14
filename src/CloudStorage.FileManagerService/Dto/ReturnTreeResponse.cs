namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// ������ ������ �� ������ ������ ���������.
    /// </summary>
    public sealed class ReturnTreeResponse
    {
        /// <summary>
        /// ��������� ���������
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// ���������� � ���� ��������� �� ���������� ����
        /// </summary>
        public IList<Tree> Directories { get; set; }
    }
}