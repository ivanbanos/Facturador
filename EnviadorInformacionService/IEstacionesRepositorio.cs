using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;

namespace FacturadorEstacionesRepositorio
{
    public interface IEstacionesRepositorio
    {
        FactoradorEstacionesModelo.Objetos.Resolucion BuscarResolucionActiva(IEnumerable<FactoradorEstacionesModelo.Objetos.Resolucion> resolucionRemota);
        void CambiarConsecutivoActual(int v);
        List<FactoradorEstacionesModelo.Objetos.Tercero> BuscarTercerosNoEnviados();
        List<FactoradorEstacionesModelo.Objetos.Factura> BuscarFacturasNoEnviadas();
        List<FormasPagos> BuscarFormasPagos();
        void ActuralizarTercerosEnviados(IEnumerable<int> enumerable);
        void ActuralizarFacturasEnviados(IEnumerable<int> enumerable);
        void ActuralizarTerceros(FactoradorEstacionesModelo.Objetos.Tercero tercero);
        void AgregarFacturaDesdeIdVenta();
        void MandarImprimir(int ventaId);
        List<FactoradorEstacionesModelo.Objetos.Factura> BuscarFacturasNoEnviadasFacturacion();
        List<FactoradorEstacionesModelo.Objetos.Factura> GetFacturaSinEnviarTurno();
        void ActuralizarFacturasEnviadosFacturacion(IEnumerable<int> facturas);
        void ActuralizarFacturasEnviadosTurno(int factura);
        List<FacturaFechaReporte> BuscarFechasReportesNoEnviadas();
        void ActuralizarFechasReportesEnviadas(IEnumerable<int> facturas);
        Turno ObtenerTurnoIslaPorVenta(int ventaId);
        IEnumerable<ObjetoImprimir> GetObjetoImprimir();
        CuposRequest GetInfoCupos();
        void ActualizarPreciosCombustibles(IEnumerable<Combustible> combustibles);
        IEnumerable<TurnoSiges> GetTurnosByFechas(DateTime desde, DateTime hasta);
        IEnumerable<FactoradorEstacionesModelo.Siges.TurnoSurtidor> ObtenerTurnoInfo(int id);
        void PrepararRetroactivoTurnosPendientes();
        void AgregarFacturasDesdeIdVentaPorFecha(DateTime fecha);

    }
}
