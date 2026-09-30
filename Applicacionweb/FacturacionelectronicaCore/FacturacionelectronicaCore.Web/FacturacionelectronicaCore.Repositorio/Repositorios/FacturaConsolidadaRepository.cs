using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Mongodb;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class FacturaConsolidadaRepository : IFacturaConsolidadaRepository
    {
        private readonly IMongoHelper _mongoHelper;
        private readonly RepositorioConfig _repositorioConfig;
        private readonly string _databaseName = "FacturacionElectronica";

        public FacturaConsolidadaRepository(IOptions<RepositorioConfig> repositorioConfig, IMongoHelper mongoHelper)
        {
            _repositorioConfig = repositorioConfig.Value;
            _mongoHelper = mongoHelper;
        }

        public async Task<IEnumerable<FacturaConsolidada>> GetFacturasConsolidadas(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null, 
            string identificacionCliente = null, 
            Guid? idEstacion = null,
            string estado = null)
        {
            var filters = new List<FilterDefinition<FacturaConsolidada>>();

            if (fechaDesde.HasValue)
                filters.Add(Builders<FacturaConsolidada>.Filter.Gte(x => x.FechaCreacion, fechaDesde.Value));

            if (fechaHasta.HasValue)
                filters.Add(Builders<FacturaConsolidada>.Filter.Lte(x => x.FechaCreacion, fechaHasta.Value));

            if (!string.IsNullOrEmpty(identificacionCliente))
                filters.Add(Builders<FacturaConsolidada>.Filter.Eq(x => x.IdentificacionCliente, identificacionCliente));

            if (idEstacion.HasValue)
                filters.Add(Builders<FacturaConsolidada>.Filter.Eq(x => x.IdEstacion, idEstacion.Value.ToString()));

            if (!string.IsNullOrEmpty(estado))
                filters.Add(Builders<FacturaConsolidada>.Filter.Eq(x => x.Estado, estado));

            if (filters.Any())
            {
                return await _mongoHelper.GetFilteredDocuments<FacturaConsolidada>(_databaseName, "FacturasConsolidadas", filters);
            }
            else
            {
                return await _mongoHelper.GetAllDocuments<FacturaConsolidada>(_databaseName, "FacturasConsolidadas");
            }
        }

        public async Task<FacturaConsolidada> GetFacturaConsolidada(string id)
        {
            var filter = Builders<FacturaConsolidada>.Filter.Eq(x => x.Id, id);
            var result = await _mongoHelper.GetFilteredDocuments<FacturaConsolidada>(_databaseName, "FacturasConsolidadas", filter);
            return result.FirstOrDefault();
        }

        public async Task<FacturaConsolidada> GetFacturaConsolidadaPorGuid(string guid)
        {
            var filter = Builders<FacturaConsolidada>.Filter.Eq(x => x.Guid, guid);
            var result = await _mongoHelper.GetFilteredDocuments<FacturaConsolidada>(_databaseName, "FacturasConsolidadas", filter);
            return result.FirstOrDefault();
        }

        public async Task<FacturaConsolidada> CrearFacturaConsolidada(FacturaConsolidada factura)
        {
            factura.FechaCreacion = DateTime.Now;
            factura.FechaActualizacion = DateTime.Now;
            
            await _mongoHelper.CreateDocument<FacturaConsolidada>(_databaseName, "FacturasConsolidadas", factura);
            return factura;
        }

        public async Task<FacturaConsolidada> ActualizarFacturaConsolidada(FacturaConsolidada factura)
        {
            factura.FechaActualizacion = DateTime.Now;
            
            var filter = Builders<FacturaConsolidada>.Filter.Eq(x => x.Id, factura.Id);
            var update = Builders<FacturaConsolidada>.Update
                .Set(x => x.Estado, factura.Estado)
                .Set(x => x.IdFacturaElectronica, factura.IdFacturaElectronica)
                .Set(x => x.InfoFacturacionElectronica, factura.InfoFacturacionElectronica)
                .Set(x => x.FechaActualizacion, factura.FechaActualizacion)
                .Set(x => x.Observaciones, factura.Observaciones);

            await _mongoHelper.UpdateDocument<FacturaConsolidada>(_databaseName, "FacturasConsolidadas", filter, update);
            return factura;
        }

        public async Task<int> ObtenerSiguienteConsecutivo(Guid idEstacion)
        {
            // Para simplificar, voy a usar un enfoque básico de obtener el máximo consecutivo + 1
            var facturas = await _mongoHelper.GetAllDocuments<FacturaConsolidada>(_databaseName, "FacturasConsolidadas");
            var facturasEstacion = facturas.Where(x => x.IdEstacion == idEstacion.ToString());
            
            if (facturasEstacion.Any())
            {
                return facturasEstacion.Max(x => x.Consecutivo) + 1;
            }
            return 1;
        }

        public async Task<bool> OrdenYaConsolidada(string guidOrden)
        {
            var facturas = await _mongoHelper.GetAllDocuments<FacturaConsolidada>(_databaseName, "FacturasConsolidadas");
            return facturas.Any(x => x.OrdenesConsolidadas.Contains(guidOrden));
        }

        public async Task<IEnumerable<FacturaConsolidada>> GetFacturasConsolidadasPorPeriodo(
            DateTime fechaDesde, 
            DateTime fechaHasta, 
            Guid idEstacion)
        {
            var filters = new List<FilterDefinition<FacturaConsolidada>>
            {
                Builders<FacturaConsolidada>.Filter.Gte(x => x.FechaCreacion, fechaDesde),
                Builders<FacturaConsolidada>.Filter.Lte(x => x.FechaCreacion, fechaHasta),
                Builders<FacturaConsolidada>.Filter.Eq(x => x.IdEstacion, idEstacion.ToString())
            };

            return await _mongoHelper.GetFilteredDocuments<FacturaConsolidada>(_databaseName, "FacturasConsolidadas", filters);
        }
    }
}