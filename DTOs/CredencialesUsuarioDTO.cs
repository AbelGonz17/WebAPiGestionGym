using ApiGym.Entidades;
using System.ComponentModel.DataAnnotations;

namespace ApiGym.DTOs
{
    public class CredencialesUsuarioDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        [EnumDataType(typeof(RolUsuario), ErrorMessage = "Rol inválido. Opciones válidas: Cliente, Administrador.")]
        public RolUsuario Rol { get; set; }
    }
}
