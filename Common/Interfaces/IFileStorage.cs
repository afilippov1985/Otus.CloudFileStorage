using Common.Results;
using Common.Queries;

namespace Common.Interfaces
{
    public interface IFileStorage
    {
        public InitializeResult InitializeManager(string AuthenticatedUserId);
        public ContentResult StorageContent(string path, string AuthenticatedUserId);
        public CreateDirectoryResult CreateDirectory(CreateDirectoryQuery request, string AuthenticatedUserId);
        public CreateFileResult CreateFile(CreateFileQuery request, string AuthenticatedUserId);
        public ResultResult Upload(UploadQuery request, string AuthenticatedUserId);
        public ResultResult Rename(RenameQuery request, string AuthenticatedUserId);
        public ResultResult Delete(DeleteQuery request, string AuthenticatedUserId);
        public ResultResult Paste(PasteQuery request, string AuthenticatedUserId);
        public UpdateFileResult UpdateFile(UpdateFileQuery fileRequest, string AuthenticatedUserId);
        public TreeResult Tree(string path, string AuthenticatedUserId);
        public DownloadResult Preview(string path, string AuthenticatedUserId);
        public DownloadResult Download(string path, string AuthenticatedUserId);
    }
}
