using FileManagerService.Models;

namespace FileManagerService.Requests
{
    public class RenameRequest
    {
        public string Disk { get; set; }

        /// <summary>
        /// тип объекта
        /// </summary>
        public EntityType Type { get; set; }

        /// <summary>
        /// старое имя
        /// </summary>
        public string OldName { get; set; }

        /// <summary>
        /// новое имя
        /// </summary>
        public string NewName { get; set; }
    }
}
