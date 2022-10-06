using static Common.Models.DirectoryAttributes;

namespace Common.Queries
{
    public class RenameQuery
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
