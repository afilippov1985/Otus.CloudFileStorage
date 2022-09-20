using Microsoft.AspNetCore.Mvc;
using PublicAccessService.Models;
using PublicAccessService.Data;
using System.Diagnostics;
// using FileManagerService.Models;
using System.IO;

namespace PublicAccessService.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _storagePath;
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _storagePath = configuration.GetValue<string>("StoragePath");
            _logger = logger;
            _db = db;
        }

        private string GetDiskPath(string disk, string userId)
        {
            var dir = Path.Combine(_storagePath, userId);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            return dir;
        }

        private string GetContentPath(string diskPath, string? path)
        {
            return path != null ? Path.Combine(diskPath, path.Replace('/', Path.DirectorySeparatorChar)) : diskPath;
        }

        [HttpGet]
        [Route("/view/{publicId}")]
        public IActionResult ShareView(string publicId)
        {
            // http://localhost:7001/view/0ffd9b41412b7cb366147de250906f8f
            var share = _db.Shares.Where(x => x.PublicId == publicId).FirstOrDefault();
            if (share == null)
            {
                return NotFound();
            }

            string diskPath = GetDiskPath(share.Disk, share.UserId);
            string contentPath = GetContentPath(diskPath, share.Path);

            var info = new System.IO.FileInfo(contentPath);
            if (info.Attributes != FileAttributes.Normal)
            {
                return Forbid();
            }

            ViewData["Url"] = $"/download/{publicId}";
            ViewData["Length"] = info.Length;
            ViewData["Name"] = info.Name;

            return View(info);
        }

        [HttpGet]
        [Route("/download/{publicId}")]
        public IActionResult Download(string publicId)
        {
            var share = _db.Shares.Where(x => x.PublicId == publicId).FirstOrDefault();
            if (share == null)
            {
                return NotFound();
            }

            string diskPath = GetDiskPath(share.Disk, share.UserId);
            string contentPath = GetContentPath(diskPath, share.Path);

            var info = new System.IO.FileInfo(contentPath);
            if (info.Attributes != FileAttributes.Normal)
            {
                return Forbid();
            }

            return PhysicalFile(contentPath, "application/octet-stream", info.Name);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}