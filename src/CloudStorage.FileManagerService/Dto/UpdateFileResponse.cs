﻿namespace CloudStorage.FileManagerService.Dto
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