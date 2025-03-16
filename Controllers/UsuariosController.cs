using ApiGym.DTOs;
using ApiGym.Entidades;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace ApiGym.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize(Policy = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private const string cacheTag = "usuario";

        public UsuariosController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCacheStore)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
        }

        [HttpGet]
        [OutputCache(Tags = [cacheTag])]
        public async Task<ActionResult<List<UsuarioDTO>>> Get()
        {
            var usuarios = await context.Usuario
           .OrderBy(u => u.Name)
           .ProjectTo<UsuarioDTO>(mapper.ConfigurationProvider) // Aquí se usa ProjectTo
           .ToListAsync();

            return usuarios;
        }

        [HttpGet("{id:guid}", Name ="obtenerUsarioId")]
        public async Task<ActionResult<UsuarioDTO>> GetId(Guid id)
        {
            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.Id == id);
            if (usuario == null)
            {
                return NotFound($"El Usuario con el ID {id} no se encuentra en el sistema");
            }

            return mapper.Map<UsuarioDTO>(usuario);
        }

        
        [HttpPut("{id:guid}")]
        public async Task<ActionResult> Put(Guid id, CreacionUsuarioDTO creacionUsuario)
        {
            var ExisteUsuario = await context.Usuario.AnyAsync(x => x.Id == id);

            if (!ExisteUsuario)
            {
                return NotFound($"El Usuario con el ID {id} no se encuentra en el sistema");
            }

            var usuario = mapper.Map(creacionUsuario,ExisteUsuario);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:guid}")]
        public async Task<ActionResult> Patch (Guid id , JsonPatchDocument<UsuarioPatchDTO> patchDocument)
        {
            if(patchDocument == null)
            {
                return BadRequest();
            }

            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.Id == id);

            if(usuario == null)
            {
                return NotFound($"El Usuario con el ID {id} no se encuentra en el sistema");
            }

            var usuarioDTO = mapper.Map<UsuarioPatchDTO>(usuario);

            patchDocument.ApplyTo(usuarioDTO);

            var isValid = TryValidateModel(usuarioDTO);

            if(!isValid)
            {
                return BadRequest(ModelState);
            }

            mapper.Map(usuarioDTO, usuario);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var ExisteUsuario = await context.Usuario.AnyAsync(x => x.Id == id);
            if (!ExisteUsuario)
            {
                return NotFound($"El Usuario con el ID {id} no se encuentra en el sistema");
            }
            context.Remove(new Usuario() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }

}
