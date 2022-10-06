using Common.Models;

namespace Common.Results
{
    public sealed class ContentResult
    {
        public Result? Result { get; set; }

        public IList<DirectoryAttributes>? Directories { get; set; }

        public IList<Models.FileAttributes>? Files { get; set; }
    }
}
