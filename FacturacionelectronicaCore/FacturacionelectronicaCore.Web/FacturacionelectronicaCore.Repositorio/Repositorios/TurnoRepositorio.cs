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
    public interface ITurnoRepositorio
    {
        Task Add(Turno turno);
        Task<IEnumerable<Turno>> Get(DateTime fechaInicial, DateTime fechaFinal, string estacion);
        Task<IEnumerable<Turno>> Get(DateTime fecha, int numero, string isla, string estacion);
    }
    public class TurnoRepositorio : ITurnoRepositorio
    {

        private readonly IMongoHelper _mongoHelper;
        private readonly RepositorioConfig _repositorioConfig;

        public TurnoRepositorio(IMongoHelper mongoHelper, IOptions<RepositorioConfig> repositorioConfig)
        {
            _mongoHelper = mongoHelper;
            _repositorioConfig = repositorioConfig.Value;
        }

        public async Task Add(Turno turno)
        {
            var filter = Builders<Turno>.Filter.Eq("FechaApertura", turno.FechaApertura)
                & Builders<Turno>.Filter.Eq("Numero", turno.Numero)
                & Builders<Turno>.Filter.Eq("Isla", turno.Isla)
                & Builders<Turno>.Filter.Eq("EstacionGuid", turno.EstacionGuid);

            var turnos = await _mongoHelper.GetFilteredDocuments<Turno>(_repositorioConfig.Cliente, "Turnos", filter);
            var turnoExistente = turnos.FirstOrDefault();

            if (turnoExistente == null)
            {
                if (string.IsNullOrWhiteSpace(turno.Id))
                {
                    turno.Id = Guid.NewGuid().ToString();
                }

                await _mongoHelper.CreateDocument(_repositorioConfig.Cliente, "Turnos", turno);
                return;
            }

            turno.Id = turnoExistente.Id;
            var filterById = Builders<Turno>.Filter.Eq("Id", turnoExistente.Id);
            await _mongoHelper.ReplaceDocument(_repositorioConfig.Cliente, "Turnos", filterById, turno);
        }

        public async Task<IEnumerable<Turno>> Get(DateTime fechaInicial, DateTime fechaFinal, string estacion)
        {
            var filter = Builders<Turno>.Filter.Gte("FechaApertura", fechaInicial)
                & Builders<Turno>.Filter.Lte("FechaApertura", fechaFinal.AddDays(1).AddHours(1));
            var turnos = await _mongoHelper.GetFilteredDocuments<Turno>(_repositorioConfig.Cliente, "Turnos", filter);
            return turnos.Where(x => x.EstacionGuid == estacion);
        }

        public async Task<IEnumerable<Turno>> Get(DateTime fecha, int numero, string isla, string estacion)
        {
            var filter = Builders<Turno>.Filter.Eq("FechaApertura", fecha)
                & Builders<Turno>.Filter.Eq("Numero", numero)
               & Builders<Turno>.Filter.Eq("Isla", isla);
            var turnos = await _mongoHelper.GetFilteredDocuments(_repositorioConfig.Cliente, "Turnos", filter);
            return turnos.Where(x => x.EstacionGuid == estacion);
        }
    }
}
