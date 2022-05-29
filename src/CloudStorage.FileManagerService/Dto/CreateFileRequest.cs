namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// данные запроса создания документа 
    /// </summary>
    public class CreateFileRequest
    {
        /// <summary>
        /// путь
        /// </summary>
        public string Path { get; set; }    
        /// <summary>
        /// наименование с расширением
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Disk { get; set; }
    }
}
