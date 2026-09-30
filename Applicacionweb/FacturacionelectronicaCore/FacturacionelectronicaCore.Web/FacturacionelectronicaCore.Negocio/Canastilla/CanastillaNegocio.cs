using AutoMapper;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Canastilla
{
    public class CanastillaNegocio : ICanastillaNegocio
    {
        private readonly ICanastillaRepositorio _canastillaRepositorio;
        private readonly IMapper _mapper;

        public CanastillaNegocio(ICanastillaRepositorio canastillaRepositorio, IMapper mapper)
        {
            _canastillaRepositorio = canastillaRepositorio ?? throw new System.ArgumentNullException(nameof(canastillaRepositorio));
            _mapper = mapper ?? throw new System.ArgumentNullException(nameof(mapper));
        }

        public async Task<int> AddOrUpdate(IEnumerable<Modelo.Canastilla> canastillas)
        {
            try
            {
                var tercerosRepositorio = _mapper.Map<IEnumerable<Modelo.Canastilla>, IEnumerable<Repositorio.Entities.Canastilla>>(canastillas);

                await _canastillaRepositorio.AddRange(tercerosRepositorio);
                return 0;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Modelo.Canastilla> GetCanastilla(Guid guid)
        {
            try
            {
                var terceros = await _canastillaRepositorio.GetCanastillas();
                var tercerosNegocio = _mapper.Map<IEnumerable<Repositorio.Entities.Canastilla>, IEnumerable<Modelo.Canastilla>>(terceros);
                return tercerosNegocio.FirstOrDefault(x=>x.guid== guid);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Modelo.Canastilla>> GetCanastillas(Guid? estacion = null)
        {
            try
            {
                var terceros = await _canastillaRepositorio.GetCanastillas(estacion);
                return _mapper.Map<IEnumerable<Repositorio.Entities.Canastilla>, IEnumerable<Modelo.Canastilla>>(terceros);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
