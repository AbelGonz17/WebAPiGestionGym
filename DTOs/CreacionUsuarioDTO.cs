using ApiGym.Entidades;

namespace ApiGym.DTOs
{
    public class CreacionUsuarioDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime FechaRegistro { get; set; }
        public RolUsuario Rol { get; set; }
        
    }
}
