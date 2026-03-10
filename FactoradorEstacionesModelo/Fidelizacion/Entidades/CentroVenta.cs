using System.ComponentModel.DataAnnotations;

namespace Dominio.Entidades
{
    public class CentroVenta
    {
        public CentroVenta()
        {
        }

        [Key]
        public int Id { get; set; }
        public string Nit { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public int ValorPorPunto { get; set; }
        public int CompaniaId { get; set; }
        public Compania? Compania { get; set; }
        public int CiudadId { get; set; }
        public Ciudad? Ciudad { get; set; }
        public int EstadoId { get; set; }
        public Estado? Estado { get; set; }
    }
}
