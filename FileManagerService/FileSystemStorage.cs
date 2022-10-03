using FileManagerService.Data;
using FileManagerService.Interfaces;
using FileManagerService.Models;
using FileManagerService.Requests;
using FileManagerService.Responses;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagerService
{
    public class FileSystemStorage : IFileStorage
    {
        private readonly string _storagePath;
        private readonly string _publicAccessServiceUrl;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly Dictionary<string, string> _mimeMap;
        private readonly ApplicationDbContext _db;

        public FileSystemStorage(IConfiguration configuration, IPublishEndpoint publishEndpoint, ApplicationDbContext db)
        {
            _db = db;

            _storagePath = configuration.GetValue<string>("StoragePath");
            _publicAccessServiceUrl = configuration.GetValue<string>("PublicAccessServiceUrl");

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

        public InitializeResponse InitializeManager(string AuthenticatedUserId)
        {
            var shareList = _db.Shares
                .Where(x => x.UserId == AuthenticatedUserId)
                .Select(x => new AddShareResponse() { Disk = x.Disk, Path = x.Path, PublicId = x.PublicId })
                .ToList();

            return new InitializeResponse()
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
            };
        }

        public ContentResponse StorageContent(string path, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
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

            return new ContentResponse()
            {
                Result = new(Status.Success, ""),
                Directories = dirs,
                Files = files,
            };
        }

        public CreateDirectoryResponse CreateDirectory(CreateDirectoryRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
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

            return response;
        }

        public CreateFileResponse CreateFile(CreateFileRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, request.Path);

            if (string.IsNullOrWhiteSpace(request?.Name))
            {
                return null;
            }

            FileInfo fileInfo = new FileInfo(Path.Combine(contentPath, request.Name));
            if (fileInfo.Exists)
            {
                return new CreateFileResponse()
                {
                    Result = new(Status.Warning, "fileExist"),
                };
            }

            FileStream fs = fileInfo.Create();
            fs.Close();
            return new CreateFileResponse()
            {
                Result = new(Status.Success, "fileCreated"),
                File = new(diskPath, fileInfo),
            };
        }

        public ResultResponse Upload(UploadRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
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

            return new ResultResponse(Status.Success, "uploaded");
        }

        public ResultResponse Rename(RenameRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);

            if (request == null || string.IsNullOrWhiteSpace(request.OldName) || string.IsNullOrWhiteSpace(request.NewName))
            {
                return null;
            }

            if (request.Type == EntityType.File)
            {
                if (!System.IO.File.Exists(Path.Combine(diskPath, request.OldName)))
                {
                    return new ResultResponse(Status.Warning, "fileNotExist");
                }

                System.IO.File.Move(Path.Combine(diskPath, request.OldName), Path.Combine(diskPath, request.NewName));
            }
            else
            {
                if (!Directory.Exists(Path.Combine(diskPath, request.OldName)))
                {
                    return new ResultResponse(Status.Warning, "dirNotExist");
                }

                Directory.Move(Path.Combine(diskPath, request.OldName), Path.Combine(diskPath, request.NewName));
            }

            return new ResultResponse(Status.Success, "renamed");
        }

        public ResultResponse Delete(DeleteRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            //string contentPath = GetContentPath(diskPath, fileRequest.Path);

            if (request == null || request.Items == null)
            {
                return null;
            }

            foreach (var Item in request.Items)
            {
                if (Item.Type == EntityType.File)
                {
                    if (!File.Exists(Path.Combine(diskPath, Item.Path)))
                        continue;

                    File.Delete(Path.Combine(diskPath, Item.Path));
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(diskPath, Item.Path)))
                        continue;

                    Directory.Delete(Path.Combine(diskPath, Item.Path), true);
                }
            }

            return new ResultResponse(Status.Success, "deleted");
        }

        public ResultResponse Paste(PasteRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
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

                return new ResultResponse(Status.Success, "copied");
            }
            catch (Exception ex)
            {
                return new ResultResponse(Status.Danger, ex.Message);
            }
        }

        public UpdateFileResponse UpdateFile(UpdateFileRequest fileRequest, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, fileRequest.Path);

            if (fileRequest == null || fileRequest.File == null)
            {
                return null;
            }

            FileInfo fileInfo = new FileInfo(Path.Combine(contentPath, fileRequest.File.FileName));
            using (FileStream stream = fileInfo.Create())
            {
                fileRequest.File.CopyTo(stream);
            }

            return new UpdateFileResponse()
            {
                Result = new(Status.Success, "fileUpdated"),
                File = new(diskPath, fileInfo),
            };
        }

        public TreeResponse Tree(string path, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, path);

            var response = new TreeResponse()
            {
                Result = new(Status.Success, "treeReturned"),
            };

            foreach (var i in Directory.EnumerateDirectories(contentPath))
            {
                response.Directories.Add(new(diskPath, new DirectoryInfo(i)));
            }

            return response;
        }

        public DownloadResponse Preview(string path, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, path);
            var fileInfo = new FileInfo(contentPath);

            //string contentType;
            if (!_mimeMap.TryGetValue(fileInfo.Extension.ToLower(), out string contentType))
            {
                contentType = "application/octet-stream";
            }

            return new DownloadResponse()
            {
                ContentPath = contentPath,
                ContentType = contentType
            };
        }

        public DownloadResponse Download(string path, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, path);
            var fileInfo = new FileInfo(contentPath);

            return new DownloadResponse()
            {
                ContentPath = contentPath,
                ContentType = "application/octet-stream",
                NameFile = fileInfo.Name
            };
        }

        public async Task<ResultResponse> Zip(ZipRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);

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

                System.Threading.Thread.Sleep(1000);

                return new ResultResponse(Status.Success, "");
            }
            catch
            {
                return new ResultResponse(Status.Warning, "zipError");
            }
        }

        public async Task<ResultResponse> Unzip(UnzipRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);

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

                return new ResultResponse(Status.Success, "");
            }
            catch
            {
                return new ResultResponse(Status.Warning, "zipError");
            }
        }

        public async Task<AddShareResponse> AddShare(AddShareRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);

            var share = new Share()
            {
                UserId = AuthenticatedUserId,
                Disk = request.Disk,
                Path = request.Path,
                CreatedAt = DateTime.UtcNow,
            };

            _db.Shares.Add(share);
            await _db.SaveChangesAsync();

            return new AddShareResponse()
            {
                Disk = share.Disk,
                Path = share.Path,
                PublicId = share.PublicId,
            };
        }

        public async Task<RemoveShareResponse> RemoveShare(RemoveShareRequest request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);

            var share = _db.Shares.Where(x => x.PublicId == request.PublicId).FirstOrDefault();
            if (share != null)
            {
                _db.Shares.Remove(share);
                await _db.SaveChangesAsync();
            }

            return new RemoveShareResponse()
            {
                Disk = request.Disk,
                Path = request.Path,
            };
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

        private string GetDiskPath(string AuthenticatedUserId)
        {
            var dir = Path.Combine(_storagePath, AuthenticatedUserId);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        private static string GetContentPath(string diskPath, string? path)
        {
            return path != null ? Path.Combine(diskPath, path.Replace(DirectoryAttributes.DirectorySeparatorChar, Path.DirectorySeparatorChar)) : diskPath;
        }
    }
}
