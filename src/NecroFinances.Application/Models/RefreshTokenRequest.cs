using System.ComponentModel.DataAnnotations;

namespace NecroFinances.Application.Models
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
