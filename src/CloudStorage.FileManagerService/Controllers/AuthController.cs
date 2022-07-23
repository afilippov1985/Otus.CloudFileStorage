using Microsoft.AspNetCore.Mvc;
using CloudStorage.FileManagerService.Dto;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
namespace CloudStorage.FileManagerService.Controllers
{
    [Route("/auth/[action]")]
    public class AuthController : ControllerBase
    {
        [HttpGet]
        [ActionName("remember")]
        public IActionResult Remember()
        {
            // TODO
            return Ok(new RememberResponse() {
                CsrfToken = "random token stored in session",
                User = null,
            });
        }

        [HttpPost]
        [ActionName("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // var user = authenticateUser(request.Login, request.Password);
            // //HttpContext.Session.SetInt32("userId", 1); // TODO user id

            if (ModelState.IsValid)
            {
                var user = authenticateUser(request.Login, request.Password);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Unauthorized();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, "Administrator"),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                //     //AllowRefresh = <bool>,
                //     // Refreshing the authentication session should be allowed.

                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20),
                //     // The time at which the authentication ticket expires. A 
                //     // value set here overrides the ExpireTimeSpan option of 
                //     // CookieAuthenticationOptions set with AddCookie.

                //     //IsPersistent = true,
                //     // Whether the authentication session is persisted across 
                //     // multiple requests. When used with cookies, controls
                //     // whether the cookie's lifetime is absolute (matching the
                //     // lifetime of the authentication ticket) or session-based.

                //     //IssuedUtc = <DateTimeOffset>,
                //     // The time at which the authentication ticket was issued.

                //     //RedirectUri = <string>
                //     // The full path or absolute URI to be used as an http 
                //     // redirect response value.
                };

                HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                Console.WriteLine($"User {user.UserName} logged in at {user.RegDate}.");

                return Ok(new LoginResponse() {
                    User = user,
                });
            }
            return BadRequest();         
        }

        private UserAuth? authenticateUser(string login, string password)
        {
            // TODO user model
            if (login == "admin" && password == "admin") // TODO проверять по базе
            {
                return new UserAuth()
                {
                    Id = 1,
                    UserName = login,
                    UserPassword = password,
                    RegDate = DateTime.Now
                };
            }
            else
            {
                return null;
            }
        }

        [HttpPost]
        [ActionName("logout")]
        public IActionResult Logout()
        {
            // Clear the existing external cookie
            HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);
            Console.WriteLine("Logout");//todo
            return Ok();
        }
    }

    public class UserAuth //todo из ветки Феди
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Пароль
        /// </summary>
        public string UserPassword { get; set; }

        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime RegDate { get; set; }
    }
}