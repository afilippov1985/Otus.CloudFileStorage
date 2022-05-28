
#nullable disable

namespace CloudStorage.FileManagerService.Dto
{
    /// <summary>
    /// Ответ на запрос инициализации менеджера
    /// </summary>
    public class InitializeResponse
    {
        /// <summary>
        /// Результат инициализации
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// Конфигурация
        /// </summary>
        public ConfigResult Config { get; set; }
    }

    /// <summary>
    /// Настройки менеджера файлов
    /// </summary>
    public class ConfigResult
    {
        /// <summary>
        /// Проверять права доступа
        /// </summary>
        public bool Acl { get; set; }

        /// <summary>
        /// При установленном WindowsConfig = WindowsConfig.TwoManagers
        /// задаёт какой диск будет открыт по-умолчанию в левой панели
        /// </summary>
        public string LeftDisk { get; set; }

        /// <summary>
        /// При установленном WindowsConfig = WindowsConfig.TwoManagers
        /// задаёт какой диск будет открыт по-умолчанию в правой панели
        /// </summary>
        public string RightDisk { get; set; }

        /// <summary>
        /// При установленном WindowsConfig = WindowsConfig.TwoManagers
        /// задаёт какой путь будет открыт по-умолчанию в левой панели
        /// </summary>
        public string LeftPath { get; set; }

        /// <summary>
        /// При установленном WindowsConfig = WindowsConfig.TwoManagers
        /// задаёт какой путь будет открыт по-умолчанию в правой панели
        /// </summary>
        public string RightPath { get; set; }

        /// <summary>
        /// Задаёт внешний вид менеджера файлов
        /// </summary>
        public int WindowsConfig { get; set; }

        /// <summary>
        /// Отображать скрытые файлы
        /// </summary>
        public bool HiddenFiles { get; set; }

        /// <summary>
        /// Язык
        /// </summary>
        public string Lang { get; set; }

        /// <summary>
        /// Подключенные диски
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Disks { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum WindowsConfig
    {
        /// <summary>
        /// Один менеджер
        /// </summary>
        OneManager = 1,

        /// <summary>
        /// Один менеджер плюс дерево папок
        /// </summary>
        OneManagerWithFolderTree,

        /// <summary>
        /// Два менеджера
        /// </summary>
        TwoManagers
    }
}
