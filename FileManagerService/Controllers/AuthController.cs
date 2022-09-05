using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FileManagerService.Requests;
using FileManagerService.Responses;
using FileManagerService.Data;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileManagerService.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [ActionName("remember")]
        public async Task<IActionResult> Remember()
        {
            if (HttpContext.User != null && HttpContext.User.Identity.IsAuthenticated)
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

        [HttpPost]
        [ActionName("reg")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            User user = new User {
                Email = request.Email,
                UserName = request.UserName ?? request.Email,
            };
            // добавляем пользователя
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                // установка куки
                await _signInManager.SignInAsync(user, false);
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
