using FacturacionelectronicaCore.Negocio.Estacion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Web.Controllers
{
    /// <summary>
    /// Nombres de las estaciones por Guid, para las consultas que combinan todas las estaciones.
    /// </summary>
    public static class NombresEstaciones
    {
        public const string MensajeIdentificacionRequerida = "Debe indicar la identificación del cliente para consultar todas las estaciones";

        /// <summary>
        /// Rango máximo de la consulta de todas las estaciones (evita traer todo el historial del cliente).
        /// </summary>
        public const int DiasMaximosConsulta = 366;

        /// <summary>
        /// Mensaje de error si el rango de fechas falta o es inválido; null si es válido.
        /// </summary>
        public static string ValidarRango(DateTime? fechaInicial, DateTime? fechaFinal)
        {
            if (!fechaInicial.HasValue || !fechaFinal.HasValue)
            {
                return "Debe indicar la fecha inicial y la fecha final";
            }
            if (fechaInicial.Value > fechaFinal.Value)
            {
                return "La fecha inicial no puede ser mayor que la fecha final";
            }
            if ((fechaFinal.Value - fechaInicial.Value).TotalDays > DiasMaximosConsulta)
            {
                return $"El rango de fechas no puede superar {DiasMaximosConsulta} días";
            }
            return null;
        }

        public static async Task<Func<string, string>> Obtener(IEstacionNegocio estacionNegocio)
        {
            var estaciones = await estacionNegocio.GetEstaciones() ?? Enumerable.Empty<Negocio.Modelo.Estacion>();
            var nombres = estaciones
                .GroupBy(x => x.guid.ToString(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Nombre, StringComparer.OrdinalIgnoreCase);

            return guid => !string.IsNullOrWhiteSpace(guid) && nombres.TryGetValue(guid.Trim(), out var nombre)
                ? nombre
                : "Sin estación";
        }
    }
}
