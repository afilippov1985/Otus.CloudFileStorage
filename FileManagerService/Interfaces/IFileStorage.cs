using FileManagerService.Requests;
using FileManagerService.Responses;
using System.Threading.Tasks;

namespace FileManagerService.Interfaces
{
    public interface IFileStorage
    {
        public InitializeResponse InitializeManager(string AuthenticatedUserId);
        public ContentResponse StorageContent(string path, string AuthenticatedUserId);
        public CreateDirectoryResponse CreateDirectory(CreateDirectoryRequest request, string AuthenticatedUserId);
        public CreateFileResponse CreateFile(CreateFileRequest request, string AuthenticatedUserId);
        public ResultResponse Upload(UploadRequest request, string AuthenticatedUserId);
        public ResultResponse Rename(RenameRequest request, string AuthenticatedUserId);
        public ResultResponse Delete(DeleteRequest request, string AuthenticatedUserId);
        public ResultResponse Paste(PasteRequest request, string AuthenticatedUserId);
        public UpdateFileResponse UpdateFile(UpdateFileRequest fileRequest, string AuthenticatedUserId);
        public TreeResponse Tree(string path, string AuthenticatedUserId);
        public DownloadResponse Preview(string path, string AuthenticatedUserId);
        public DownloadResponse Download(string path, string AuthenticatedUserId);
        public Task<ResultResponse> Zip(ZipRequest request, string AuthenticatedUserId);
        public Task<ResultResponse> Unzip(UnzipRequest request, string AuthenticatedUserId);
        public Task<AddShareResponse> AddShare(AddShareRequest request, string AuthenticatedUserId);
        public Task<RemoveShareResponse> RemoveShare(RemoveShareRequest request, string AuthenticatedUserId);
    }
}