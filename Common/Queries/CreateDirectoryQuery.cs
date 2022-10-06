﻿namespace Common.Queries
{
    public class CreateDirectoryQuery
    {
        /// <summary>
        /// Название диска.
        /// </summary>
        public string Disk { get; set; }

        /// <summary>
        /// Наименование создаваемого каталога.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Путь до каталога.
        /// </summary>
        public string Path { get; set; }
    }
}
