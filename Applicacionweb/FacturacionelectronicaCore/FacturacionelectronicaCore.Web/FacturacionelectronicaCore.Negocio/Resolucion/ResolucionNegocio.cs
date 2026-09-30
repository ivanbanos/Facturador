using AutoMapper;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Resolucion
{
    public class ResolucionNegocio : IResolucionNegocio
    {
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private readonly IMapper _mapper;
        public ResolucionNegocio(IResolucionRepositorio resolucionRepositorio,
                                       IMapper mapper)
        {
            _resolucionRepositorio = resolucionRepositorio;
            _mapper = mapper;
        }
        public async Task<Modelo.CreacionResolucion> AddNuevaResolucion(Modelo.CreacionResolucion resolucion)
        {
            try
            {
                var resolucionEntity = _mapper.Map<Modelo.CreacionResolucion, Repositorio.Entities.CreacionResolucion>(resolucion);
                return _mapper.Map< Repositorio.Entities.CreacionResolucion, Modelo.CreacionResolucion>(await _resolucionRepositorio.AddNuevaResolucion(resolucionEntity));
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AnularResolucion(Guid resolucion)
        {
            try
            {
                await _resolucionRepositorio.AnularResolucion(resolucion);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Modelo.Resolucion>> GetResolucionActiva(Guid estacion)
        {
            try
            {
                
                var resolucionEntity = await _resolucionRepositorio.GetResolucionActiva(estacion);
                if(resolucionEntity == null)
                {
                    return null;
                }
                return _mapper.Map< IEnumerable<Repositorio.Entities.Resolucion>, IEnumerable<Modelo.Resolucion>>(resolucionEntity);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Modelo.Resolucion> HabilitarResolucion(Guid resolucion, DateTime fechaVencimiento)
        {
            try
            {

                var resolucionEntity = await _resolucionRepositorio.HabilitarResolucion(resolucion, fechaVencimiento);
                if (resolucionEntity == null)
                {
                    return null;
                }
                return _mapper.Map<Repositorio.Entities.Resolucion, Modelo.Resolucion>(resolucionEntity);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
