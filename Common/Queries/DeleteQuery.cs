using static Common.Models.DirectoryAttributes;

namespace Common.Queries
{
    public class DeleteQuery
    {
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

        public string Disk { get; set; }

        /// <summary>
        /// список объектов
        /// </summary>
        public IList<Item> Items { get; set; }
    }
}
