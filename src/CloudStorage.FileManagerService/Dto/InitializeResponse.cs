
#nullable disable

namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// Ответ на запрос инициализации сервиса
    /// </summary>
    public class InitializeResponse
    {
        /// <summary>
        /// Результат инициализации
        /// </summary>
        public InitializeResult Result { get; set; }

        /// <summary>
        /// Конфигурация
        /// </summary>
        public ConfigResult Config { get; set; }


    }

    /// <summary>
    /// Результат инициализации
    /// </summary>
    public class InitializeResult
    {
        /// <summary>
        /// Статус
        /// </summary>
        public InitializeStatus Status { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum InitializeStatus
    {
        /// <summary>
        /// 
        /// </summary>
        Success,

        /// <summary>
        /// 
        /// </summary>
        Warning,

        /// <summary>
        /// 
        /// </summary>
        Danger
    }

    /// <summary>
    /// 
    /// </summary>
    public class ConfigResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool Acl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LeftDisk { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RightDisk { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string LeftPath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RightPath { get; set; }

        /// <summary>
        /// Конфиг Windows
        /// </summary>
        public int WindowsConfig { get; set; }

        /// <summary>
        /// Скрытые файлы
        /// </summary>
        public bool HiddenFiles { get; set; }

        /// <summary>
        /// Язык
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Disks { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum WindowsConfig
    {
        /// <summary>
        /// 
        /// </summary>
        OneManager = 1,

        /// <summary>
        /// 
        /// </summary>
        OneManagerWithFolderTree,

        /// <summary>
        /// 
        /// </summary>
        TwoManagers
    }
}
