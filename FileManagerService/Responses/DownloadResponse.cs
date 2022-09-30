using System.IO;

namespace FileManagerService.Responses
{
    public class DownloadResponse
    {
        public string NameFile { get; set; }
        public string ContentPath { get; set; }
        public string ContentType { get; set; }
    }
}
