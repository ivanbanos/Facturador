using AutoMapper;
using EstacionesServicio.Repositorio.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstacionesServicio.Negocio.Usuario
{
    public class UsuarioNegocio : IUsuarioNegocio
    {
        private readonly IUsuarioRespositorio _usuarioRespositorio;
        private readonly IMapper _mapper;

        public UsuarioNegocio(IUsuarioRespositorio usuarioRespositorio, IMapper mapper)
        {
            _usuarioRespositorio = usuarioRespositorio;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Modelo.Usuario>> GetUsuarios()
        {
            try
            {
                var usuariosEntities = await _usuarioRespositorio.GetUsuarios();
                return _mapper.Map<IEnumerable<Repositorio.Entities.Usuario>, List<Modelo.Usuario>>(usuariosEntities);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<Modelo.Usuario> GetUsuario(string usuario, string contrasena)
        {
            try
            {
                var user = await _usuarioRespositorio.GetUsuario(usuario, contrasena);
                return _mapper.Map<Repositorio.Entities.Usuario, Modelo.Usuario>(user);
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <inheritdoc />
        public async Task<Modelo.Usuario> GetUsuario(Guid guid)
        {
            try
            {
                var user = await _usuarioRespositorio.GetUsuario(guid);
                return _mapper.Map<Repositorio.Entities.Usuario, Modelo.Usuario>(user);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <inheritdoc />
        public Task<int> SaveUsuario(IEnumerable<Modelo.Usuario> usuarios)
        {
            try
            {
                return _usuarioRespositorio.SaveUsuario( usuarios.Select(x => _mapper.Map<Modelo.Usuario, Repositorio.Entities.Usuario>(x)));
            }catch(Exception)
            {
                throw;
            }
        }
    }
}
