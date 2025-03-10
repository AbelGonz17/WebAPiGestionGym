using ApiGym.Entidades;

namespace ApiGym.DTOs
{
    public class CreacionPagoDTO
    {
        public string usuarioId { get; set; }
        public decimal monto { get; set; }
        public DateTime FechaPago { get; set; }
        public MetodosDePago metodoDePago { get; set; }
    }
}
