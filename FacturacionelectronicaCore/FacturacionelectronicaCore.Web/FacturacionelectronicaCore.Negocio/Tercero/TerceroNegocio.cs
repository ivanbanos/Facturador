using AutoMapper;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using FacturacionelectronicaCore.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Tercero
{
    public class TerceroNegocio : ITerceroNegocio
    {
        private readonly ITerceroRepositorio _terceroRepositorio;
        private readonly IMapper _mapper;
        private readonly IFacturacionElectronicaFacade _alegraFacade;
        private readonly Alegra _alegra;

        public TerceroNegocio(ITerceroRepositorio terceroRepositorio, IMapper mapper, IFacturacionElectronicaFacade alegraFacade, IOptions<Alegra> alegra)
        {
            _terceroRepositorio = terceroRepositorio;
            _mapper = mapper;
            _alegraFacade = alegraFacade;
            _alegra = alegra.Value;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Modelo.Tercero>> GetTerceros()
        {
            try
            {

                await SincronizarTerceros();
                var terceros = await _terceroRepositorio.GetTerceros();
                return _mapper.Map<IEnumerable<Repositorio.Entities.Tercero>, IEnumerable<Modelo.Tercero>>(terceros);
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <inheritdoc />
        public async Task<int> AddOrUpdate(IEnumerable<Modelo.Tercero> terceros)
        {
            int cantidadNoPasados = 0;
            try
            {
                var tercerosRepositorio = _mapper.Map<IEnumerable<Modelo.Tercero>, IEnumerable<Repositorio.Entities.TerceroInput>>(terceros);

                if (_alegra.UsaAlegra && _alegra.ValidaTercero  && tercerosRepositorio.Count() == 1)
                {
                    foreach (var ti in tercerosRepositorio)
                    {
                        try
                        {
                            var t = _mapper.Map<Repositorio.Entities.TerceroInput, Modelo.Tercero>(ti);
                            if (ti.idFacturacion != null)
                            {
                                await _alegraFacade.ActualizarTercero(t, ti.idFacturacion);
                            }
                            else
                            {
                                var idFacturacion = await _alegraFacade.GenerarTercero(t);
                                await _terceroRepositorio.SetIdFacturacion(t.Guid, idFacturacion);
                            }
                        }
                        catch (Exception)
                        {
                            cantidadNoPasados++;
                        }
                    }
                }
                
                 await _terceroRepositorio.AddOrUpdate(tercerosRepositorio);
                return cantidadNoPasados;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Modelo.Tercero> GetTercero(Guid idTercero)
        {
            try
            {
                var terceros = await _terceroRepositorio.GetTerceros();

                var tercero = terceros.ToList().Find(x => x.Guid == idTercero);
                if(tercero.idFacturacion == null)
                {
                    await SincronizarTerceros();
                    terceros = await _terceroRepositorio.GetTerceros();

                    tercero = terceros.ToList().Find(x => x.Guid == idTercero);
                }
                return _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(tercero);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> GetIsTerceroValidoPorIdentificacion(string identificacion)
        {
            var terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(identificacion)).FirstOrDefault();
            var tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
            if (tercero.idFacturacion == null)
            {
                
                    await SincronizarTerceros();
                
                terceroEntity = (await _terceroRepositorio.ObtenerTerceroPorIdentificacion(identificacion)).FirstOrDefault();
                tercero = _mapper.Map<Repositorio.Entities.Tercero, Modelo.Tercero>(terceroEntity);
                if (tercero.idFacturacion == null)
                {
                    return false;
                }
            }
            return true;
        }

        public async Task SincronizarTerceros()
        {
            if (_alegra.UsaAlegra && _alegra.ValidaTercero)
            {
                try
                {
                    int start = 0;
                    while (true)
                    {
                        var terceros = await _alegraFacade.GetTerceros(start);
                        if (!terceros.Any())
                        {
                            break;
                        }
                        await _terceroRepositorio.AddOrUpdate(terceros.ConvertToTerceros());
                        start += 30;
                    }
                }
                catch (Exception) { }
            }
            
        }
    }
}
