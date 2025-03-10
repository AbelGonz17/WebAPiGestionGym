using ApiGym.DTOs;
using ApiGym.Entidades;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace ApiGym.Controllers
{
    [ApiController]
    [Route("api/plan")]
    public class PlanController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IOutputCacheStore outputCache;
        private const string cacheTag = "plan";

        public PlanController(ApplicationDbContext context, IMapper mapper, IOutputCacheStore outputCache)
        {
            this.context = context;
            this.mapper = mapper;
            this.outputCache = outputCache;
        }

        [HttpGet]
        [OutputCache(Tags = [cacheTag])]
        public async Task<ActionResult<List<PlanDTO>>> Get()
        {
            var Plan = await context.Plan
            .OrderBy(u => u.Nombre)
            .ProjectTo<PlanDTO>(mapper.ConfigurationProvider)
            .ToListAsync();

            return Plan;
        }

        [HttpGet("{id:int}", Name = "obtenerPlanId")]
        [OutputCache(Tags = [cacheTag])]
        public async Task<ActionResult<PlanDTO>> GetId(int id)
        {
            var plan = await context.Plan.FirstOrDefaultAsync(x => x.Id == id );
            if (plan == null)
            {
                return NotFound($"El Usuario con el ID {id} no se encuentra en el sistema");
            }
            return mapper.Map<PlanDTO>(plan);
        }

        [HttpPost]
        public async Task<ActionResult> Post(CreacionPlanDTO creacionPlanDTO)
        {
            var plan = mapper.Map<Plan>(creacionPlanDTO);
            context.Add(plan);
            await context.SaveChangesAsync();
            return CreatedAtRoute("obtenerPlanId", new { id = plan.Id }, plan);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, CreacionPlanDTO creacionPlanDTO)
        {
           var existe = await context.Plan.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound($"El plan con el ID {id} no se encuentra en el sistema");
            }

            var plan = mapper.Map(creacionPlanDTO, existe);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Plan.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound($"El plan con el ID {id} no se encuentra en el sistema");
            }

            context.Remove(new Plan { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }

}
