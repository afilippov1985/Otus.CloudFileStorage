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
            DAL.Models.UserAuth? user = null;
            using (var db = new ApplicationDbContext())
            {
                user = db.Users.Where(u => u.UserName == request.Login).FirstOrDefault();
            }

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Unauthorized();
            }

            if (new DAL.Models.PasswordHasher().VerifyHashedPassword(user, user.UserPassword, request.Password) == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Unauthorized();
            }

            var claims = new List<Claim> {
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties() {
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

            return Ok(new LoginResponse()
            {
                User = user,
            });
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

}