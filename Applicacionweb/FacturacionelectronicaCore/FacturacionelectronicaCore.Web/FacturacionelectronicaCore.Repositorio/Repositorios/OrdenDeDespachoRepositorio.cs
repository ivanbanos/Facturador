using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesSevicio.Respositorio.Extention;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Mongodb;
using FacturacionelectronicaCore.Repositorio.Recursos;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class OrdenDeDespachoRepositorio : IOrdenDeDespachoRepositorio
    {
        private const string Coleccion = "ordenes";

        private readonly ISQLHelper _sqlHelper;
        private readonly RepositorioConfig _repositorioConfig;
        private readonly IMongoHelper _mongoHelper;

        public OrdenDeDespachoRepositorio(ISQLHelper sqlHelper, IOptions<RepositorioConfig> repositorioConfig, IMongoHelper mongoHelper)
        {
            _sqlHelper = sqlHelper;
            _repositorioConfig = repositorioConfig.Value;
            _mongoHelper = mongoHelper;
        }

        /// <summary>
        /// Filtro por estación tolerante a mayúsculas/minúsculas del Guid, pero resuelto en Mongo
        /// (usa índice) en lugar de traer todas las estaciones y filtrar en memoria.
        /// </summary>
        private static FilterDefinition<OrdenesMongo> FiltroEstacion(Guid estacion)
        {
            var guid = estacion.ToString();
            return Builders<OrdenesMongo>.Filter.In("EstacionGuid", new[] { guid.ToLowerInvariant(), guid.ToUpperInvariant() });
        }

        private static UpdateDefinition<OrdenesMongo> ConstruirActualizacion(string idFacturaElectronicaActual, OrdenDeDespacho factura)
        {
            var facturaYaGenerada = !string.IsNullOrEmpty(idFacturaElectronicaActual)
                && !idFacturaElectronicaActual.StartsWith("error", StringComparison.OrdinalIgnoreCase);

            if (facturaYaGenerada)
            {
                // La factura electronica ya fue generada: solo se actualizan los datos de forma de pago,
                // el resto de la orden queda tal como se genero la factura.
                return Builders<OrdenesMongo>.Update
                    .Set(x => x.FormaDePago, factura.FormaDePago)
                    .Set(x => x.FormaDePago2, factura.FormaDePago2)
                    .Set(x => x.Total1, factura.Total1)
                    .Set(x => x.Total2, factura.Total2)
                    .Set(x => x.ConsecutivoLocal, factura.ConsecutivoLocal);
            }

            return Builders<OrdenesMongo>.Update
                .Set(x => x.Identificacion, factura.Identificacion)
                .Set(x => x.NombreTercero, factura.NombreTercero)
                .Set(x => x.Placa, factura.Placa)
                .Set(x => x.idFacturaElectronica, factura.idFacturaElectronica)
                .Set(x => x.TurnoGuid, factura.TurnoGuid)
                .Set(x => x.FormaDePago, factura.FormaDePago)
                .Set(x => x.FormaDePago2, factura.FormaDePago2)
                .Set(x => x.Total1, factura.Total1)
                .Set(x => x.Total2, factura.Total2)
                .Set(x => x.Kilometraje, factura.Kilometraje)
                .Set(x => x.NumeroTransaccion, factura.NumeroTransaccion)
                .Set(x => x.ConsecutivoLocal, factura.ConsecutivoLocal)
                // Sin factura electronica aun: los montos pueden cambiar (p. ej. descuento del cliente).
                .Set(x => x.Descuento, factura.Descuento)
                .Set(x => x.SubTotal, factura.SubTotal)
                .Set(x => x.Total, factura.Total);
        }

        /// <summary>
        /// Trae en una sola consulta las órdenes existentes de la estación para los IdVentaLocal dados.
        /// </summary>
        private async Task<List<OrdenesMongo>> BuscarPorIdsVentaLocal(IEnumerable<int> idsVentaLocal, Guid estacion)
        {
            var filtro = Builders<OrdenesMongo>.Filter.And(
                Builders<OrdenesMongo>.Filter.In("IdVentaLocal", idsVentaLocal.Distinct()),
                Builders<OrdenesMongo>.Filter.Eq("EstacionGuid", estacion.ToString()));
            return await _mongoHelper.GetFilteredDocuments<OrdenesMongo>(_repositorioConfig.Cliente, Coleccion, filtro);
        }

        public async Task AddRange(IEnumerable<OrdenDeDespacho> lists, Guid estacion)
        {
            var ordenes = lists.ToList();
            if (!ordenes.Any())
            {
                return;
            }

            var existentes = (await BuscarPorIdsVentaLocal(ordenes.Select(x => x.IdVentaLocal), estacion))
                .GroupBy(x => x.IdVentaLocal)
                .ToDictionary(g => g.Key, g => (Guid: g.First().guid, IdFacturaElectronica: g.First().idFacturaElectronica));

            var operaciones = new List<WriteModel<OrdenesMongo>>();
            foreach (var factura in ordenes)
            {
                if (existentes.TryGetValue(factura.IdVentaLocal, out var existente))
                {
                    var filterGuid = Builders<OrdenesMongo>.Filter.Eq("_id", existente.Guid);
                    operaciones.Add(new UpdateOneModel<OrdenesMongo>(filterGuid, ConstruirActualizacion(existente.IdFacturaElectronica, factura)));

                    var facturaYaGenerada = !string.IsNullOrEmpty(existente.IdFacturaElectronica)
                        && !existente.IdFacturaElectronica.StartsWith("error", StringComparison.OrdinalIgnoreCase);
                    if (!facturaYaGenerada)
                    {
                        // Si la misma venta viene repetida en el lote, la siguiente se evalúa con el estado ya actualizado
                        existentes[factura.IdVentaLocal] = (existente.Guid, factura.idFacturaElectronica);
                    }
                }
                else
                {
                    var facturaMongo = new OrdenesMongo(factura);
                    facturaMongo.guid = Guid.NewGuid().ToString();
                    facturaMongo.EstacionGuid = estacion.ToString();
                    facturaMongo.Estado = "Creada";
                    operaciones.Add(new InsertOneModel<OrdenesMongo>(facturaMongo));
                    existentes[factura.IdVentaLocal] = (facturaMongo.guid, facturaMongo.idFacturaElectronica);
                }
            }

            // Ordenado para respetar inserción → actualización cuando una venta se repite en el lote
            await _mongoHelper.BulkWrite(_repositorioConfig.Cliente, Coleccion, operaciones);

            //var dataTable = lists.ToDataTable();
            //await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AgregarOrdenDespacho,
            //    new { ordenes = dataTable.AsTableValuedParameter(UserDefinedTypes.OrdenesDeDespachoType), estacion }).ConfigureAwait(false);
        }

        private static List<FilterDefinition<OrdenesMongo>> FiltrosTercero(string identificacionTercero, string nombreTercero)
        {
            var filters = new List<FilterDefinition<OrdenesMongo>>();
            if (!string.IsNullOrEmpty(identificacionTercero))
            {
                filters.Add(Builders<OrdenesMongo>.Filter.Eq("Identificacion", identificacionTercero));
            }
            if (!string.IsNullOrEmpty(nombreTercero))
            {
                filters.Add(Builders<OrdenesMongo>.Filter.Eq("NombreTercero", nombreTercero));
            }
            return filters;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrdenDeDespacho>> GetOrdenesDeDespacho(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero,
                                                                       string nombreTercero, Guid? estacion)
        {
            // FechaReporte is the source of truth for reporting filters.
            // Keep fallback to Fecha only when FechaReporte is clearly uninitialized.
            var fechaMinimaValida = new DateTime(2000, 1, 1);

            var filters = FiltrosTercero(identificacionTercero, nombreTercero);
            if (estacion != null)
            {
                filters.Add(FiltroEstacion(estacion.Value));
            }

            if (fechaInicial != null || fechaFinal != null)
            {
                // (FechaReporte >= fechaMinimaValida AND FechaReporte en rango) OR (FechaReporte < fechaMinimaValida AND Fecha en rango)
                // Equivale a la combinación anterior, pero con un solo $or para que Mongo use un índice por rama.
                var porFechaReporte = new List<FilterDefinition<OrdenesMongo>> { Builders<OrdenesMongo>.Filter.Gte("FechaReporte", fechaMinimaValida) };
                // Sin estación no hay consulta de respaldo: las órdenes antiguas sin FechaReporte entran por Fecha aquí mismo
                var sinFechaReporte = estacion == null
                    ? Builders<OrdenesMongo>.Filter.Or(
                        Builders<OrdenesMongo>.Filter.Lt("FechaReporte", fechaMinimaValida),
                        Builders<OrdenesMongo>.Filter.Exists("FechaReporte", false))
                    : Builders<OrdenesMongo>.Filter.Lt("FechaReporte", fechaMinimaValida);
                var porFecha = new List<FilterDefinition<OrdenesMongo>> { sinFechaReporte };
                if (fechaInicial != null)
                {
                    porFechaReporte.Add(Builders<OrdenesMongo>.Filter.Gte("FechaReporte", fechaInicial.Value));
                    porFecha.Add(Builders<OrdenesMongo>.Filter.Gte("Fecha", fechaInicial.Value));
                }
                if (fechaFinal != null)
                {
                    porFechaReporte.Add(Builders<OrdenesMongo>.Filter.Lt("FechaReporte", fechaFinal.Value.AddDays(1)));
                    porFecha.Add(Builders<OrdenesMongo>.Filter.Lt("Fecha", fechaFinal.Value.AddDays(1)));
                }
                filters.Add(Builders<OrdenesMongo>.Filter.Or(
                    Builders<OrdenesMongo>.Filter.And(porFechaReporte),
                    Builders<OrdenesMongo>.Filter.And(porFecha)));
            }

            var facturasMongo = (await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, Coleccion, filters)).ToList();
            if (estacion == null || facturasMongo.Any())
            {
                // Más recientes primero según la misma fecha que usa el filtro (FechaReporte, o Fecha si no está inicializada).
                // Se ordena aquí porque el $or mezcla dos campos de fecha y Mongo no puede ordenarlo con un índice.
                return facturasMongo
                    .OrderByDescending(x => x.FechaReporte >= fechaMinimaValida ? x.FechaReporte : x.Fecha)
                    .ToList();
            }

            // Sin resultados: órdenes antiguas sin FechaReporte, filtradas por Fecha
            var filtersFallback = FiltrosTercero(identificacionTercero, nombreTercero);
            filtersFallback.Add(FiltroEstacion(estacion.Value));
            if (fechaInicial != null)
            {
                filtersFallback.Add(Builders<OrdenesMongo>.Filter.Gte("Fecha", fechaInicial.Value));
            }
            if (fechaFinal != null)
            {
                filtersFallback.Add(Builders<OrdenesMongo>.Filter.Lt("Fecha", fechaFinal.Value.AddDays(1)));
            }
            filtersFallback.Add(Builders<OrdenesMongo>.Filter.Or(
                Builders<OrdenesMongo>.Filter.Lt("FechaReporte", DateTime.Now.AddYears(-10)),
                Builders<OrdenesMongo>.Filter.Exists("FechaReporte", false)));

            var ordenFallback = Builders<OrdenesMongo>.Sort.Descending("Fecha");
            return await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, Coleccion, filtersFallback, ordenFallback);
        }

        public Task<int> AddOrdenesImprimir(IEnumerable<FacturasEntity> lists)
        {
            return _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AddOrdenesImprimir,
                new { ordenes = lists.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Entity) });
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrdenDeDespacho>> GetOrdenesImprimir(Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("estacion", estacion);

            var list = await _sqlHelper.GetsAsync<FacturasEntity>(StoredProcedures.GetOrdenesImprimir, paramList);
            var guids = list.Select(x => x.Guid.ToString()).ToList();
            if (!guids.Any())
            {
                return new List<OrdenDeDespacho>();
            }

            // Una sola consulta con $in en lugar de una por orden
            var filter = Builders<OrdenesMongo>.Filter.In("_id", guids.Distinct());
            var ordenesPorGuid = (await _mongoHelper.GetFilteredDocuments<OrdenesMongo>(_repositorioConfig.Cliente, Coleccion, filter))
                .ToDictionary(x => x.guid);

            // Se conserva el orden en que las devuelve SQL
            return guids
                .Where(ordenesPorGuid.ContainsKey)
                .Select(g => (OrdenDeDespacho)ordenesPorGuid[g])
                .ToList();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenDespachoPorGuid(string guid)
        {
            var filter = Builders<OrdenesMongo>.Filter.Eq("_id", guid.ToString());
            return await _mongoHelper.GetFilteredDocuments<OrdenesMongo>(_repositorioConfig.Cliente, Coleccion, filter);
        }

        public async Task<int> AnularOrdenes(IEnumerable<FacturasEntity> ordenes)
        {
            var guids = ordenes.Select(x => x.Guid.ToString()).Distinct().ToList();
            if (guids.Any())
            {
                var filter = Builders<OrdenesMongo>.Filter.In("_id", guids);
                var update = Builders<OrdenesMongo>.Update.Set(x => x.Estado, "Anulada");
                await _mongoHelper.UpdateMany(_repositorioConfig.Cliente, Coleccion, filter, update);
            }
            return 1;
            //return await  _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.AnularOrdenesDeDespacho,
            //    new { Ordenes = ordenes.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Entity) });
        }


        /// <inheritdoc />
        public Task<IEnumerable<OrdenDeDespacho>> GetOrdenesDeDespachoByFactura(string facturaGuid)
        {

            var paramList = new DynamicParameters();
            paramList.Add("facturaGuid", facturaGuid);
            return _sqlHelper.GetsAsync<OrdenDeDespacho>(StoredProcedures.GetOrdenesDeDespachoByFactura, paramList);
        }

        public async Task SetIdFacturaElectronicaOrdenesdeDespacho(string idFacturaElectronica, string guid)
        {
            // UpdateOne por _id: si no existe no hace nada (antes se consultaba primero)
            var filter = Builders<OrdenesMongo>.Filter.Eq("_id", guid.ToString());
            var update = Builders<OrdenesMongo>.Update
                .Set(x => x.idFacturaElectronica, idFacturaElectronica)
                .Set(x => x.Estado, "Anulada");
            await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, Coleccion, filter, update);
        }

        public async Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion)
        {
            List<FilterDefinition<OrdenesMongo>> filters = new List<FilterDefinition<OrdenesMongo>>
            {
                Builders<OrdenesMongo>.Filter.Eq("IdVentaLocal", idVentaLocal),
                Builders<OrdenesMongo>.Filter.Eq("EstacionGuid", estacion.ToString())
            };

            return await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, Coleccion, filters);
        }

        public async Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenesPorTurno(Guid turno)
        {
            List<FilterDefinition<OrdenDeDespacho>> filters = new List<FilterDefinition<OrdenDeDespacho>>();


            filters.Add(Builders<OrdenDeDespacho>.Filter.Eq("TurnoGuid", turno));



            return await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, Coleccion, filters);
        }

        public async Task<bool> AgregarTurnoAOrdenDeDespacho(int idVentaLocal, string turnoGuid, Guid estacion)
        {
            var filtro = Builders<OrdenesMongo>.Filter.And(
                Builders<OrdenesMongo>.Filter.Eq("IdVentaLocal", idVentaLocal),
                Builders<OrdenesMongo>.Filter.Eq("EstacionGuid", estacion.ToString()));

            var update = Builders<OrdenesMongo>.Update.Set(x => x.TurnoGuid, turnoGuid);
            var encontradas = await _mongoHelper.UpdateMany(_repositorioConfig.Cliente, Coleccion, filtro, update);
            return encontradas > 0;
        }

        public async Task AgregarFechaReporteFactura(IEnumerable<FacturaFechaReporte> facturas, Guid estacion)
        {
            var lista = facturas.ToList();
            if (!lista.Any())
            {
                return;
            }

            // Antes: una consulta por factura y sin filtrar la estación (traía las órdenes de todas las estaciones)
            var guidPorVenta = (await BuscarPorIdsVentaLocal(lista.Select(x => x.IdVentaLocal), estacion))
                .GroupBy(x => x.IdVentaLocal)
                .ToDictionary(g => g.Key, g => g.First().guid);

            var operaciones = lista
                .Where(f => guidPorVenta.ContainsKey(f.IdVentaLocal))
                .Select(f => (WriteModel<OrdenesMongo>)new UpdateOneModel<OrdenesMongo>(
                    Builders<OrdenesMongo>.Filter.Eq("_id", guidPorVenta[f.IdVentaLocal]),
                    Builders<OrdenesMongo>.Update.Set(x => x.FechaReporte, f.FechaReporte)))
                .ToList();

            await _mongoHelper.BulkWrite(_repositorioConfig.Cliente, Coleccion, operaciones);
        }
    }
}
