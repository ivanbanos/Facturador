using AutoMapper;
using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Estacion
{
    public class EstacionNegocio : IEstacionNegocio
    {
        private readonly IEstacionesRepository _estacionesRepository;
        private readonly ICombustiblesEstacionRepository _combustiblesEstacionRepository;
        private readonly IMapper _mapper;

        public EstacionNegocio(IEstacionesRepository estacionesRepository,
                                ICombustiblesEstacionRepository combustiblesEstacionRepository,
                                IMapper mapper)
        {
            _estacionesRepository = estacionesRepository;
            _combustiblesEstacionRepository = combustiblesEstacionRepository;
            _mapper = mapper;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Modelo.Estacion>> GetEstaciones()
        {
            try
            {
                var estaciones = await _estacionesRepository.GetEstaciones();
                return _mapper.Map<IEnumerable<Repositorio.Entities.Estacion>, IEnumerable<Modelo.Estacion>>(estaciones);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Modelo.Estacion> GetEstacion(Guid estacionGuid)
        {
            try
            {
                var estaciones = await _estacionesRepository.GetEstacion(estacionGuid);
                return _mapper.Map<Repositorio.Entities.Estacion, Modelo.Estacion>(estaciones.FirstOrDefault());
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc />
        public Task AddEstacion(IEnumerable<Modelo.Estacion> estacions)
        {
            try
            {
                return _estacionesRepository.AddRange(_mapper.Map<IEnumerable<Modelo.Estacion>, IEnumerable<Repositorio.Entities.Estacion>>(estacions));
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc />
        public Task<int> BorrarEstacion(IEnumerable<FacturasEntity> estaciones)
        {
            try
            {
                var estacionesList = _mapper.Map<IEnumerable<FacturasEntity>, IEnumerable<Repositorio.Entities.FacturasEntity>>(estaciones);
                return _estacionesRepository.BorrarEstacion(estacionesList);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Modelo.CombustibleEstacion>> GetCombustiblesEstacion(Guid estacionGuid)
        {
            var combustibles = await _combustiblesEstacionRepository.GetCombustiblesEstacion(estacionGuid).ConfigureAwait(false);
            return _mapper.Map<IEnumerable<Repositorio.Entities.CombustibleEstacion>, IEnumerable<Modelo.CombustibleEstacion>>(combustibles);
        }

        public Task UpsertCombustibleEstacion(Guid estacionGuid, Modelo.CombustibleEstacion combustible)
        {
            if (combustible == null || string.IsNullOrWhiteSpace(combustible.Combustible))
            {
                throw new ArgumentException("Combustible invalido");
            }

            return _combustiblesEstacionRepository.UpsertCombustibleEstacion(
                estacionGuid,
                combustible.Combustible,
                combustible.Precio,
                combustible.EsGas);
        }
    }
}
