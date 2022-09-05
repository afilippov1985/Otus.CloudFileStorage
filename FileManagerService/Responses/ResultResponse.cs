namespace FileManagerService.Responses
{
    public class ResultResponse
    {
        public Result Result { get; set; }

        public ResultResponse(Status status, string message)
        {
            Result = new(status, message);
        }
    }
}
