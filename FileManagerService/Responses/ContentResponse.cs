using System.Collections.Generic;
using FileManagerService.Models;

namespace FileManagerService.Responses
{
    public class ContentResponse
    {
        public Result Result { get; set; }

        public IList<DirectoryAttributes> Directories { get; set; }

        public IList<FileAttributes> Files { get; set; }
    }
}
