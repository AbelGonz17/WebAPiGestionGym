namespace ApiGym.Entidades
{
    public class Membresia
    {
        public int ID { get; set; }
        public string usuarioId { get; set; }
        public int planId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public EstadoMembresia Estado { get; set; }
        public Usuario usuario { get; set; }
        public Plan plan { get; set; }
    }

    public enum EstadoMembresia
    {
        Activa = 1,
        Inactiva = 2
    }
}
