using ApiGym.DTOs;
using ApiGym.Entidades;
using AutoMapper;

namespace ApiGym.Utilities
{
    public class AutoMapperProfile:Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Usuario, UsuarioDTO>();
            CreateMap<CreacionUsuarioDTO, Usuario>();

            CreateMap<Plan, PlanDTO>();
            CreateMap<CreacionPlanDTO, Plan>();

            CreateMap<Membresia, MembresiaDTO>();
            CreateMap<CreacionMembresiaDTO, Membresia>();
            CreateMap<MembresiaPatchDTO, Membresia>().ReverseMap();

            CreateMap<Pago, PagoDTO>().ReverseMap();
            CreateMap<CreacionPagoDTO, Pago>().ReverseMap();

        }
    }
}
