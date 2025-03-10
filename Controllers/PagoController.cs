using ApiGym.DTOs;
using ApiGym.Entidades;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace ApiGym.Controllers
{
    [ApiController]
    [Route("api/pago")]
    public class PagoController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCache;

        public PagoController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCache)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCache = outputCache;
        }

        [HttpGet]
        public async Task<ActionResult<List<PagoDTO>>> Get()
        {
            try
            {
                var pagos = await context.Pagos
                    .AsNoTracking()
                    .OrderBy(u => u.Id)
                    .ProjectTo<PagoDTO>(mapper.ConfigurationProvider)
                    .ToListAsync();

                if (!pagos.Any())
                {
                    return NoContent(); // 204 si no hay pagos registrados
                }

                return Ok(pagos); // 200 con la lista de pagos
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [HttpGet("{id:int}", Name = "obtenerPagoId")]
        public async Task<ActionResult<PagoDTO>> GetId(int Id)
        {
            var pago = await context.Pagos.FirstOrDefaultAsync(x => x.Id == Id);
            if (pago == null)
            {
                return NotFound($"El pago con el ID {Id} no se encuentra en el sistema");
            }
            return mapper.Map<PagoDTO>(pago);
        }


        [HttpPost]
        public async Task<ActionResult> Post(CreacionPagoDTO creacionPagoDTO)
        {
            var membresia = await context.Membresias
                .FirstOrDefaultAsync(x => x.usuarioId == creacionPagoDTO.usuarioId);

            if(membresia == null)
            {
                return NotFound("el usuario no tiene una membresia registrada");
            }

            if(membresia.Estado == Entidades.EstadoMembresia.Activa)
            {
                return BadRequest("la membresia esta activa, no es necesario pagar");
            }

            var plan = await context.Plan
                .FirstOrDefaultAsync(x => x.Id == membresia.planId);
            if(plan == null)
            {
                return NotFound("el plan de la membresia no existe");
            }

            if(creacionPagoDTO.monto < plan.Precio)
            {
                return BadRequest("el monto ingresado es menor al precio del plan");
            }

            var usuarioId = membresia.usuarioId;

            var pago = new Pago
            {
                usuarioId = usuarioId,
                monto = creacionPagoDTO.monto,
                FechaPago = DateTime.UtcNow,
                metodoDePago = creacionPagoDTO.metodoDePago
            };

            context.Add(pago);

            membresia.FechaInicio= DateTime.UtcNow;
            membresia.FechaFin = DateTime.UtcNow.AddMonths(plan.Duracion);
            membresia.Estado = Entidades.EstadoMembresia.Activa;

            await context.SaveChangesAsync();

            return Ok("Pago realizado con exito. Membresia activada");

        }
    }
}
