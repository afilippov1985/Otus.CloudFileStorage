namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// Результат выполнения операции
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public Result(Status status, string message)
        {
            Status = status;
            Message = message;
        }

        /// <summary>
        /// Статус
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// 
        /// </summary>
        Success,

        /// <summary>
        /// 
        /// </summary>
        Warning,

        /// <summary>
        /// 
        /// </summary>
        Danger
    }
}
