using System.ComponentModel.DataAnnotations;

namespace ApiGym.DTOs
{
    public class EditarClaimDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}
