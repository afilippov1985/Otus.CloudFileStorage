namespace CloudStorage.FileManagerService.Dto
{
    public class LoginRequest
    {
        public string Login { get; set; }
        /// <summary>
        /// наименование с расширением
        /// </summary>
        public string Password { get; set; }
    }
}
