using AutoMapper;
using EstacionesServicio.Repositorio.Entities;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Turno
{
    public interface ITurnoNegocio {

        Task Add(Modelo.Turno turno);
        Task<IEnumerable<Modelo.TurnoReporte>> Get(DateTime fechaInicial, DateTime fechaFinal, string surtidor);
        Task<IEnumerable<Modelo.TurnoReporte>> GetDia(Modelo.FiltroTurnoDia filtro);
    }
    public class TurnoNegocio : ITurnoNegocio
    {
        private readonly ITurnoRepositorio _turnoRepositorio;
        private readonly IMapper _mapper;

        public TurnoNegocio(ITurnoRepositorio turnoRepositorio, IMapper mapper)
        {
            _turnoRepositorio = turnoRepositorio;
            _mapper = mapper;
        }

        public async Task Add(Modelo.Turno turno)
        {
            await _turnoRepositorio.Add(_mapper.Map<Repositorio.Entities.Turno>(turno));
        }

        public async Task<IEnumerable<Modelo.TurnoReporte>> Get(DateTime fechaInicial, DateTime fechaFinal, string surtidor)
        {
            var turnos = _mapper.Map<IEnumerable<Modelo.Turno>>(await _turnoRepositorio.Get(fechaInicial, fechaFinal, surtidor));
            return ConstruirReporte(turnos, incluirTotales: true);
        }

        public async Task<IEnumerable<Modelo.TurnoReporte>> GetDia(Modelo.FiltroTurnoDia filtro)
        {
            var fecha = filtro.Fecha.Date;
            var turnos = _mapper.Map<IEnumerable<Modelo.Turno>>(await _turnoRepositorio.Get(fecha, fecha, filtro.Estacion.ToString()));
            var reporte = ConstruirReporte(turnos, incluirTotales: false);

            if (!string.IsNullOrWhiteSpace(filtro.Empleado))
            {
                reporte = reporte.Where(x => !string.IsNullOrWhiteSpace(x.Empleado)
                    && x.Empleado.IndexOf(filtro.Empleado, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Isla))
            {
                reporte = reporte.Where(x => !string.IsNullOrWhiteSpace(x.Isla)
                    && string.Equals(x.Isla.Trim(), filtro.Isla.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (filtro.NumeroTurno.HasValue)
            {
                reporte = reporte.Where(x => x.NumeroTurno == filtro.NumeroTurno.Value);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Surtidor))
            {
                reporte = reporte.Where(x => !string.IsNullOrWhiteSpace(x.Surtidor)
                    && x.Surtidor.IndexOf(filtro.Surtidor, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Manguera))
            {
                reporte = reporte.Where(x => !string.IsNullOrWhiteSpace(x.Manguera)
                    && x.Manguera.IndexOf(filtro.Manguera, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(filtro.Combustible))
            {
                reporte = reporte.Where(x => !string.IsNullOrWhiteSpace(x.Combustible)
                    && x.Combustible.IndexOf(filtro.Combustible, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return reporte;
        }

        private static IEnumerable<Modelo.TurnoReporte> ConstruirReporte(IEnumerable<Modelo.Turno> turnos, bool incluirTotales)
        {
            var turnosreporte = new List<Modelo.TurnoReporte>();
            var diferenciaGeneral = 0d;
            var totalGeneral = 0d;
            foreach (var turno in turnos)
            {
                var turnoDesc = $"{turno.FechaApertura.ToString("dd-MM-yyyy")}- Isla {turno.Isla.Trim()} - Número {turno.Numero}";
                var diferenciaTotal = 0d;
                var totalTotal = 0d;
                foreach(var turnosur in turno.turnoSurtidores)
                {
                    // Si el cierre es 0, colocarlo igual a apertura
                    var cierreAjustado = turnosur.Cierre.Value == 0 ? turnosur.Apertura : turnosur.Cierre.Value;
                    
                    var totalReporte = new Modelo.TurnoReporte {
                        FechaTurno = turno.FechaApertura,
                        Empleado = turno.Empleado,
                        Isla = turno.Isla,
                        NumeroTurno = turno.Numero,
                        Apertura = turnosur.Apertura,
                        Cierre = cierreAjustado,
                        Combustible = turnosur.Combustible,
                        Diferencia = cierreAjustado - turnosur.Apertura,
                        Manguera = turnosur.Manguera,
                        Precio = turnosur.precioCombustible,
                        Surtidor = turnosur.Surtidor,
                        Total = (cierreAjustado - turnosur.Apertura) * turnosur.precioCombustible,
                        turno = turnoDesc 
                    };
                    diferenciaTotal += totalReporte.Diferencia;
                    totalTotal += totalReporte.Total;
                    turnosreporte.Add(totalReporte);
                }
                diferenciaGeneral += diferenciaTotal;
                totalGeneral += totalTotal;

            }

            if (incluirTotales)
            {
                turnosreporte.Add(new TurnoReporte()
                {
                    Diferencia = diferenciaGeneral,
                    Total = totalGeneral,
                    turno = "Total"
                });
            }

            return turnosreporte;
        }
    }
}
