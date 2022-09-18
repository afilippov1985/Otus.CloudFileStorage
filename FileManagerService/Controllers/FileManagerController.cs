using FileManagerService.Data;
using FileManagerService.Models;
using FileManagerService.Requests;
using FileManagerService.Responses;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagerService.Controllers
{
    [Route("/file-manager/[action]")]
    public class FileManagerController : ControllerBase
    {
        private readonly string _storagePath;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly Dictionary<string, string> _mimeMap;
        private readonly ApplicationDbContext _db;

        public FileManagerController(IConfiguration configuration, IPublishEndpoint publishEndpoint, ApplicationDbContext db)
        {
            _db = db;

            _storagePath = configuration.GetValue<string>("StoragePath");

            _publishEndpoint = publishEndpoint;

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

        private string GetDiskPath(string disk)
        {
            var dir = Path.Combine(_storagePath, GetAuthenticatedUserId());
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        private string GetContentPath(string diskPath, string? path)
        {
            return path != null ? Path.Combine(diskPath, path.Replace(DirectoryAttributes.DirectorySeparatorChar, Path.DirectorySeparatorChar)) : diskPath;
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

                    ShareBaseUrl = "http://localhost:7001/view/", // TODO URL доступа к опубликованному файлу
                    ShareList = shareList,
                }
            });
        }

        [HttpGet]
        [ActionName("content")]
        public IActionResult StorageContent([FromQuery(Name = "disk")] string disk, [FromQuery(Name = "path")] string path)
        {
            string diskPath = GetDiskPath(disk);
            string contentPath = GetContentPath(diskPath, path);

            var dirs = new List<DirectoryAttributes>();
            foreach (var i in Directory.EnumerateDirectories(contentPath))
            {
                dirs.Add(new DirectoryAttributes(diskPath, new DirectoryInfo(i)));
            }

            var files = new List<Models.FileAttributes>();
            foreach (var i in Directory.EnumerateFiles(contentPath))
            {
                files.Add(new Models.FileAttributes(diskPath, new FileInfo(i)));
            }

            return Ok(new ContentResponse()
            {
                Result = new(Status.Success, ""),
                Directories = dirs,
                Files = files,
            });
        }

        [HttpPost]
        [ActionName("create-directory")]
        public IActionResult CreateDirectory([FromBody] CreateDirectoryRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);
            string contentPath = GetContentPath(diskPath, request.Path);

            var dirInfo = new DirectoryInfo(contentPath);
            dirInfo.CreateSubdirectory(request.Name);

            var newDirectory = new DirectoryAttributes(diskPath, new DirectoryInfo(Path.Combine(contentPath, request.Name)));

            var response = new CreateDirectoryResponse()
            {
                Result = new(Status.Success, "dirCreated"),
                Directory = newDirectory,
            };

            foreach (string dir in Directory.EnumerateDirectories(contentPath))
            {
                response.Tree.Add(new(diskPath, new DirectoryInfo(dir)));
            }

            return Ok(response);
        }

        /// <summary>
        /// запрос на содание файла
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("create-file")]
        public IActionResult CreateFile([FromBody] CreateFileRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);
            string contentPath = GetContentPath(diskPath, request.Path);

            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return UnprocessableEntity();
            }

            FileInfo fileInfo = new FileInfo(Path.Combine(contentPath, request.Name));
            if (fileInfo.Exists)
            {
                return Ok(new CreateFileResponse()
                {
                    Result = new(Status.Warning, "fileExist"),
                });
            }

            FileStream fs = fileInfo.Create();
            fs.Close();
            return Ok(new CreateFileResponse()
            {
                Result = new(Status.Success, "fileCreated"),
                File = new(diskPath, fileInfo),
            });
        }

        [HttpPost]
        [ActionName("upload")]
        public IActionResult Upload([FromForm] UploadRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);
            string contentPath = GetContentPath(diskPath, request.Path);

            foreach (IFormFile uploadedFile in request.Files)
            {
                string filePath = Path.Combine(contentPath, Path.GetFileName(uploadedFile.FileName));
                if (request.Overwrite == 1 || !System.IO.File.Exists(filePath))
                {
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadedFile.CopyTo(stream);
                    }
                }
            }

            return Ok(new ResultResponse(Status.Success, "uploaded"));
        }

        [HttpPost]
        [ActionName("rename")]
        public IActionResult Rename([FromBody] RenameRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);

            if (request == null || string.IsNullOrWhiteSpace(request.OldName) || string.IsNullOrWhiteSpace(request.NewName))
            {
                return UnprocessableEntity();
            }

            if (request.Type == EntityType.File)
            {
                if (!System.IO.File.Exists(Path.Combine(diskPath, request.OldName)))
                {
                    return Ok(new ResultResponse(Status.Warning, "fileNotExist"));
                }

                System.IO.File.Move(Path.Combine(diskPath, request.OldName), Path.Combine(diskPath, request.NewName));
            }
            else
            {
                if (!Directory.Exists(Path.Combine(diskPath, request.OldName)))
                {
                    return Ok(new ResultResponse(Status.Warning, "dirNotExist"));
                }

                Directory.Move(Path.Combine(diskPath, request.OldName), Path.Combine(diskPath, request.NewName));
            }

            return Ok(new ResultResponse(Status.Success, "renamed"));
        }

        [HttpPost]
        [ActionName("delete")]
        public IActionResult Delete([FromBody] DeleteRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);
            //string contentPath = GetContentPath(diskPath, fileRequest.Path);

            if (request == null || request.Items == null)
            {
                return UnprocessableEntity();
            }

            foreach (var Item in request.Items)
            {
                if (Item.Type == EntityType.File)
                {
                    if (!System.IO.File.Exists(Path.Combine(diskPath, Item.Path)))
                        continue;

                    System.IO.File.Delete(Path.Combine(diskPath, Item.Path));
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(diskPath, Item.Path)))
                        continue;

                    Directory.Delete(Path.Combine(diskPath, Item.Path), true);
                }
            }

            return Ok(new ResultResponse(Status.Success, "deleted"));
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite);
            }

            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir, overwrite);
            }
        }

        [HttpPost]
        [ActionName("paste")]
        public IActionResult Paste([FromBody] PasteRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);
            string contentPath = GetContentPath(diskPath, request.Path);
            bool overwrite = false;

            try
            {
                if (request.Clipboard.Type == PasteRequest.ClipboardObject.ClipboardType.Cut)
                {
                    foreach (string dir in request.Clipboard.Directories)
                    {
                        var dirInfo = new DirectoryInfo(Path.Combine(diskPath, dir));
                        dirInfo.MoveTo(Path.Combine(contentPath, dirInfo.Name));
                    }

                    foreach (string file in request.Clipboard.Files)
                    {
                        var fileInfo = new FileInfo(Path.Combine(diskPath, file));
                        fileInfo.MoveTo(Path.Combine(contentPath, fileInfo.Name), overwrite);
                    }
                }
                else
                {
                    foreach (string dir in request.Clipboard.Directories)
                    {
                        var dirInfo = new DirectoryInfo(Path.Combine(diskPath, dir));
                        CopyDirectory(Path.Combine(diskPath, dir), Path.Combine(contentPath, dirInfo.Name), overwrite);
                    }

                    foreach (string file in request.Clipboard.Files)
                    {
                        var fileInfo = new FileInfo(Path.Combine(diskPath, file));
                        fileInfo.CopyTo(Path.Combine(contentPath, fileInfo.Name), overwrite);
                    }
                }

                return Ok(new ResultResponse(Status.Success, "copied"));
            }
            catch (Exception ex)
            {
                return Ok(new ResultResponse(Status.Danger, ex.Message));
            }
        }

        [HttpPost]
        [ActionName("update-file")]
        public IActionResult UpdateFile([FromForm] UpdateFileRequest fileRequest)
        {
            string diskPath = GetDiskPath(fileRequest.Disk);
            string contentPath = GetContentPath(diskPath, fileRequest.Path);

            if (fileRequest == null || fileRequest.File == null)
            {
                return UnprocessableEntity();
            }

            FileInfo fileInfo = new FileInfo(Path.Combine(contentPath, fileRequest.File.FileName));
            using (FileStream stream = fileInfo.Create())
            {
                fileRequest.File.CopyTo(stream);
            }

            return Ok(new UpdateFileResponse()
            {
                Result = new(Status.Success, "fileUpdated"),
                File = new(diskPath, fileInfo),
            });
        }

        [HttpGet]
        [ActionName("tree")]
        public IActionResult Tree([FromQuery(Name = "disk")] string disk, [FromQuery(Name = "path")] string path)
        {
            string diskPath = GetDiskPath(disk);
            string contentPath = GetContentPath(diskPath, path);

            var response = new TreeResponse() {
                Result = new(Status.Success, "treeReturned"),
            };

            foreach (var i in Directory.EnumerateDirectories(contentPath))
            {
                response.Directories.Add(new(diskPath, new DirectoryInfo(i)));
            }

            return Ok(response);
        }

        [HttpGet]
        [ActionName("preview")]
        public IActionResult Preview([FromQuery(Name = "disk")] string disk, [FromQuery(Name = "path")] string path)
        {
            string diskPath = GetDiskPath(disk);
            string contentPath = GetContentPath(diskPath, path);
            var fileInfo = new FileInfo(contentPath);

            string contentType;
            if (!_mimeMap.TryGetValue(fileInfo.Extension.ToLower(), out contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(contentPath, contentType);
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [ActionName("download")]
        public IActionResult Download([FromQuery(Name = "disk")] string disk, [FromQuery(Name = "path")] string path)
        {
            string diskPath = GetDiskPath(disk);
            string contentPath = GetContentPath(diskPath, path);
            var fileInfo = new FileInfo(contentPath);

            return PhysicalFile(contentPath, "application/octet-stream", fileInfo.Name);
        }

        [HttpPost]
        [ActionName("zip")]
        public async Task<IActionResult> Zip([FromBody] ZipRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);

            try
            {
                await _publishEndpoint.Publish<ArchiveService.Messages.ZipMessage>(new
                {
                    DiskPath = diskPath,
                    Disk = request.Disk,
                    Path = request.Path,
                    Name = request.Name,
                    Directories = request.Elements.Directories,
                    Files = request.Elements.Files,
                });

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
            string diskPath = GetDiskPath(request.Disk);

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
    }
}
