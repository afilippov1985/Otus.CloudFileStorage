using FileManagerService.Interfaces;
using FileManagerService.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FileManagerService.Controllers
{
    [Route("/file-manager/[action]")]
    public class FileManagerController : ControllerBase
    {
        private readonly IFileStorage _fileSystemStorage;

        public FileManagerController(IFileStorage fileSystemStorage)
        {
            _fileSystemStorage = fileSystemStorage;
        }

        private string GetAuthenticatedUserId()
        {
            var claim = HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                throw new UnauthorizedAccessException();
            }
            return claim.Value;
        }

        [HttpGet]
        [ActionName("initialize")]
        public IActionResult InitializeManager()
        {
            return Ok(_fileSystemStorage.InitializeManager(GetAuthenticatedUserId()));
        }

        [HttpGet]
        [ActionName("content")]
        public IActionResult StorageContent([FromQuery(Name = "path")] string path)
        {
            return Ok(_fileSystemStorage.StorageContent(path, GetAuthenticatedUserId()));
        }

        [HttpPost]
        [ActionName("create-directory")]
        public IActionResult CreateDirectory([FromBody] CreateDirectoryRequest request)
        {
            return Ok(_fileSystemStorage.CreateDirectory(request, GetAuthenticatedUserId()));
        }

        /// <summary>
        /// запрос на содание файла
        /// </summary>
        [HttpPost]
        [ActionName("create-file")]
        public IActionResult CreateFile([FromBody] CreateFileRequest request)
        {
            var response = _fileSystemStorage.CreateFile(request, GetAuthenticatedUserId());
            if (response == null)
                return UnprocessableEntity();
            else return Ok(response);
        }

        [HttpPost]
        [ActionName("upload")]
        public IActionResult Upload([FromForm] UploadRequest request)
        {
            return Ok(_fileSystemStorage.Upload(request, GetAuthenticatedUserId()));
        }

        [HttpPost]
        [ActionName("rename")]
        public IActionResult Rename([FromBody] RenameRequest request)
        {
            var response = _fileSystemStorage.Rename(request, GetAuthenticatedUserId());
            if (response == null)
                return UnprocessableEntity();
            else return Ok(response);
        }

        [HttpPost]
        [ActionName("delete")]
        public IActionResult Delete([FromBody] DeleteRequest request)
        {
            var response = _fileSystemStorage.Delete(request, GetAuthenticatedUserId());
            if (response == null)
                return UnprocessableEntity();
            else return Ok(response);
        }

        [HttpPost]
        [ActionName("paste")]
        public IActionResult Paste([FromBody] PasteRequest request)
        {
            return Ok(_fileSystemStorage.Paste(request, GetAuthenticatedUserId()));
        }

        [HttpPost]
        [ActionName("update-file")]
        public IActionResult UpdateFile([FromForm] UpdateFileRequest fileRequest)
        {
            var response = _fileSystemStorage.UpdateFile(fileRequest, GetAuthenticatedUserId());
            if (response == null)
                return UnprocessableEntity();
            else return Ok(response);
        }

        [HttpGet]
        [ActionName("tree")]
        public IActionResult Tree([FromQuery(Name = "path")] string path)
        {
            return Ok(_fileSystemStorage.Tree(path, GetAuthenticatedUserId()));
        }

        [HttpGet]
        [ActionName("preview")]
        public IActionResult Preview([FromQuery(Name = "path")] string path)
        {
            var response = _fileSystemStorage.Preview(path, GetAuthenticatedUserId());
            return PhysicalFile(response.ContentPath, response.ContentType);
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [ActionName("download")]
        public IActionResult Download([FromQuery(Name = "path")] string path)
        {
            var response = _fileSystemStorage.Preview(path, GetAuthenticatedUserId());
            return PhysicalFile(response.ContentPath, response.ContentType, response.NameFile);
        }

        [HttpPost]
        [ActionName("zip")]
        public async Task<IActionResult> Zip([FromBody] ZipRequest request)
        {
            return Ok(await _fileSystemStorage.Zip(request, GetAuthenticatedUserId()));
        }

        [HttpPost]
        [ActionName("unzip")]
        public async Task<IActionResult> Unzip([FromBody] UnzipRequest request)
        {
            return Ok(await _fileSystemStorage.Unzip(request, GetAuthenticatedUserId()));
        }

        [HttpPost]
        [ActionName("addShare")]
        public async Task<IActionResult> AddShare([FromBody] AddShareRequest request)
        {
            return Ok(await _fileSystemStorage.AddShare(request, GetAuthenticatedUserId()));
        }

        [HttpPost]
        [ActionName("removeShare")]
        public async Task<IActionResult> RemoveShare([FromBody] RemoveShareRequest request)
        {
            return Ok(await _fileSystemStorage.RemoveShare(request, GetAuthenticatedUserId()));
        }
    }
}
