using Core.Application.Factories;
using Core.Domain.Entities;
using Core.Domain.Services.Abstractions;
using Core.Infrastructure.DataAccess;
using Microsoft.AspNetCore.Mvc;
using PublicAccessService.Models;
using System.Diagnostics;

namespace PublicAccessService.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        private IFileStorage GetFileStorage(string userId, string diskName)
        {
            var userDisk = _db.GetUserDisk(userId, diskName);
            return FileStorageFactory.GetFileStorage(userDisk);
        }

        [HttpGet]
        [Route("/view/{publicId}")]
        public async Task<IActionResult> ShareView(string publicId)
        {
            // http://localhost:7001/view/0ffd9b41412b7cb366147de250906f8f
            var share = _db.GetShare(publicId);
            if (share == null)
            {
                return NotFound();
            }

            var storage = GetFileStorage(share.UserId, share.Disk);

            try
            {
                var (fileProperties, stream) = await storage.ReadFileAsync(share.Path);

                ViewData["Url"] = $"/download/{publicId}";
                ViewData["Length"] = fileProperties.Size;
                ViewData["Name"] = fileProperties.Filename;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when read file {0}", share.Path);
                return Forbid();
            }
        }

        [HttpGet]
        [Route("/download/{publicId}")]
        public async Task<IActionResult> Download(string publicId)
        {
            var share = _db.GetShare(publicId);
            if (share == null)
            {
                return NotFound();
            }

            var storage = GetFileStorage(share.UserId, share.Disk);

            try
            {
                var (fileProperties, stream) = await storage.ReadFileAsync(share.Path);

                ViewData["Url"] = $"/download/{publicId}";
                ViewData["Length"] = fileProperties.Size;
                ViewData["Name"] = fileProperties.Filename;

                return File(stream, "application/octet-stream", fileProperties.Filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when read file {0}", share.Path);
                return Forbid();
            }
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
