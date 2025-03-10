using Microsoft.AspNetCore.Identity;

namespace ApiGym.Entidades
{
    public class Usuario
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public RolUsuario Rol { get; set; }
    }
    
    public enum RolUsuario
    {
        Administrador = 1,
        Cliente = 2
    }


}
