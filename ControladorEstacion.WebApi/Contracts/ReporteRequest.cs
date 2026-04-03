using System.ComponentModel.DataAnnotations;

namespace ControladorEstacion.WebApi.Contracts;

public sealed class ReporteRequest
{
    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }
}
