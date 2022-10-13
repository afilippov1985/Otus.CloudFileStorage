using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Common.Interfaces;
using Common.Results;
using Common.Models;
using Common.Queries;
using Common.Data;
using Common.Messages;

namespace Common
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

            //todo откуда считывать настройки???
            //_storagePath = configuration.GetValue<string>("StoragePath");
            //_publicAccessServiceUrl = configuration.GetValue<string>("PublicAccessServiceUrl");

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

        public InitializeResult InitializeManager(string AuthenticatedUserId)
        {
            var shareList = _db.Shares
                .Where(x => x.UserId == AuthenticatedUserId)
                .Select(x => new AddShareResult() { Disk = x.Disk, Path = x.Path, PublicId = x.PublicId })
                .ToList();

            return new InitializeResult()
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

        public ContentResult StorageContent(string path, string AuthenticatedUserId)
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

            return new ContentResult()
            {
                Result = new(Status.Success, ""),
                Directories = dirs,
                Files = files,
            };
        }

        public CreateDirectoryResult CreateDirectory(CreateDirectoryQuery request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, request.Path);

            var dirInfo = new DirectoryInfo(contentPath);
            dirInfo.CreateSubdirectory(request.Name);

            var newDirectory = new DirectoryAttributes(diskPath, new DirectoryInfo(Path.Combine(contentPath, request.Name)));

            var response = new CreateDirectoryResult()
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

        public CreateFileResult CreateFile(CreateFileQuery request, string AuthenticatedUserId)
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
                return new CreateFileResult()
                {
                    Result = new(Status.Warning, "fileExist"),
                };
            }

            FileStream fs = fileInfo.Create();
            fs.Close();
            return new CreateFileResult()
            {
                Result = new(Status.Success, "fileCreated"),
                File = new (diskPath, fileInfo),
            };
        }

        public ResultResult Upload(UploadQuery request, string AuthenticatedUserId)
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

            return new ResultResult(Status.Success, "uploaded");
        }

        public ResultResult Rename(RenameQuery request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);

            if (request == null || string.IsNullOrWhiteSpace(request.OldName) || string.IsNullOrWhiteSpace(request.NewName))
            {
                return null;
            }

            if (request.Type == DirectoryAttributes.EntityType.File)
            {
                if (!File.Exists(Path.Combine(diskPath, request.OldName)))
                {
                    return new ResultResult(Status.Warning, "fileNotExist");
                }

                File.Move(Path.Combine(diskPath, request.OldName), Path.Combine(diskPath, request.NewName));
            }
            else
            {
                if (!Directory.Exists(Path.Combine(diskPath, request.OldName)))
                {
                    return new ResultResult(Status.Warning, "dirNotExist");
                }

                Directory.Move(Path.Combine(diskPath, request.OldName), Path.Combine(diskPath, request.NewName));
            }

            return new ResultResult(Status.Success, "renamed");
        }

        public ResultResult Delete(DeleteQuery request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            //string contentPath = GetContentPath(diskPath, fileRequest.Path);

            if (request == null || request.Items == null)
            {
                return null;
            }

            foreach (var Item in request.Items)
            {
                if (Item.Type == DirectoryAttributes.EntityType.File)
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

            return new ResultResult(Status.Success, "deleted");
        }

        public ResultResult Paste(PasteQuery request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, request.Path);
            bool overwrite = false;

            try
            {
                if (request.Clipboard.Type == PasteQuery.ClipboardObject.ClipboardType.Cut)
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

                return new ResultResult(Status.Success, "copied");
            }
            catch (Exception ex)
            {
                return new ResultResult(Status.Danger, ex.Message);
            }
        }

        public UpdateFileResult UpdateFile(UpdateFileQuery fileRequest, string AuthenticatedUserId)
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

            return new UpdateFileResult()
            {
                Result = new(Status.Success, "fileUpdated"),
                File = new(diskPath, fileInfo),
            };
        }

        public TreeResult Tree(string path, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, path);

            var response = new TreeResult()
            {
                Result = new(Status.Success, "treeReturned"),
            };

            foreach (var i in Directory.EnumerateDirectories(contentPath))
            {
                response.Directories.Add(new(diskPath, new DirectoryInfo(i)));
            }

            return response;
        }

        public DownloadResult Preview(string path, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, path);
            var fileInfo = new FileInfo(contentPath);

            //string contentType;
            if (!_mimeMap.TryGetValue(fileInfo.Extension.ToLower(), out string contentType))
            {
                contentType = "application/octet-stream";
            }

            return new DownloadResult()
            {
                ContentPath = contentPath,
                ContentType = contentType
            };
        }

        public DownloadResult Download(string path, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);
            string contentPath = GetContentPath(diskPath, path);
            var fileInfo = new FileInfo(contentPath);

            return new DownloadResult()
            {
                ContentPath = contentPath,
                ContentType = "application/octet-stream",
                NameFile = fileInfo.Name
            };
        }

        public async Task<ResultResult> Zip(ZipQuery request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);

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

                return new ResultResult(Status.Success, "");
            }
            catch
            {
                return new ResultResult(Status.Warning, "zipError");
            }
        }

        public async Task<ResultResult> Unzip(UnzipQuery request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);

            try
            {
                await _publishEndpoint.Publish<UnzipMessage>(new
                {
                    DiskPath = diskPath,
                    Disk = request.Disk,
                    Path = request.Path,
                    Folder = request.Folder,
                });

                System.Threading.Thread.Sleep(1000);

                return new ResultResult(Status.Success, "");
            }
            catch
            {
                return new ResultResult(Status.Warning, "zipError");
            }
        }

        public async Task<AddShareResult> AddShare(AddShareQuery request, string AuthenticatedUserId)
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

            return new AddShareResult()
            {
                Disk = share.Disk,
                Path = share.Path,
                PublicId = share.PublicId,
            };
        }

        public async Task<RemoveShareResult> RemoveShare(RemoveShareQuery request, string AuthenticatedUserId)
        {
            string diskPath = GetDiskPath(AuthenticatedUserId);

            var share = _db.Shares.Where(x => x.PublicId == request.PublicId).FirstOrDefault();
            if (share != null)
            {
                _db.Shares.Remove(share);
                await _db.SaveChangesAsync();
            }

            return new RemoveShareResult()
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
