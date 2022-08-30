using CloudStorage.FileManagerService.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.IO;
using Microsoft.Extensions.Options;

namespace CloudStorage.FileManagerService.Controllers
{
    /// <summary>
    /// Набор запросов для управления файлами хранилища
    /// </summary>

    //[ApiController]
    [ApiVersion("1.0")]
    [Route("/file-manager/[action]")]
    public class FileManagerController : ControllerBase
    {
        private readonly FileManagerServiceOptions _options;
        private Dictionary<string, string> _mimeMap;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public FileManagerController(IOptions<FileManagerServiceOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(FileManagerServiceOptions));
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

        private string GetDiskPath(string disk)
        {
            var claim = HttpContext.User.FindFirst("Id");
            if (claim == null)
            {
                throw new UnauthorizedAccessException();
            }
            var dir = Path.Combine(_options.UserFilesPath, claim.Value);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        private string GetContentPath(string diskPath, string path)
        {
            return path != null ? Path.Combine(diskPath, path.Replace(DirectoryAttributes.DirectorySeparatorChar, Path.DirectorySeparatorChar)) : diskPath;
        }

        /// <summary>
        /// Возвращает информацию об инициализации сервиса FileManagerService
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("initialize")]
        //[ProducesResponseType(typeof(InitializeResponse), (int)HttpStatusCode.OK)]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public IActionResult InitializeManager()
        {
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
                    WindowsConfig = (int)WindowsConfig.OneManager
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

            var files = new List<Dto.FileAttributes>();
            foreach (var i in Directory.EnumerateFiles(contentPath))
            {
                files.Add(new Dto.FileAttributes(diskPath, new FileInfo(i)));
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
        public IActionResult CreateDirectory([FromBody] NewDirectory dir)
        {
            string diskPath = GetDiskPath(dir.Disk);
            string contentPath = GetContentPath(diskPath, dir.Path);

            var dirInfo = new DirectoryInfo(contentPath);
            dirInfo.CreateSubdirectory(dir.Name);

            var newDirectory = new DirectoryAttributes(diskPath, new DirectoryInfo(Path.Combine(contentPath, dir.Name)));

            var tree = new List<Tree>();
            foreach (var i in Directory.EnumerateDirectories(contentPath))
            {
                tree.Add(new Tree(diskPath, new DirectoryInfo(i)));
            }

            return Ok(new TreeResponse()
            {
                Result = new(Status.Success, "dirCreated"),
                Directory = newDirectory,
                Tree = tree
            });
        }

        /// <summary>
        /// запрос на содание файла
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("create-file")]
        public IActionResult CreateFile([FromBody] CreateFileRequest fileRequest)
        {
            string diskPath = GetDiskPath(fileRequest.Disk);
            string contentPath = GetContentPath(diskPath, fileRequest.Path);

            if (string.IsNullOrWhiteSpace(fileRequest?.Name))
            {
                return UnprocessableEntity();
            }

            FileInfo fileInfo = new FileInfo(Path.Combine(contentPath, fileRequest.Name));
            if (fileInfo.Exists)
            {
                return Ok(new CreateFileResponse()
                {
                    Result = new Result(Status.Warning, "fileExist"),
                });
            }

            FileStream fs = fileInfo.Create();
            fs.Close();
            return Ok(new CreateFileResponse()
            {
                Result = new Result(Status.Success, "fileCreated"),
                File = new Dto.FileAttributes(diskPath, fileInfo)
            });
        }

        [HttpPost]
        [ActionName("upload")]
        public IActionResult StorageUpload([FromQuery(Name = "disk")] string disk,
                                           [FromQuery(Name = "path")] string path,
                                           [FromQuery(Name = "overwrite")] int overwrite,
                                           [FromQuery(Name = "files[]")] List<IFormFile> files)
        {
            string diskPath = GetDiskPath(disk);
            string contentPath = GetContentPath(diskPath, path);

            foreach (IFormFile uploadedFile in files)
            {
                string filePath = Path.Combine(contentPath, Path.GetFileName(uploadedFile.FileName));
                if (overwrite == 1 || !System.IO.File.Exists(filePath))
                {
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadedFile.CopyTo(stream);
                    }
                }
            }

            return Ok(new UploadResponse()
            {
                Result = new(Status.Success, "uploaded"),
            });
        }

        /// <summary>
        /// переименовть объект
        /// </summary>
        /// <param name="request">тело запроса</param>
        /// <returns>результат запроса</returns>
        [HttpPost]
        [ActionName("rename")]
        public IActionResult Rename([FromBody] RenameRequest request)
        {
            string diskPath = GetDiskPath(request.Disk);
            //string contentPath = GetContentPath(diskPath, fileRequest.Path);

            if (request == null || string.IsNullOrWhiteSpace(request.OldName) || string.IsNullOrWhiteSpace(request.NewName))
            {
                return UnprocessableEntity();
            }

            if (request.Type == EntityType.File)
            {
                if (!System.IO.File.Exists(Path.Combine(diskPath, request.OldName)))
                    return Ok(new Result(Status.Warning, "fileNotExist"));

                System.IO.File.Move(Path.Combine(diskPath, request.OldName), Path.Combine(diskPath, request.NewName));
            }
            else
            {
                if (!Directory.Exists(Path.Combine(diskPath, request.OldName)))
                    return Ok(new Result(Status.Warning, "dirNotExist"));

                Directory.Move(Path.Combine(diskPath, request.OldName), Path.Combine(diskPath, request.NewName));
            }

            return Ok(new RenameResponse()
            {
                Result = new Result(Status.Success, "renamed")
            });
        }

        /// <summary>
        /// переименовть объект
        /// </summary>
        /// <param name="request">тело запроса</param>
        /// <returns>результат запроса</returns>
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

            foreach (Item Item in request.Items)
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

                    Directory.Delete(Path.Combine(diskPath, Item.Path));
                }
            }

            return Ok(new DeleteResponse()
            {
                Result = new Result(Status.Success, "deleted")
            });
        }

        /// <summary>
        /// запрос на Изменение файла
        /// </summary>
        /// <param name="fileRequest"></param>
        /// <returns></returns>
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
                Result = new Result(Status.Success, "fileUpdated"),
                File = new Dto.FileAttributes(diskPath, fileInfo)
            });
        }

        [HttpGet]
        [ActionName("tree")]
        public IActionResult ReturnTree([FromQuery(Name = "disk")] string disk, [FromQuery(Name = "path")] string path)
        {
            string diskPath = GetDiskPath(disk);
            string contentPath = GetContentPath(diskPath, path);

            var tree = new List<Tree>();
            foreach (var i in Directory.EnumerateDirectories(contentPath))
            {
                tree.Add(new Tree(diskPath, new DirectoryInfo(i)));
            }

            return Ok(new ReturnTreeResponse()
            {
                Result = new(Status.Success, "treeReturned"),
                Directories = tree
            });
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
        public IActionResult Zip([FromBody] ZipRequest zipRequest)
        {
            if (true) // задание отправлено в очередь
            {
                // TODO отправить задание в очередь
                return Ok(new ZipResponse() {
                    Result = new Result(Status.Success, ""),
                });
            }

            return Ok(new ZipResponse() {
                Result = new Result(Status.Warning, "zipError"),
            });
        }

        [HttpPost]
        [ActionName("unzip")]
        public IActionResult Unzip([FromBody] UnzipRequest unzipRequest)
        {
            if (true) // задание отправлено в очередь
            {
                // TODO отправить задание в очередь
                return Ok(new UnzipResponse() {
                    Result = new Result(Status.Success, ""),
                });
            }

            return Ok(new UnzipResponse() {
                Result = new Result(Status.Warning, "zipError"),
            });
        }

    }
}
