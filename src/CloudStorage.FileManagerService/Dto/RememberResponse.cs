namespace CloudStorage.FileManagerService.Dto
{
    public class RememberResponse
    {
        public string CsrfToken { get; set; }

        public object? User { get; set; }
    }
}
