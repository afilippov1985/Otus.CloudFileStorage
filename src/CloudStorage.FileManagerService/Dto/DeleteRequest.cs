namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// тело запроса удаление объекта
    /// </summary>
    public class DeleteRequest
    {
        /// <summary>
        /// 
        /// </summary>
        public string Disk { get; set; }
        /// <summary>
        /// список объектов
        /// </summary>
        public IList<Item> Items { get; set; }
    }

    /// <summary>
    /// объект
    /// </summary>
    public class Item
    {
        /// <summary>
        /// путь
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// тип
        /// </summary>
        public EntityType Type { get; set; }
    }
}
