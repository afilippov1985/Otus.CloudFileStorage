using Core.Domain.ValueObjects;
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
        public DirectoryProperties Directory { get; set; }

        /// <summary>
        /// Информация о всех каталогах по указанному пути
        /// </summary>
        public IList<TreeProperties> Tree { get; set; } = new List<TreeProperties>();

        public CreateDirectoryResponse()
        {
            Tree = new List<TreeProperties>();
        }
    }
}
