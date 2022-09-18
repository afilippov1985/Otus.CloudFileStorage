namespace FileManagerService.Requests
{
    public class RemoveShareRequest
    {
        public string Disk { get; set; }

        public string Path { get; set; }

        public string PublicId { get; set; }
    }
}
