namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// данные тела запроса на переименование файла/папки
    /// </summary>
    public class RenameRequest
    {
        public string Disk { get; set; }
        /// <summary>
        /// тип объекта
        /// </summary>
        public EntityType Type { get; set; }
        /// <summary>
        /// старое название
        /// </summary>
        public string OldName { get; set; }
        /// <summary>
        /// новое название
        /// </summary>
        public string NewName { get; set; }
    }
}
