using ApiGym.DTOs;
using ApiGym.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;

namespace ApiGym.Controllers
{
    [ApiController]
    [Route("api/Users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<IdentityUser> userManager;
        private readonly IConfiguration configuration;
        private readonly SignInManager<IdentityUser> signInManager;

        public UserController(ApplicationDbContext context,UserManager<IdentityUser> userManager, IConfiguration configuration
            , SignInManager<IdentityUser> signInManager)
        {
            this.context = context;
            this.userManager = userManager;
            this.configuration = configuration;
            this.signInManager = signInManager;
        }

        [HttpPost("Registro")]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Registrar(CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var user = new IdentityUser
            {
                UserName = credencialesUsuarioDTO.Email,
                Email = credencialesUsuarioDTO.Email
            };

            var resultado = await userManager.CreateAsync(user, credencialesUsuarioDTO.Password);

            if (!resultado.Succeeded)
            {
                foreach (var error in resultado.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return ValidationProblem();
            }

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Email = credencialesUsuarioDTO.Email,
                Name = credencialesUsuarioDTO.name,
                FechaRegistro = DateTime.UtcNow,
                Rol = credencialesUsuarioDTO.Rol
            };

            context.Add(usuario);
            await context.SaveChangesAsync();

            await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, usuario.Rol.ToString()));

            var respuestaAutenticacion = await ConstruirToken(credencialesUsuarioDTO);
            return respuestaAutenticacion;
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesUsuarioDTO credencialesUsuario)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);

            if(usuario is null)
            {
                return RetornarLoginIncorrecto();
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, credencialesUsuario.Password, lockoutOnFailure:false);

            if (resultado.Succeeded)
            {
                return await ConstruirToken(credencialesUsuario);
            }
            else
            {
                return RetornarLoginIncorrecto();
            }
        }

        private ActionResult RetornarLoginIncorrecto()
        {
            ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrecta");
            return ValidationProblem();
        }

        private async Task<RespuestaAutenticacionDTO> ConstruirToken (CredencialesUsuarioDTO credencialesUsuarioDTO)
        {
            var usuario = await context.Usuario
                .FirstOrDefaultAsync(x => x.Email == credencialesUsuarioDTO.Email);

            if(usuario == null)
            {
                throw new Exception("usuario no existe");
            }

            var claims = new List<Claim>
            {
                new Claim("Email",credencialesUsuarioDTO.Email),
                new Claim("UsuarioId", usuario.Id.ToString()),
                new Claim("Rol",usuario.Rol.ToString())
            };

            var user = await userManager.FindByEmailAsync(credencialesUsuarioDTO.Email);
            var claimsDB = await userManager.GetClaimsAsync(user);

            claims.AddRange(claimsDB);  

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddYears(1);

            var tokenDeSeguridad = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiracion,
                signingCredentials: credenciales
            );

            var token = new JwtSecurityTokenHandler().WriteToken(tokenDeSeguridad);

            return new RespuestaAutenticacionDTO
            { 
                Token = token, 
                Expiracion = expiracion 
            };

        }

    }
}
