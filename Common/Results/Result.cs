namespace Common.Results
{
    /// <summary>
    /// Результат выполнения операции
    /// </summary>
    public sealed class Result
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

    public enum Status
    {
        Success,

        Warning,

        Danger
    }
}
