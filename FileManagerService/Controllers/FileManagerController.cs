using Core.Application.Factories;
using Core.Domain.Entities;
using Core.Domain.ValueObjects;
using Core.Domain.Services.Abstractions;
using Core.Infrastructure.DataAccess;
using FileManagerService.Requests;
using FileManagerService.Responses;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FileManagerService.Controllers
{
    [Route("/file-manager/[action]")]
    public class FileManagerController : ControllerBase
    {
        private readonly string _publicAccessServiceUrl;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly Dictionary<string, string> _mimeMap;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<FileManagerController> _logger;

        public FileManagerController(IConfiguration configuration, IPublishEndpoint publishEndpoint, ApplicationDbContext db, ILogger<FileManagerController> logger)
        {
            _publicAccessServiceUrl = configuration.GetValue<string>("PublicAccessServiceUrl");
            _publishEndpoint = publishEndpoint;
            _db = db;
            _logger = logger;

            _mimeMap = new Dictionary<string, string>() {
                { ".bmp", "image/x-ms-bmp" },
                { ".gif", "image/gif" },
                { ".ico", "image/x-icon" },
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".tif", "image/tiff" },
                { ".tiff", "image/tiff" },
                { ".webp", "image/webp" },
            };
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

        private UserDisk GetAuthenticatedUserDisk(string diskName)
        {
            return _db.GetUserDisk(GetAuthenticatedUserId(), diskName);
        }

        private IFileStorage GetFileStorage(string diskName)
        {
            IFileStorage storage = FileStorageFactory.GetFileStorage(GetAuthenticatedUserDisk(diskName));
            // используем паттерн Decorator
            IFileStorage loggingStorage = new LoggingFileStorage(storage, _logger);
            return loggingStorage;
        }

        [HttpGet]
        [ActionName("initialize")]
        public IActionResult InitializeManager()
        {
            var shareList = _db.Shares
                .Where(x => x.UserId == GetAuthenticatedUserId())
                .Select(x => new AddShareResponse() { Disk = x.Disk, Path = x.Path, PublicId = x.PublicId })
                .ToList();

            return Ok(new InitializeResponse()
            {
                Result = new(Status.Success, ""),
                Config = new()
                {
                    Acl = false,
                    HiddenFiles = true,
                    Disks = new()
                    {
                        { "public",
                            new()
                            {
                                { "driver", "local" }
                            }
                        }
                    },
                    Lang = "ru",
                    LeftDisk = "",
                    RightDisk = "",
                    LeftPath = "",
                    RightPath = "",
                    WindowsConfig = (int)WindowsConfig.OneManager,

                    ShareBaseUrl = _publicAccessServiceUrl + "/view/",
                    ShareList = shareList,
                }
            });
        }

        [HttpGet]
        [ActionName("content")]
        public async Task<IActionResult> StorageContent([FromQuery(Name = "disk")] string disk, [FromQuery(Name = "path")] string path)
        {
            var storage = GetFileStorage(disk);

            var list = await storage.GetListingAsync(path);

            var dirs = list.Where(x => x is not FileProperties);

            var files = list.Where(x => x is FileProperties);

            return Ok(new ContentResponse()
            {
                Result = new(Status.Success, ""),
                Directories = dirs,
                Files = files,
            });
        }

        [HttpPost]
        [ActionName("create-directory")]
        public async Task<IActionResult> CreateDirectory([FromBody] CreateDirectoryRequest request)
        {
            var storage = GetFileStorage(request.Disk);

            var newDirectoryProperties = await storage.CreateDirectoryAsync(request.Path, request.Name);

            var response = new CreateDirectoryResponse()
            {
                Result = new(Status.Success, "dirCreated"),
                Directory = newDirectoryProperties,
            };

            return Ok(response);
        }

        /// <summary>
        /// запрос на содание файла
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("create-file")]
        public async Task<IActionResult> CreateFile([FromBody] CreateFileRequest request)
        {
            var storage = GetFileStorage(request.Disk);

            var fileProperties = await storage.CreateFileAsync(request.Path, request.Name);

            return Ok(new CreateFileResponse()
            {
                Result = new(Status.Success, "fileCreated"),
                File = fileProperties,
            });
        }

        [HttpPost]
        [ActionName("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadRequest request)
        {
            var storage = GetFileStorage(request.Disk);

            await Parallel.ForEachAsync(request.Files, async (uploadedFile, cancellationToken) => {
                await storage.WriteToFileAsync(request.Path, uploadedFile.FileName, uploadedFile.OpenReadStream(), request.Overwrite == 1);
            });

            return Ok(new ResultResponse(Status.Success, "uploaded"));
        }

        [HttpPost]
        [ActionName("rename")]
        public async Task<IActionResult> Rename([FromBody] RenameRequest request)
        {
            var storage = GetFileStorage(request.Disk);

            var success = await storage.RenameAsync(request.OldName, request.NewName);

            return Ok(new ResultResponse(Status.Success, "renamed"));
        }

        [HttpPost]
        [ActionName("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteRequest request)
        {
            var storage = GetFileStorage(request.Disk);

            await Parallel.ForEachAsync(request.Items, async (item, cancellationToken) => {
                await storage.DeleteAsync(item.Path);
            });

            return Ok(new ResultResponse(Status.Success, "deleted"));
        }

        [HttpPost]
        [ActionName("paste")]
        public async Task<IActionResult> Paste([FromBody] PasteRequest request)
        {
            var storage = GetFileStorage(request.Disk);
            if (request.Path == null)
            {
                request.Path = "";
            }

            if (request.Clipboard.Type == PasteRequest.ClipboardObject.ClipboardType.Cut)
            {
                await Parallel.ForEachAsync(request.Clipboard.Directories, async (item, cancellationToken) => {
                    await storage.MoveToDirectoryAsync(item, request.Path);
                });
                await Parallel.ForEachAsync(request.Clipboard.Files, async (item, cancellationToken) => {
                    await storage.MoveToDirectoryAsync(item, request.Path);
                });
            }
            else
            {
                await Parallel.ForEachAsync(request.Clipboard.Directories, async (item, cancellationToken) => {
                    await storage.CopyToDirectoryAsync(item, request.Path);
                });
                await Parallel.ForEachAsync(request.Clipboard.Files, async (item, cancellationToken) => {
                    await storage.CopyToDirectoryAsync(item, request.Path);
                });
            }

            return Ok(new ResultResponse(Status.Success, "copied"));
        }

        [HttpPost]
        [ActionName("update-file")]
        public async Task<IActionResult> UpdateFile([FromForm] UpdateFileRequest request)
        {
            var storage = GetFileStorage(request.Disk);

            var fileProperties = await storage.WriteToFileAsync(request.Path, request.File.FileName, request.File.OpenReadStream(), true);

            return Ok(new UpdateFileResponse()
            {
                Result = new(Status.Success, "fileUpdated"),
                File = fileProperties,
            });
        }

        [HttpGet]
        [ActionName("tree")]
        public IActionResult Tree([FromQuery(Name = "disk")] string disk, [FromQuery(Name = "path")] string path)
        {
            // FIX
            
            var response = new TreeResponse() {
                Result = new(Status.Success, "treeReturned"),
            };

            return Ok(response);
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [ActionName("preview")]
        public async Task<IActionResult> Preview([FromQuery(Name = "disk")] string disk, [FromQuery(Name = "path")] string path)
        {
            var storage = GetFileStorage(disk);

            var (fileProperties, stream) = await storage.ReadFileAsync(path);

            string contentType;
            if (!_mimeMap.TryGetValue(fileProperties.Extension.ToLower(), out contentType))
            {
                contentType = "application/octet-stream";
            }

            return File(stream, contentType, fileProperties.Filename);
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [ActionName("download")]
        public async Task<IActionResult> Download([FromQuery(Name = "disk")] string disk, [FromQuery(Name = "path")] string path)
        {
            var storage = GetFileStorage(disk);

            var (fileProperties, stream) = await storage.ReadFileAsync(path);

            return File(stream, "application/octet-stream", fileProperties.Filename);
        }

        [HttpPost]
        [ActionName("zip")]
        public async Task<IActionResult> Zip([FromBody] ZipRequest request)
        {
            try
            {
                await _publishEndpoint.Publish<Core.Domain.Messages.ZipMessage>(new
                {
                    UserId = GetAuthenticatedUserId(),
                    Disk = request.Disk,
                    Path = request.Path,
                    Name = request.Name,
                    Directories = request.Elements.Directories,
                    Files = request.Elements.Files,
                });

                await Task.Delay(1000);

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
            try
            {
                await _publishEndpoint.Publish<Core.Domain.Messages.UnzipMessage>(new
                {
                    UserId = GetAuthenticatedUserId(),
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

            return Ok(new AddShareResponse() {
                Disk = share.Disk,
                Path = share.Path,
                PublicId = share.PublicId,
            });
        }

        [HttpPost]
        [ActionName("removeShare")]
        public async Task<IActionResult> RemoveShare([FromBody] RemoveShareRequest request)
        {
            var share = _db.GetShare(request.PublicId);
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
    }
}
