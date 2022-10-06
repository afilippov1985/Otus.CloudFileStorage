namespace Common.Results
{
    public class ResultResult
    {
        public Result Result { get; set; }

        public ResultResult(Status status, string message)
        {
            Result = new(status, message);
        }
    }
}
