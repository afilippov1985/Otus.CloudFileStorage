using Common.Data;
using FileManagerService.Requests;
using FileManagerService.Responses;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileManagerService.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IAntiforgery _csrfService;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IAntiforgery csrfService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _csrfService = csrfService;
        }

        [HttpGet]
        [ResponseCache(NoStore = true)]
        [ActionName("remember")]
        public async Task<IActionResult> Remember()
        {
            var tokens = _csrfService.GetAndStoreTokens(HttpContext);
            HttpContext.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!, new CookieOptions() { HttpOnly = false });

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
