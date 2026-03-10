using EnviadorInformacionService.Models;
using EnviadorInformacionService.Models.Externos;
using FactoradorEstacionesModelo.Objetos;
using FacturacionelectronicaCore.Repositorio.Entities;
using ReporteFacturas;
using System;
using System.Collections.Generic;

namespace EnviadorInformacionService
{
    public interface IConexionEstacionRemota
    {
        string getToken();
        IEnumerable<string> GetGuidsFacturasPendientes(Guid estacion, string token);
        bool EnviarTerceros(IEnumerable<Tercero> terceros, string token);
        bool EnviarFacturas(IEnumerable<Factura> facturas, IEnumerable<FormasPagos> formas, Guid estacion, string token);
        IEnumerable<Tercero> RecibirTercerosActualizados(Guid estacion, string token);
        IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.Factura> RecibirFacturasImprimir(Guid estacion, string token);
        IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho> RecibirOrdenesImprimir(Guid estacion, string token);
        IEnumerable<Resolucion> GetResolucionEstacion(Guid estacionFuente, string token);
        InfoEstacion getInfoEstacion(Guid estacionFuente, string token);
        bool AgregarFechaReporteFactura(List<FacturaFechaReporte> facturasFechas, Guid estacionFuente, string token);
        string GetInfoFacturaElectronica(int ventaId, Guid estacionFuente, string v);
        IEnumerable<Canastilla> RecibirCanastilla(Guid estacion, string token);
        Resolucion GetResolucionEstacionCanastilla(Guid estacionFuente, string token);
        bool EnviarFacturasCanastilla(IEnumerable<FacturaCanastilla> facturas, Guid estacionFuente, string token);
        int ObtenerParaImprimir(Guid idEstacion, string token);
        ResolucionElectronica GetResolucionElectronica(string token);
        bool SetTurnoFactura(int ventaId, DateTime fechaApertura, string isla, int numero, Guid estacionFuente, string token);
        void SubirTurno(FacturacionelectronicaCore.Negocio.Modelo.Turno turno, Guid estacionFuente, string token);
        void SubirInfoCupos(CuposRequest cuposInfo, Guid estacionFuente, string token);
        string GetInfoFacturaElectronicaCanastilla(int consecutivo, Guid estacionFuente, string v);
    }
}
