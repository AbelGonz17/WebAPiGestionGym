using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace ApiGym.Servicios
{
    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ServicioUsuarios(UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<IdentityUser> ObtenerUsuario()
        {
            var usuarioIdClaim = httpContextAccessor.HttpContext.User.Claims
         .Where(x => x.Type == "UsuarioId")  // Aquí buscas el claim "UsuarioId"
         .FirstOrDefault();

            if (usuarioIdClaim is null)
            {
                return null; // Si no se encuentra, el usuario no está autenticado correctamente
            }

            var usuarioId = usuarioIdClaim.Value;

            // Ahora, puedes usar este Id para obtener el usuario de la base de datos
            return await userManager.FindByIdAsync(usuarioId);
        }
    }
}
