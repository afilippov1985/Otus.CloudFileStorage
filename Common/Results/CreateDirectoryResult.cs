using Common.Models;

namespace Common.Results
{
    public class CreateDirectoryResult
    {
        /// <summary>
        /// Результат выполения
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// Информация о созданном каталоге
        /// </summary>
        public DirectoryAttributes Directory { get; set; }

        /// <summary>
        /// Информация о всех каталогах по указанному пути
        /// </summary>
        public IList<TreeAttributes> Tree { get; set; }

        public CreateDirectoryResult()
        {
            Tree = new List<TreeAttributes>();
        }
    }
}
