using System.Collections.Generic;
using System.Reflection.Metadata;

namespace FileManagerService.Responses
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
        public string? LeftDisk { get; set; }

        /// <summary>
        /// При установленном WindowsConfig = WindowsConfig.TwoManagers
        /// задаёт какой диск будет открыт по-умолчанию в правой панели
        /// </summary>
        public string? RightDisk { get; set; }

        /// <summary>
        /// При установленном WindowsConfig = WindowsConfig.TwoManagers
        /// задаёт какой путь будет открыт по-умолчанию в левой панели
        /// </summary>
        public string? LeftPath { get; set; }

        /// <summary>
        /// При установленном WindowsConfig = WindowsConfig.TwoManagers
        /// задаёт какой путь будет открыт по-умолчанию в правой панели
        /// </summary>
        public string? RightPath { get; set; }

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
        public string? Lang { get; set; }

        /// <summary>
        /// Подключенные диски
        /// </summary>
        public IDictionary<string, Dictionary<string, string>> Disks { get; set; }

        public string ShareBaseUrl { get; set; }

        public IList<AddShareResponse> ShareList { get; set; }
    }

    /// <summary>
    /// Тип интерфейса
    /// </summary>
    public enum WindowsConfig
    {
        /// <summary>
        /// Один менеджер файлов
        /// </summary>
        OneManager = 1,

        /// <summary>
        /// Один менеджер плюс дерево папок
        /// </summary>
        OneManagerWithFolderTree,

        /// <summary>
        /// Два менеджера файлов
        /// </summary>
        TwoManagers
    }

    // паттерн Builder
    public class ConfigResultBuilder
    {
        private bool _acl = false;

        private string? _leftDisk = null;

        private string? _rightDisk = null;

        private string? _leftPath = null;

        private string? _rightPath = null;

        private WindowsConfig _windowsConfig = WindowsConfig.OneManager;

        private bool _hiddenFiles = false;

        private string? _lang = null;

        private IDictionary<string, Dictionary<string, string>> _disks = new Dictionary<string, Dictionary<string, string>>();

        private string _shareBaseUrl = "";

        private IList<AddShareResponse> _shareList = new List<AddShareResponse>();

        public ConfigResultBuilder SetWindowsConfig(WindowsConfig windowsConfig, string? leftDisk = null, string? leftPath = null, string? rightDisk = null, string? rightPath = null)
        {
            _windowsConfig = windowsConfig;
            _leftDisk = leftDisk;
            _leftPath = leftPath;
            _rightDisk = rightDisk;
            _rightPath = rightPath;

            return this;
        }

        public ConfigResultBuilder SetLang(string lang)
        {
            _lang = lang;

            return this;
        }
        
        public ConfigResultBuilder SetShowHiddenFiles(bool hiddenFiles)
        {
            _hiddenFiles = hiddenFiles;

            return this;
        }

        public ConfigResultBuilder SetShareBaseUrl(string shareBaseUrl)
        {
            _shareBaseUrl = shareBaseUrl;

            return this;
        }

        public ConfigResultBuilder SetShareList(IList<AddShareResponse> shareList)
        {
            _shareList = shareList;

            return this;
        }

        public ConfigResultBuilder AddDisk(string diskName, string diskDriver)
        {
            _disks.Add(diskName, new() {
                { "driver", "local" }
            });

            return this;
        }

        public ConfigResultBuilder AddShare(string disk, string path, string publicId)
        {
            _shareList.Add(new AddShareResponse() {
                Disk = disk,
                Path = path,
                PublicId = publicId,
            });

            return this;
        }

        public ConfigResult Build()
        {
            return new ConfigResult() {
                Acl = _acl,
                LeftDisk = _leftDisk,
                LeftPath = _leftPath,
                RightDisk = _rightDisk,
                RightPath = _rightPath,
                WindowsConfig = (int)_windowsConfig,
                HiddenFiles = _hiddenFiles,
                Lang = _lang,
                Disks = _disks,
                ShareBaseUrl = _shareBaseUrl,
                ShareList = _shareList,
            };
        }
    }
}
