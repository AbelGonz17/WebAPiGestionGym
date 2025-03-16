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
using System.Linq;

namespace ApiGym.Controllers
{
    [ApiController]
    [Route("api/Users")]
    [Authorize(Policy = "Administrador")]
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
                Id = new Guid(user.Id),
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
        public async Task<ActionResult<RespuestaAutenticacionDTO>> Login(CredencialesLoginDTO credencialesUsuario)
        {
            var usuario = await userManager.FindByEmailAsync(credencialesUsuario.Email);

            if(usuario is null)
            {
                return RetornarLoginIncorrecto();
            }

            var resultado = await signInManager.CheckPasswordSignInAsync(usuario, credencialesUsuario.Password, lockoutOnFailure:false);

            if (!resultado.Succeeded)
            {
                return RetornarLoginIncorrecto();
            }

            var claims = await userManager.GetClaimsAsync(usuario);
            var rolClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
            var rol = rolClaim?.Value ?? "Cliente";

            var credencialesUsuarioDTO = new CredencialesUsuarioDTO
            {
                Email = credencialesUsuario.Email,
                Password = credencialesUsuario.Password,
                name = usuario.UserName,
                Rol = (RolUsuario)Enum.Parse(typeof(RolUsuario), rol)
            };

            return await ConstruirToken(credencialesUsuarioDTO);

        }

        [HttpPost("hacer-admin")]
        public async Task<ActionResult> HacerAdmin(EditarClaimDTO editarClaimDTO)
        {
            var user = await userManager.FindByEmailAsync(editarClaimDTO.Email);
            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.Email == editarClaimDTO.Email);
            if(user == null )
            {
                return NotFound();
            }

            var claims = await userManager.GetClaimsAsync(user);

            var claimsRol = claims.Where(c => c.Type == ClaimTypes.Role).ToList();

            foreach(var claim in claimsRol)
            {
                await userManager.RemoveClaimAsync(user, claim);
            }

            var newClaim = new Claim(ClaimTypes.Role, RolUsuario.Administrador.ToString());
            var resultado = await userManager.AddClaimAsync(user,newClaim);
            usuario.Rol = RolUsuario.Administrador;
            context.Usuario.Update(usuario);
            await context.SaveChangesAsync();

            if (!resultado.Succeeded)
            {
                return BadRequest(resultado.Errors);
            }

            return Ok("Usuario promovido a administrador y roles anteriores eliminados");
        }

        [HttpPost("remover-admin")]
        public async Task<ActionResult> RemoverAdmin(EditarClaimDTO editarClaimDTO)
        {
            var user = await userManager.FindByEmailAsync(editarClaimDTO.Email);
            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.Email == editarClaimDTO.Email);

            if (user == null || usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var claims = await userManager.GetClaimsAsync(user);
            var claim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role && c.Value == RolUsuario.Administrador.ToString());

            if(claim == null)
            {
                return BadRequest("el usuario no tiene el rol de administrador");
            }

            var resultado = await userManager.RemoveClaimAsync(user, claim);       
            if (!resultado.Succeeded)
            {
                return BadRequest(resultado.Errors);
            }

            var newClaim = new Claim(ClaimTypes.Role, RolUsuario.Cliente.ToString());
            var results = await userManager.AddClaimAsync(user, newClaim);
            if(!results.Succeeded)
            {
                return BadRequest(results.Errors);
            }

            usuario.Rol = RolUsuario.Cliente;
            context.Usuario.Update(usuario);
            await context.SaveChangesAsync();
            

            return Ok("Usuario ya no es administrador");
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
