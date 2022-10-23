using System.Collections.Generic;

namespace FileManagerService.Responses
{
    public class TreeResponse
    {
        /// <summary>
        /// Результат выполения
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// Информация о всех каталогах по указанному пути
        /// </summary>
        // FIX
        // public IList<TreeAttributes> Directories { get; set; }

        public TreeResponse()
        {
            // Directories = new List<TreeAttributes>();
        }
    }
}
