using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Mongodb;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface ICupoRepositorio
    {
        Task AddAutomoto(CupoAutomotor cupoAutomotor, string estacionGuid);

        Task AddCleinte(CupoCliente cupoCliente, string estacionGuid);
        Task<IEnumerable<CupoCliente>> GetCupoCliente(string estacion);
        Task<IEnumerable<CupoAutomotor>> GetCupoAutomotor(string estacion);
    }

    public class CupoRepositorio : ICupoRepositorio
    {
        private readonly IMongoHelper _mongoHelper;
        private readonly RepositorioConfig _repositorioConfig;

        public CupoRepositorio(IMongoHelper mongoHelper, IOptions<RepositorioConfig> repositorioConfig)
        {
            _mongoHelper = mongoHelper;
            _repositorioConfig = repositorioConfig.Value;
        }

        public async Task AddAutomoto(CupoAutomotor cupoAutomotor, string estacionGuid)
        {
            cupoAutomotor.EstacionGuid = estacionGuid;
            var filter = Builders<CupoAutomotor>.Filter.Eq("Placa", cupoAutomotor.Placa);
            var cupos = await _mongoHelper.GetFilteredDocuments<CupoAutomotor>(_repositorioConfig.Cliente, "CupoAutomotor", filter);
            if (!cupos.Any(x => x.EstacionGuid == cupoAutomotor.EstacionGuid.ToString()))
            {
                cupoAutomotor.Id = Guid.NewGuid().ToString();
                await _mongoHelper.CreateDocument(_repositorioConfig.Cliente, "CupoAutomotor", cupoAutomotor);
            }
            else
            {
                var cupo = cupos.First(x => x.EstacionGuid == estacionGuid);
                var filterGuid = Builders<CupoAutomotor>.Filter.Eq("_id", cupo.Id);
                var update = Builders<CupoAutomotor>.Update
                    .Set(x => x.COD_CLI, cupoAutomotor.COD_CLI)
                    .Set(x => x.Nit, cupoAutomotor.Nit)
                    .Set(x => x.CupoAsignado, cupoAutomotor.CupoAsignado)
                    .Set(x => x.CupoDisponible, cupoAutomotor.CupoDisponible);
                await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "CupoAutomotor", filterGuid, update);
            }
        }

        public async Task AddCleinte(CupoCliente cupoCliente, string estacionGuid)
        {
            cupoCliente.EstacionGuid = estacionGuid;
            var filter = Builders<CupoCliente>.Filter.Eq("COD_CLI", cupoCliente.COD_CLI);
            var cupos = await _mongoHelper.GetFilteredDocuments<CupoCliente>(_repositorioConfig.Cliente, "CupoCliente", filter);
            if (!cupos.Any(x => x.EstacionGuid == cupoCliente.EstacionGuid))
            {
                cupoCliente.Id = Guid.NewGuid().ToString();
                await _mongoHelper.CreateDocument(_repositorioConfig.Cliente, "CupoCliente", cupoCliente);
            }
            else
            {
                var cupo = cupos.First(x => x.EstacionGuid == estacionGuid);
                var filterGuid = Builders<CupoCliente>.Filter.Eq("_id", cupo.Id);
                var update = Builders<CupoCliente>.Update
                    .Set(x => x.COD_CLI, cupoCliente.COD_CLI)
                    .Set(x => x.Nit, cupoCliente.Nit)
                    .Set(x => x.CupoAsignado, cupoCliente.CupoAsignado)
                    .Set(x => x.CupoDisponible, cupoCliente.CupoDisponible);
                await _mongoHelper.UpdateDocument(_repositorioConfig.Cliente, "CupoCliente", filterGuid, update);
            }
        }

        public async Task<IEnumerable<CupoAutomotor>> GetCupoAutomotor(string estacion)
        {
            var CupoAutomotor = await _mongoHelper.GetAllDocuments<CupoAutomotor>(_repositorioConfig.Cliente, "CupoAutomotor");
            if (CupoAutomotor.Any(x => x.EstacionGuid.ToLower() == estacion.ToLower()))
            {
                return CupoAutomotor.Where(x => x.EstacionGuid.ToLower() == estacion.ToLower());

            }
            else
            {
                return new List<CupoAutomotor>();
            }
        }
        public async Task<IEnumerable<CupoCliente>> GetCupoCliente(string estacion)
        {
            var CupoAutomotor = await _mongoHelper.GetAllDocuments<CupoCliente>(_repositorioConfig.Cliente, "CupoCliente");
            if (CupoAutomotor.Any(x => x.EstacionGuid == estacion))
            {
                return CupoAutomotor.Where(x => x.EstacionGuid == estacion);
            }
            else
            {
                return new List<CupoCliente>();
            }
        }

    }
}
