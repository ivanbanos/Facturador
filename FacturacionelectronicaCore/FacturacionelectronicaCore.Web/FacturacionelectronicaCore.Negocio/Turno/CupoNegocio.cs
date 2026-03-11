using AutoMapper;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Turno
{
    public interface ICupoNegocio
    {
        Task Add(Modelo.CuposRequest request); 
        Task<IEnumerable<Modelo.CupoAutomotor>> GetCupoAutomotor(string EstacionGuid);
        Task<IEnumerable<Modelo.CupoCliente>> GetCupoCliente(string EstacionGuid);
    }
    public class CupoNegocio : ICupoNegocio
    {
        private readonly ICupoRepositorio _cupoRepositorio;
        private readonly IMapper _mapper;

        public CupoNegocio(ICupoRepositorio cupoRepositorio, IMapper mapper)
        {
            _cupoRepositorio = cupoRepositorio;
            _mapper = mapper;
        }
        public async Task Add(Modelo.CuposRequest request)
        {
            foreach(var cupo in request.cuposAutomotores)
            {

                await _cupoRepositorio.AddAutomoto(_mapper.Map<Repositorio.Entities.CupoAutomotor>(cupo), request.EstacionGuid);
            }
            foreach (var cupo in request.cuposClientes)
            {

                await _cupoRepositorio.AddCleinte(_mapper.Map<Repositorio.Entities.CupoCliente>(cupo), request.EstacionGuid);
            }
        }
        public async Task<IEnumerable<Modelo.CupoAutomotor>> GetCupoAutomotor(string EstacionGuid)
        {
            return _mapper.Map<IEnumerable<Modelo.CupoAutomotor>>(await _cupoRepositorio.GetCupoAutomotor(EstacionGuid));
        }
        public async Task<IEnumerable<Modelo.CupoCliente>> GetCupoCliente(string EstacionGuid)
        {
            return _mapper.Map<IEnumerable<Modelo.CupoCliente>>(await _cupoRepositorio.GetCupoCliente(EstacionGuid));
        }
    }
}
