﻿using CloudStorage.FileManagerService.Dto;
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
    //[Route("file-manager/[action]")]
    [Route("/file-manager/[action]")]
    public class FileManagerController : ControllerBase
    {
        private readonly FileManagerServiceOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public FileManagerController(IOptions<FileManagerServiceOptions> options)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(FileManagerServiceOptions));
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
            string diskPath = _options.UserFilesPath; // FIX move to app settings
            // TODO validate path
            string contentPath = path != null ? Path.Combine(diskPath, path.Replace(DirectoryAttributes.DirectorySeparatorChar, Path.DirectorySeparatorChar)) : diskPath;

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

            return Ok(new ContentResponse() {
                Result = new(Status.Success, ""),
                Directories = dirs,
                Files = files,
            });
        }

        [HttpPost]
        [ActionName("create-directory")]
        public IActionResult CreateDirectory([FromBody]NewDirectory dir)
        {
            try
            {   
                string diskPath = _options.UserFilesPath;
                string dirPath = dir.Path != null ? Path.Combine(diskPath, dir.Path.Replace(DirectoryAttributes.DirectorySeparatorChar, Path.DirectorySeparatorChar)) : diskPath;

                var dirInfo = new DirectoryInfo(dirPath);
                dirInfo.CreateSubdirectory(dir.Name);

                var newDirectory = new DirectoryAttributes(diskPath, new DirectoryInfo(Path.Combine(dirPath, dir.Name)));

                var tree = new List<Tree>();
                foreach (var i in Directory.EnumerateDirectories(dirPath))
                {
                    tree.Add(new Tree(diskPath, new DirectoryInfo(i)));
                }
                
                return Ok(new TreeResponse() {
                    Result = new(Status.Success, $"Directory \"{dir.Name}\" created"),
                    Directory = newDirectory,
                    Tree = tree
                });
            }
            catch(Exception ex)
            {
                return BadRequest(new TreeResponse() {
                    Result = new(Status.Warning, ex.Message)
                });
            }
            
        }
    }
}
