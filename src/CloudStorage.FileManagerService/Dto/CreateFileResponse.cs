namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// ответ на запрос создания документа 
    /// </summary>
    public class CreateFileResponse
    {
        /// <summary>
        /// результат
        /// </summary>
        public Result Result { get;set;}

        /// <summary>
        /// описание созданного документа
        /// </summary>
        public FileAttributes File { get; set;} 
    }
}
