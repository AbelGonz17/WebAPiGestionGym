using System.ComponentModel.DataAnnotations.Schema;

namespace ApiGym.Entidades
{
    public class Pago
    {
        public int Id { get; set; }
        public string usuarioId { get; set; }
        public decimal monto { get; set; }
        public DateTime FechaPago { get; set; }
        public MetodosDePago metodoDePago { get; set; }
      

    }

    public enum MetodosDePago
    { 
        efectivo = 1,
        tarjeta =2

    }
}
