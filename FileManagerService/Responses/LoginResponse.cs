namespace FileManagerService.Responses
{
    public class LoginResponse
    {
        public UserObject User { get; set; }

        public class UserObject
        {
            public string Email { get; set; }
            public string UserName { get; set; }

        }
    }
}
