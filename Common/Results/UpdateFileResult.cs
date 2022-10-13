using Common.Models;
namespace Common.Results
{
    public class UpdateFileResult
    {
        /// <summary>
        /// результат
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// описание созданного документа
        /// </summary>
        public Common.Models.FileAttributes File { get; set; }
    }
}
