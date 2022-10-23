using Core.Domain.Entities;
using Core.Infrastructure.DataAccess;
using FileManagerService.Requests;
using FileManagerService.Responses;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileManagerService.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly string _storagePath;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IAntiforgery _csrfService;

        public AuthController(ApplicationDbContext db, IConfiguration configuration, UserManager<User> userManager, SignInManager<User> signInManager, IAntiforgery csrfService)
        {
            _db = db;
            _storagePath = configuration.GetValue<string>("StoragePath");
            _userManager = userManager;
            _signInManager = signInManager;
            _csrfService = csrfService;
        }

        private void AddCsrfCookie()
        {
            var tokens = _csrfService.GetAndStoreTokens(HttpContext);
            HttpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions() { HttpOnly = false });
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [ActionName("remember")]
        public IActionResult Remember()
        {
            AddCsrfCookie();
            if (HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                return Ok(new LoginResponse() {
                    User = new() {
                        Email = HttpContext.User.FindFirst(ClaimTypes.Email).Value,
                        UserName = HttpContext.User.FindFirst(ClaimTypes.Name).Value,
                    },
                });
            }

            return Unauthorized();
        }

        private Task CreateDefaultDiskForUserAsync(User user)
        {
            _db.UserDisks.Add(new UserDisk()
            {
                UserId = user.Id,
                Disk = "public",
                Driver = Core.Domain.Services.Abstractions.FileStorageDriver.FileSystem,
                StorageOptions = $"{_storagePath}/{user.Id}",
            });
            return _db.SaveChangesAsync();
        }

        [HttpPost]
        [ActionName("reg")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            User user = new (){
                Email = request.Email,
                UserName = request.UserName ?? request.Email,
            };

            // добавляем пользователя
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                // создаем диск для пользователя
                var task1 = CreateDefaultDiskForUserAsync(user);

                // заходим под пользователем
                var task2 = _signInManager.SignInAsync(user, false);

                await task1;
                await task2;

                return Ok(new LoginResponse()
                {
                    User = new() {
                        Email = user.Email,
                        UserName = user.UserName,
                    },
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [ActionName("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
            if (result.Succeeded)
            {
                return Ok(new LoginResponse()
                {
                    User = new() {
                        Email = request.Email,
                        UserName = "", // TODO
                    },
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [ActionName("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }
    }
}
