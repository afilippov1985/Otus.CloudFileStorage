using Common.Models;

namespace Common.Results
{
    public class TreeResult
    {
        /// <summary>
        /// Результат выполения
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// Информация о всех каталогах по указанному пути
        /// </summary>
        public IList<TreeAttributes> Directories { get; set; }

        public TreeResult()
        {
            Directories = new List<TreeAttributes>();
        }
    }
}
