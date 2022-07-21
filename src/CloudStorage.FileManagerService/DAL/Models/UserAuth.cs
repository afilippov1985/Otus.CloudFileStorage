#nullable disable

namespace CloudStorage.FileManagerService.DAL.Models
{

    /// <summary>
    /// Модель для аутентификации пользователя
    /// </summary>
    public class UserAuth
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; }
        
        /// <summary>
        /// Пароль
        /// </summary>
        public string UserPassword { get; set; }

        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime RegDate { get; set; }
    }
}
