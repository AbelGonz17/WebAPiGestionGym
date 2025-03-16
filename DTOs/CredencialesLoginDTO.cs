using System.ComponentModel.DataAnnotations;

namespace ApiGym.DTOs
{
    public class CredencialesLoginDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
