using ApiGym.Entidades;

namespace ApiGym.DTOs
{
    public class MembresiaDTO
    {
        public int ID { get; set; }
        public int planId { get; set; }
        public string usuarioId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public EstadoMembresia Estado { get; set; }    
    }
}
