#nullable disable

using System.ComponentModel.DataAnnotations.Schema;

namespace CloudStorage.FileManagerService.DAL.Models
{

    /// <summary>
    /// Модель для аутентификации пользователя
    /// </summary>
    // [Table("users_auth", Schema = "public")]
    public class UserAuth
    {
        /// <summary>
        /// Идентификатор
        /// </summary>

        public int Id { get; set; }

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
