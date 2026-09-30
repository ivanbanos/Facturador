using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Mongodb;
using FacturacionelectronicaCore.Repositorio.Recursos;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class FacturaCanastillaRepository : IFacturaCanastillaRepository
    {
        private readonly ISQLHelper _sqlHelper;
        private readonly IMongoHelper _mongoHelper;
        private readonly RepositorioConfig _repositorioConfig;

        public FacturaCanastillaRepository(IOptions<RepositorioConfig> repositorioConfig, ISQLHelper sqlHelper, IMongoHelper mongoHelper)
        {
            _sqlHelper = sqlHelper;
            _mongoHelper = mongoHelper;
            _repositorioConfig = repositorioConfig.Value;
        }
        public async Task<int> Add(FacturaCanastilla factura, IEnumerable<CanastillaFactura> detalleFactura, Guid estacion)
        {
            factura.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            factura.IdEstacion = estacion.ToString();
            var filter = Builders<FacturaCanastilla>.Filter.Eq("FacturasCanastillaId", factura.FacturasCanastillaId);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaCanastilla>(_repositorioConfig.Cliente, "facturasCanastillas", filter);
            var facturaExistente = facturasMongo.FirstOrDefault(x => x.IdEstacion.ToLower() == estacion.ToString().ToLower());
            if (facturaExistente == null)
            {
                factura.Guid = Guid.NewGuid().ToString();
                await _mongoHelper.CreateDocument(_repositorioConfig.Cliente, "facturasCanastillas", factura);
                return 0;
            }

            if (string.IsNullOrEmpty(factura.idFacturaElectronica))
            {
                factura.idFacturaElectronica = facturaExistente.idFacturaElectronica;
            }

            factura.Id = facturaExistente.Id;
            factura.Guid = string.IsNullOrEmpty(factura.Guid) ? facturaExistente.Guid : factura.Guid;
            factura.IdEstacion = facturaExistente.IdEstacion;

            var filterId = Builders<FacturaCanastilla>.Filter.Eq("_id", new ObjectId(facturaExistente.Id));
            await _mongoHelper.ReplaceDocument(_repositorioConfig.Cliente, "facturasCanastillas", filterId, factura);
            return 0;
        }

        public async Task<bool> FacturaGenerada(int facturasCanastillaId, Guid estacion)
        {
            var filter = Builders<FacturaCanastilla>.Filter.Eq("FacturasCanastillaId", facturasCanastillaId);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaCanastilla>(_repositorioConfig.Cliente, "facturasCanastillas", filter);
            return facturasMongo.Any(x => x.IdEstacion == estacion.ToString()) && !facturasMongo.First(x => x.IdEstacion == estacion.ToString()).idFacturaElectronica.StartsWith("error");
        }

        public Task<IEnumerable<FacturaCanastillaDetalleResponse>> GetDetalleFactura(string idFactura)
        {
            var paramList = new DynamicParameters();
            paramList.Add("guid", idFactura);

            return _sqlHelper.GetsAsync<FacturaCanastillaDetalleResponse>(StoredProcedures.getFacturaCanatillaDetalle, paramList);
        }

        public async Task<IEnumerable<FacturaCanastilla>> GetFactura(string idFactura)
        {
            var id = new ObjectId(idFactura);
            var filter = Builders<FacturaCanastilla>.Filter.Eq("_id", id);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaCanastilla>(_repositorioConfig.Cliente, "facturasCanastillas", filter);
            if (facturasMongo.Any())
            {
                return facturasMongo;
            }
            return null;
        }

        public async Task<FacturaCanastilla> GetFacturaPorIdCanastilla(int consecutivo, Guid estacion)
        {
            var filter = Builders<FacturaCanastilla>.Filter.Eq("FacturasCanastillaId", consecutivo);
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaCanastilla>(_repositorioConfig.Cliente, "facturasCanastillas", filter);
            return facturasMongo.FirstOrDefault(x => x.IdEstacion.ToLower() == estacion.ToString().ToLower());
        }

        public async Task<IEnumerable<FacturaCanastilla>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion)
        {
            List<FilterDefinition<FacturaCanastilla>> filters = new List<FilterDefinition<FacturaCanastilla>>();

            // Always filter by station first
            filters.Add(Builders<FacturaCanastilla>.Filter.Eq("IdEstacion", estacion.ToString()));

            if (fechaInicial != null)
            {
                filters.Add(Builders<FacturaCanastilla>.Filter.Gte("fecha", fechaInicial.Value.Date));
            }
            if (fechaFinal != null)
            {
                filters.Add(Builders<FacturaCanastilla>.Filter.Lte("fecha", fechaFinal.Value.Date.AddDays(1)));
            }
            if (!string.IsNullOrEmpty(identificacionTercero))
            {
                filters.Add(Builders<FacturaCanastilla>.Filter.Eq("terceroId.Identificacion", identificacionTercero));
            }
            if (!string.IsNullOrEmpty(nombreTercero))
            {
                filters.Add(Builders<FacturaCanastilla>.Filter.Eq("terceroId.Nombre", nombreTercero));
            }

            // If no filters were added except station filter, get all documents for the station
            if (filters.Count == 1)
            {
                var facturasMongo = await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, "facturasCanastillas", filters);
                return facturasMongo;
            }
            else
            {
                var facturasMongo = await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, "facturasCanastillas", filters);
                return facturasMongo;
            }
        }
        public async Task SetIdFacturaElectronicaFactura(string idFacturaElectronica, string guid)
        {
            var filter = Builders<FacturaCanastilla>.Filter.Eq("_id", guid.ToString());
            var facturasMongo = await _mongoHelper.GetFilteredDocuments<FacturaCanastilla>(_repositorioConfig.Cliente, "facturasCanastillas", filter);
            if (facturasMongo.Any())
            {
                var facturaMongo = facturasMongo.First();
                var filterGuid = Builders<FacturaCanastilla>.Filter.Eq("_id", facturaMongo.Guid);
                var update = Builders<FacturaCanastilla>.Update
                    .Set(x => x.idFacturaElectronica, idFacturaElectronica)
                    .Set(x => x.estado, "Anulada");
                await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "facturasCanastillas", filterGuid, update);

            }
            //var paramList = new DynamicParameters();
            //paramList.Add("idFacturaElectronica", idFacturaElectronica);
            //paramList.Add("guid", guid);
            //await _sqlHelper.GetsAsync<int>(StoredProcedures.SetIdFacturaElectronicaFactura, paramList);
        }
    }
}
