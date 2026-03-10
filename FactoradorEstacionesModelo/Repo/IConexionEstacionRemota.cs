using EnviadorInformacionService.Models;
using EnviadorInformacionService.Models.Externos;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using FacturacionelectronicaCore.Repositorio.Entities;
using Modelo;
using System;
using System.Collections.Generic;

namespace FacturadorEstacionesPOSWinForm.Repo
{
    public interface IConexionEstacionRemota
    {
        bool GetIsTerceroValidoPorIdentificacion(string identificacion);

        string ObtenerFacturaPorIdVentaLocal(int idVentaLocal);

        string ObtenerOrdenDespachoPorIdVentaLocal(int identificacion);
        string CrearFacturaOrdenesDeDespacho(string guid);

        string CrearFacturaFacturas(string guid);
        string EnviarFactura(Factura factura);
        List<Canastilla> GetCanastilla();
        int GenerarFacturaCanastilla(FacturaCanastilla factura);
        object EnviarFacturaSiges(FacturaSiges factura);

        string getToken();
        IEnumerable<string> GetGuidsFacturasPendientes(Guid estacion, string token);
        bool EnviarTerceros(IEnumerable<Tercero> terceros, string token);
        bool EnviarFacturas(IEnumerable<FacturaSiges> facturas, IEnumerable<FormaPagoSiges> formas, Guid estacion, string token);
        IEnumerable<Tercero> RecibirTercerosActualizados(Guid estacion, string token);
        IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.Factura> RecibirFacturasImprimir(Guid estacion, string token);
        IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho> RecibirOrdenesImprimir(Guid estacion, string token);
        IEnumerable<FactoradorEstacionesModelo.Objetos.Resolucion> GetResolucionEstacion(Guid estacionFuente, string token);
        InfoEstacion getInfoEstacion(Guid estacionFuente, string token);
        bool AgregarFechaReporteFactura(List<FacturaFechaReporte> facturasFechas, Guid estacionFuente, string token);
        string GetInfoFacturaElectronica(int ventaId, Guid estacionFuente, string v);
        IEnumerable<Canastilla> RecibirCanastilla(Guid estacion, string token);
        Resolucion GetResolucionEstacionCanastilla(Guid estacionFuente, string token);
        bool EnviarFacturasCanastilla(IEnumerable<FacturaCanastilla> facturas, Guid estacionFuente, string token);
        int ObtenerParaImprimir(Guid idEstacion, string token);
        ResolucionElectronica GetResolucionElectronica(string token);
        string GetInfoFacturaElectronicaCanastilla(int facturasCanastillaId, Guid estacionFuente, string v);
    }
}
