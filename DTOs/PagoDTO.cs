using ApiGym.Entidades;

namespace ApiGym.DTOs
{
    public class PagoDTO
    {
        public int Id { get; set; }
        public string usuarioId { get; set; }
        public decimal monto { get; set; }
        public DateTime FechaPago { get; set; }
        public MetodosDePago metodoDePago { get; set; }
    }
}
