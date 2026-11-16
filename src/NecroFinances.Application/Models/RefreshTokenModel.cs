using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NecroFinances.Application.Models
{
    [Table("RefreshTokens")]
    public class RefreshTokenModel
    {
        [Key]
        [Column("Id")]
        public long Id { get; set; }

        [Column("UserID")]
        public long UserID { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("TokenHash", TypeName = "char(64)")]
        public string TokenHash { get; set; } = string.Empty;

        [Column("ExpiresAt")]
        public DateTime ExpiresAt { get; set; }
    }
}
