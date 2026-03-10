using System;
namespace EnviadorInformacionService.Models
{
    public class ObjetoImprimir
    {
        public int Id { get; set; }
        public DateTime fecha { get; set; }
        public int Isla { get; set; }
        public int Numero { get; set; }
        public string Objeto { get; set; }
        public int impreso { get; set; }
    }
}
