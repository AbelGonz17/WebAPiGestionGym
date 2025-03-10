using Microsoft.AspNetCore.Identity;

namespace ApiGym.Servicios
{
    public interface IServicioUsuarios
    {
        Task<IdentityUser> ObtenerUsuario();
    }
}