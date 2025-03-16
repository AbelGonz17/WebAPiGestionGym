using ApiGym.DTOs;
using ApiGym.Entidades;
using ApiGym.Servicios;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiGym.Controllers
{
    [ApiController]
    [Route("api/membresias")]
    [Authorize(Policy ="Administrador")]
    public class MembresiaController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCacheStore;
        private readonly IServicioUsuarios servicioUsuarios;
        private const string cacheTag = "membresia";

        public MembresiaController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCacheStore, IServicioUsuarios servicioUsuarios)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCacheStore = outputCacheStore;
            this.servicioUsuarios = servicioUsuarios;
        }

        [HttpGet]
        public async Task<ActionResult<List<MembresiaDTO>>> GetAction()
        {
            var membresias = await context.Membresias
            .OrderBy(u => u.ID)
            .ProjectTo<MembresiaDTO>(mapper.ConfigurationProvider)
            .ToListAsync();
            return membresias;
        }

        [HttpGet("{id:int}", Name = "obtenerMembresiaId")]
        public async Task<ActionResult<MembresiaDTO>> GetId(int id)
        {
            var membresia = await context.Membresias.FirstOrDefaultAsync(x => x.ID == id);
            if (membresia == null)
            {
                return NotFound($"La membresia con el ID {id} no se encuentra en el sistema");
            }

            return mapper.Map<MembresiaDTO>(membresia);
        }
            

        [HttpGet("Inactivas",Name = "ObtenerMembresiasInactivas")]
        public async Task<ActionResult<List<MembresiaDTO>>> Get()
        {
            var usuarios = await context.Membresias
                .Where(x => x.Estado == EstadoMembresia.Inactiva)
                .OrderBy(x => x.FechaFin)
                .ProjectTo<MembresiaDTO>(mapper.ConfigurationProvider)
                .ToListAsync();


            return usuarios;
        }        

        [HttpPost]
        public async Task<ActionResult> Post(CreacionMembresiaDTO creacionMembresiaDTO)
        {
            var usuarioId = creacionMembresiaDTO.usuarioId;

            if (usuarioId == string.Empty)
            {
                return BadRequest("El ID del usuario no es válido.");
            }

            var ExisteUser = await context.Usuario
                .AnyAsync(x => x.Id.ToString() == creacionMembresiaDTO.usuarioId);
            if (!ExisteUser)
            {
                return BadRequest("El usuario no se encuentra en el sistema");
            }

            var membresiaDB = await context.Membresias      
                .AnyAsync(x => x.usuarioId == usuarioId);

            if(membresiaDB)
            {
                return BadRequest("El usuario ya tiene una membresía registrada");
            }
         
            var membresia = MapMembresia(creacionMembresiaDTO);
         
            context.Add(membresia);
            await context.SaveChangesAsync();
            return CreatedAtRoute("obtenerMembresiaId", new { id = membresia.ID }, membresia);
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<MembresiaPatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }
           
            var membresiaDB = await context.Membresias
                .Include(m => m.plan)
                .FirstOrDefaultAsync(x => x.ID == id);
            if (membresiaDB == null)
            {
                return NotFound($"La membresia con el ID {id} no se encuentra en el sistema");
            }
            var membresiaDTO = mapper.Map<MembresiaPatchDTO>(membresiaDB);

            patchDocument.ApplyTo(membresiaDTO);

            var isValid = TryValidateModel(membresiaDTO);
            if (!isValid)
            {
                return BadRequest(ModelState);
            }

            if (membresiaDB.plan.Id != membresiaDTO.planId)
            {
                // Si el plan ha cambiado, actualizar las fechas según la duración del nuevo plan
                var nuevoPlan = await context.Plan.FirstOrDefaultAsync(p => p.Id == membresiaDTO.planId);
                if (nuevoPlan != null)
                {
                    // Asignar la nueva fecha de inicio (por ejemplo, la fecha actual)
                    membresiaDB.FechaInicio = DateTime.Now;

                    // Asignar la nueva fecha de fin (calculada con la duración del nuevo plan)
                    membresiaDB.FechaFin = membresiaDB.FechaInicio.AddMonths(nuevoPlan.Duracion);
                }
            }

            mapper.Map(membresiaDTO, membresiaDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("expirar/{usuarioId}")]
        public async Task<ActionResult> ExpirarMembresia(string  usuarioId)
        {
            var membresia = await context.Membresias.FirstOrDefaultAsync(x => x.usuarioId == usuarioId);

            if (membresia == null)
            {
                return NotFound("Membresía no encontrada.");
            }

            membresia.FechaFin = DateTime.UtcNow.AddDays(-1);
            membresia.Estado = EstadoMembresia.Inactiva;

            await context.SaveChangesAsync();
            return Ok("Membresía expirada correctamente.");
        }



        private Membresia MapMembresia(CreacionMembresiaDTO membresiaCreacionDTO)
        {
           var plan = context.Plan.FirstOrDefault(x => x.Id == membresiaCreacionDTO.planId);
            if (plan == null)
            {
                throw new Exception($"El plan con el ID {membresiaCreacionDTO.planId} no se encuentra en el sistema");
            }

            var usuarioId = User.Claims.FirstOrDefault(x => x.Type == "UsuarioId").Value;

            // Mapea la información básica del DTO a la entidad Membresia
            var membresia = new Membresia
             {
                    usuarioId = usuarioId,
                    planId = membresiaCreacionDTO.planId,
                    FechaInicio = DateTime.Now,
                    FechaFin = DateTime.Now.AddMonths(plan.Duracion),
                    Estado = EstadoMembresia.Activa
            };

            // Si la fecha actual es mayor que la fecha de fin, el estado debe ser Inactivo
           if (DateTime.Now > membresia.FechaFin)
            { 
              membresia.Estado = EstadoMembresia.Inactiva;
            }

            return membresia;       
        }
    }
}
