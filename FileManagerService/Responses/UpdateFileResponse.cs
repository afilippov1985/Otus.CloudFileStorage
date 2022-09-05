using FileManagerService.Models;

namespace FileManagerService.Responses
{
    public class UpdateFileResponse
    {
        /// <summary>
        /// результат
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// описание созданного документа
        /// </summary>
        public FileAttributes File { get; set; }
    }
}
