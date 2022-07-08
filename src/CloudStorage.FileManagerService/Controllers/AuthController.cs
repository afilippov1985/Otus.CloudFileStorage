using Microsoft.AspNetCore.Mvc;
using CloudStorage.FileManagerService.Dto;

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
            var user = authenticateUser(request.Login, request.Password);

            HttpContext.Session.SetInt32("userId", 1); // TODO user id

            return Ok(new LoginResponse() {
                User = user,
            });
        }

        private object authenticateUser(string login, string password)
        {
            // TODO user model
            return "user model";
        }

        [HttpPost]
        [ActionName("logout")]
        public IActionResult Logout()
        {
            // TODO
            return Ok();
        }
    }
}
