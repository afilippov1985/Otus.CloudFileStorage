using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Common.Data
{
    public class Share
    {
        [Key]
        public int Id { get; set; }

        [Unicode(false)]
        [MaxLength(32)]
        public string PublicId { get; set; }

        public string UserId { get; set; }

        public string Disk { get; set; }

        public string Path { get; set; }

        public DateTime CreatedAt { get; set; }

        private static string GenerateRandomId()
        {
            var rdm = new Random();
            string hexValue = string.Empty;
            int num;

            for (int i = 0; i < 4; i++)
            {
                num = rdm.Next(0, int.MaxValue);
                hexValue += num.ToString("x8");
            }

            return hexValue;
        }

        public Share()
        {
            PublicId = GenerateRandomId();
        }
    }
}
