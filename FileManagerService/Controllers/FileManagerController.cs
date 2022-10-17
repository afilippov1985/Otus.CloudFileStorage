using Common.Interfaces;
using Common.Queries;
using Common.Models;
using FileManagerService.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FileManagerService.Responses;
using System.IO;
using MassTransit;
using ArchiveService.Messages;
using Microsoft.Extensions.Options;
using Common.Data;
using System.Linq;

namespace FileManagerService.Controllers
{
    [Route("/file-manager/[action]")]
    public class FileManagerController : ControllerBase
    {
        private readonly IFileStorage _fileSystemStorage;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IOptionsMonitor<FileSystemStorageOptions> _options;
        private readonly ApplicationDbContext _db;

        public FileManagerController(
            IFileStorage fileSystemStorage,
            IPublishEndpoint publishEndpoint,
            IOptionsMonitor<FileSystemStorageOptions> options,
            ApplicationDbContext db)
        {
            _fileSystemStorage = fileSystemStorage;
            _publishEndpoint = publishEndpoint;
            _options = options;
            _db = db;
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
            var query = new CreateDirectoryQuery
            {
                Path = request.Path,
                Disk = request.Disk,
                Name = request.Name
            };
            return Ok(_fileSystemStorage.CreateDirectory(query, GetAuthenticatedUserId()));
        }

        /// <summary>
        /// запрос на содание файла
        /// </summary>
        [HttpPost]
        [ActionName("create-file")]
        public IActionResult CreateFile([FromBody] CreateFileRequest request)
        {
            var query = new CreateFileQuery
            {
                Path = request.Path,
                Disk = request.Disk,
                Name = request.Name
            };
            var response = _fileSystemStorage.CreateFile(query, GetAuthenticatedUserId());
            if (response == null)
                return UnprocessableEntity();
            else return Ok(response);
        }

        [HttpPost]
        [ActionName("upload")]
        public IActionResult Upload([FromForm] UploadRequest request)
        {
            var query = new UploadQuery
            {
                Path = request.Path,
                Disk = request.Disk,
                Overwrite = request.Overwrite,
                Files = request.Files
            };
            return Ok(_fileSystemStorage.Upload(query, GetAuthenticatedUserId()));
        }

        [HttpPost]
        [ActionName("rename")]
        public IActionResult Rename([FromBody] RenameRequest request)
        {
            var query = new RenameQuery
            {
                Disk = request.Disk,
                Type = (DirectoryAttributes.EntityType)request.Type,
                OldName = request.OldName,
                NewName = request.NewName
            };
            var response = _fileSystemStorage.Rename(query, GetAuthenticatedUserId());
            if (response == null)
                return UnprocessableEntity();
            else return Ok(response);
        }

        [HttpPost]
        [ActionName("delete")]
        public IActionResult Delete([FromBody] DeleteRequest request)
        {
            var query = new DeleteQuery
            {
                Disk = request.Disk,
                Items = (IList<DeleteQuery.Item>)request.Items
            };
            var response = _fileSystemStorage.Delete(query, GetAuthenticatedUserId());
            if (response == null)
                return UnprocessableEntity();
            else return Ok(response);
        }

        [HttpPost]
        [ActionName("paste")]
        public IActionResult Paste([FromBody] PasteRequest request)
        {
            var query = new PasteQuery
            {
                Path = request.Path,
                Disk = request.Disk,
                Clipboard = new PasteQuery.ClipboardObject
                {
                    Disk = request.Clipboard.Disk,
                    Directories = request.Clipboard.Directories,
                    Files = request.Clipboard.Files,
                    Type = (PasteQuery.ClipboardObject.ClipboardType)request.Clipboard.Type
                }
            };
            return Ok(_fileSystemStorage.Paste(query, GetAuthenticatedUserId()));
        }

        [HttpPost]
        [ActionName("update-file")]
        public IActionResult UpdateFile([FromForm] UpdateFileRequest fileRequest)
        {
            var query = new UpdateFileQuery
            {
                Path = fileRequest.Path,
                Disk = fileRequest.Disk,
                File = fileRequest.File
            };
            var response = _fileSystemStorage.UpdateFile(query, GetAuthenticatedUserId());
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
            string diskPath = GetDiskPath(request.Disk);
            try
            {
                await _publishEndpoint.Publish<ZipMessage>(new
                {
                    DiskPath = diskPath,
                    Disk = request.Disk,
                    Path = request.Path,
                    Name = request.Name,
                    Directories = request.Elements.Directories,
                    Files = request.Elements.Files,
                });

                System.Threading.Thread.Sleep(1000);

                return Ok(new ResultResponse(Status.Success, ""));
            }
            catch
            {
                return Ok(new ResultResponse(Status.Warning, "zipError"));
            }
        }

        [HttpPost]
        [ActionName("unzip")]
        public async Task<IActionResult> Unzip([FromBody] UnzipRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);

            try
            {
                await _publishEndpoint.Publish<ArchiveService.Messages.UnzipMessage>(new
                {
                    DiskPath = diskPath,
                    Disk = request.Disk,
                    Path = request.Path,
                    Folder = request.Folder,
                });

                System.Threading.Thread.Sleep(1000);

                return Ok(new ResultResponse(Status.Success, ""));
            }
            catch
            {
                return Ok(new ResultResponse(Status.Warning, "zipError"));
            }
        }

        [HttpPost]
        [ActionName("addShare")]
        public async Task<IActionResult> AddShare([FromBody] AddShareRequest request)
        {
            var share = new Share()
            {
                UserId = GetAuthenticatedUserId(),
                Disk = request.Disk,
                Path = request.Path,
                CreatedAt = DateTime.UtcNow,
            };

            _db.Shares.Add(share);
            await _db.SaveChangesAsync();

            return Ok(new AddShareResponse()
            {
                Disk = share.Disk,
                Path = share.Path,
                PublicId = share.PublicId,
            });
        }

        [HttpPost]
        [ActionName("removeShare")]
        public async Task<IActionResult> RemoveShare([FromBody] RemoveShareRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);

            var share = _db.Shares.Where(x => x.PublicId == request.PublicId).FirstOrDefault();
            if (share != null)
            {
                _db.Shares.Remove(share);
                await _db.SaveChangesAsync();
            }

            return Ok(new RemoveShareResponse()
            {
                Disk = request.Disk,
                Path = request.Path,
            });
        }

        private string GetDiskPath(string AuthenticatedUserId)
        {
            var dir = Path.Combine(_options.CurrentValue.StoragePath, AuthenticatedUserId);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }
    }
}
