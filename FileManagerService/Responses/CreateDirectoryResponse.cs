using FileManagerService.Models;
using System.Collections.Generic;

namespace FileManagerService.Responses
{
    public class CreateDirectoryResponse
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

        public CreateDirectoryResponse()
        {
            Tree = new List<TreeAttributes>();
        }
    }
}
