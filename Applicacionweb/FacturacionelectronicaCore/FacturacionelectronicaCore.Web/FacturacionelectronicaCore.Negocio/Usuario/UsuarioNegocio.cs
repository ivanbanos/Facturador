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
        public async Task<bool> CrearUsuario(Modelo.Usuario usuario)
        {
            var entidad = _mapper.Map<Modelo.Usuario, Repositorio.Entities.Usuario>(usuario);
            if (entidad.guid == Guid.Empty)
            {
                entidad.guid = Guid.NewGuid();
            }
            return await _usuarioRespositorio.CrearUsuario(entidad) > 0;
        }

        /// <inheritdoc />
        public async Task<bool> CambiarContrasena(Guid guid, string contrasenaActual, string contrasenaNueva)
            => await _usuarioRespositorio.CambiarContrasena(guid, contrasenaActual, contrasenaNueva) > 0;

        /// <inheritdoc />
        public async Task<ResultadoActualizacionUsuario> ActualizarUsuario(Modelo.Usuario usuario)
        {
            var entidad = _mapper.Map<Modelo.Usuario, Repositorio.Entities.Usuario>(usuario);
            var resultado = await _usuarioRespositorio.ActualizarUsuario(entidad);
            if (resultado > 0)
            {
                return ResultadoActualizacionUsuario.Actualizado;
            }
            return resultado < 0 ? ResultadoActualizacionUsuario.UsuarioDuplicado : ResultadoActualizacionUsuario.NoExiste;
        }

        /// <inheritdoc />
        public async Task<bool> CambiarEstado(Guid guid, bool activo)
            => await _usuarioRespositorio.CambiarEstado(guid, activo) > 0;

        /// <inheritdoc />
        public async Task<bool> EliminarUsuario(Guid guid)
            => await _usuarioRespositorio.EliminarUsuario(guid) > 0;

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
