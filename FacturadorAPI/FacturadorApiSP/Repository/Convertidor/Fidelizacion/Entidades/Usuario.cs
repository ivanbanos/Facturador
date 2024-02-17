using System;
using System.ComponentModel.DataAnnotations;

namespace Dominio.Entidades
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public Guid? Guid { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
        public int PerfilId { get; set; }
        public virtual Perfil? Perfil { get; set; }
        public int EstadoId { get; set; }
        public virtual Estado? Estado { get; set; }
        public int? CentroVentaId { get; set; }
        public virtual CentroVenta? CentroVenta { get; set; }
    }
}
