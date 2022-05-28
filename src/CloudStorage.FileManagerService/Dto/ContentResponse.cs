namespace CloudStorage.FileManagerService.Dto
{
    public class ContentResponse
    {
        public Result Result { get; set; }

        public IList<DirectoryAttributes> Directories { get; set; }

        public IList<FileAttributes> Files { get; set; }
    }
}
