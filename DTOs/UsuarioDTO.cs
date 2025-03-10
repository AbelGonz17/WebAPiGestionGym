using ApiGym.Entidades;

namespace ApiGym.DTOs
{
    public class UsuarioDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime FechaRegistro { get; set; }
        public RolUsuario Rol { get; set; }
        
    }
}
