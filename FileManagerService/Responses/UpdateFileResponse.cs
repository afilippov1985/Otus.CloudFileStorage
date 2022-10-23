using Core.Domain.ValueObjects;

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
        public FileProperties File { get; set; }
    }
}
