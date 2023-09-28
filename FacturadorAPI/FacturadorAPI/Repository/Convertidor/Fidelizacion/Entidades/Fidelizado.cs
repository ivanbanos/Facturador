using System;

namespace Dominio.Entidades
{
    public class Fidelizado
    {
        public int Id { get; set; }
        public Guid? Guid { get; set; }
        public string Documento { get; set; }
        public int TipoDocumentoId { get; set; }
        public string Nombre { get; set; }
        public string? Contrasena { get; set; }
        public float? Puntos { get; set; }
        public float PorcentajePuntos { get; set; }
        public float? PuntosReservados { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public DateTime? FechaUltimoReclamo { get; set; }
        public int CentroVentaId { get; set; }
        public virtual CentroVenta? CentroVenta { get; set; }
        public int EstadoId { get; set; }
        public virtual Estado? Estado { get; set; }
        public virtual InformacionAdicional InformacionAdicional { get; set; }
    }
}
