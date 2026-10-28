using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EstacionesServicio.Modelo;
using EstacionesServicio.Negocio.Usuario;
using FacturacionelectronicaCore.Negocio.Tercero;
using FacturacionelectronicaCore.Web.Authtentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioNegocio _usuarioNegocio;
        private readonly ITerceroNegocio _terceroNegocio;
        private readonly IAuthentication _authentication;

        public UsuariosController(IUsuarioNegocio usuarioNegocio, ITerceroNegocio terceroNegocio, IAuthentication authentication)
        {
            _usuarioNegocio = usuarioNegocio;
            _terceroNegocio = terceroNegocio;
            _authentication = authentication;
        }

        /// <summary>
        /// Obtiene un identificador unico de usuario buscando por contrasena y usuario
        /// </summary>
        /// <param name="nombre">Usuario</param>
        /// <param name="contrasena">Contrasena</param>
        /// <returns>AIdentificador unico del usuario</returns>
        [HttpGet("{nombre}/{contrasena}")]
        [AllowAnonymous]
        public Task<ActionResult<string>> Login(string nombre, string contrasena)
            => GenerarToken(nombre, contrasena);

        /// <summary>
        /// Inicio de sesion enviando las credenciales en el cuerpo
        /// (la ruta GET no admite contrasenas con '/').
        /// </summary>
        [HttpPost("Login")]
        [AllowAnonymous]
        public Task<ActionResult<string>> IniciarSesion([FromBody] LoginRequest request)
            => GenerarToken(request.Usuario, request.Contrasena);

        private async Task<ActionResult<string>> GenerarToken(string nombre, string contrasena)
        {
            if (String.IsNullOrEmpty(nombre) || String.IsNullOrEmpty(contrasena))
            {
                return BadRequest(new { message = "request del cliente invalido" });
            }
            Usuario usuario = await _usuarioNegocio.GetUsuario(nombre, contrasena);
            if (usuario == null)
            {
                return Unauthorized(new { message = "nombre de usuario o contraseña incorrectos" });
            }
            return Ok(new { token = _authentication.GenerateToken(usuario) });
        }

        /// <summary>
        /// Lista los usuarios (sin contrasenas)
        /// </summary>
        [Authorize(Roles = UsuarioClaims.RolAdministrador)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioResponse>>> Listar()
        {
            var usuarios = await _usuarioNegocio.GetUsuarios();
            return Ok(usuarios.Select(ToResponse));
        }

        /// <summary>
        /// Informacion del usuario autenticado
        /// </summary>
        [HttpGet("Me")]
        public async Task<ActionResult<UsuarioResponse>> Me()
        {
            var guid = User.GetUsuarioGuid();
            if (guid == null)
            {
                return Unauthorized();
            }
            var usuario = await _usuarioNegocio.GetUsuario(guid.Value);
            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(ToResponse(usuario));
        }

        /// <summary>
        /// obtiene el usuario buscando por su identificador unico
        /// </summary>
        /// <param name="guid">Identificador unico</param>
        /// <returns>Usuario</returns>
        [Authorize(Roles = UsuarioClaims.RolAdministrador)]
        [HttpGet("{guid:guid}")]
        public async Task<ActionResult<UsuarioResponse>> Get(Guid guid)
        {
            var usuario = await _usuarioNegocio.GetUsuario(guid);
            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(ToResponse(usuario));
        }

        /// <summary>
        /// Crea un usuario. Si trae identificacion de tercero queda asociado a ese cliente;
        /// si no, es Administrador u Operador segun el rol enviado. Solo administradores.
        /// </summary>
        [Authorize(Roles = UsuarioClaims.RolAdministrador)]
        [HttpPost("Crear")]
        public async Task<ActionResult<UsuarioResponse>> Crear([FromBody] CrearUsuarioRequest request)
        {
            var identificacion = string.IsNullOrWhiteSpace(request.IdentificacionTercero)
                ? null
                : request.IdentificacionTercero.Trim();

            if (identificacion != null)
            {
                var terceros = await _terceroNegocio.BuscarPorIdentificacion(identificacion);
                if (terceros == null || !terceros.Any())
                {
                    return BadRequest(new { message = $"No existe un cliente con identificación {identificacion}" });
                }
            }

            var usuario = new Usuario
            {
                guid = Guid.NewGuid(),
                Nombre = request.Nombre.Trim(),
                NombreUsuario = request.Usuario.Trim(),
                Contrasena = request.Contrasena,
                IdentificacionTercero = identificacion,
                Rol = RolAGuardar(identificacion, request.Rol)
            };

            var creado = await _usuarioNegocio.CrearUsuario(usuario);
            if (!creado)
            {
                return Conflict(new { message = $"El usuario '{usuario.NombreUsuario}' ya existe" });
            }
            return Ok(ToResponse(usuario));
        }

        /// <summary>
        /// Actualiza nombre, usuario, cliente asociado y (opcional) la contrasena de un usuario.
        /// </summary>
        [Authorize(Roles = UsuarioClaims.RolAdministrador)]
        [HttpPut("{guid:guid}")]
        public async Task<ActionResult<UsuarioResponse>> Actualizar(Guid guid, [FromBody] ActualizarUsuarioRequest request)
        {
            var identificacion = string.IsNullOrWhiteSpace(request.IdentificacionTercero)
                ? null
                : request.IdentificacionTercero.Trim();

            if (identificacion != null)
            {
                if (EsUsuarioAutenticado(guid))
                {
                    return BadRequest(new { message = "No puede convertir su propio usuario en cliente" });
                }
                var terceros = await _terceroNegocio.BuscarPorIdentificacion(identificacion);
                if (terceros == null || !terceros.Any())
                {
                    return BadRequest(new { message = $"No existe un cliente con identificación {identificacion}" });
                }
            }

            var rol = RolAGuardar(identificacion, request.Rol);
            if (rol == UsuarioClaims.RolOperador && EsUsuarioAutenticado(guid))
            {
                return BadRequest(new { message = "No puede quitarse el perfil de administrador" });
            }

            var usuario = new Usuario
            {
                guid = guid,
                Nombre = request.Nombre.Trim(),
                NombreUsuario = request.Usuario.Trim(),
                Contrasena = string.IsNullOrEmpty(request.Contrasena) ? null : request.Contrasena,
                IdentificacionTercero = identificacion,
                Rol = rol
            };

            var resultado = await _usuarioNegocio.ActualizarUsuario(usuario);
            switch (resultado)
            {
                case ResultadoActualizacionUsuario.NoExiste:
                    return NotFound(new { message = "El usuario no existe" });
                case ResultadoActualizacionUsuario.UsuarioDuplicado:
                    return Conflict(new { message = $"El usuario '{usuario.NombreUsuario}' ya existe" });
                default:
                    return Ok(ToResponse(usuario));
            }
        }

        /// <summary>
        /// Habilita o inhabilita un usuario. Un usuario inhabilitado no puede iniciar sesion.
        /// </summary>
        [Authorize(Roles = UsuarioClaims.RolAdministrador)]
        [HttpPost("{guid:guid}/Estado")]
        public async Task<ActionResult> CambiarEstado(Guid guid, [FromBody] CambiarEstadoUsuarioRequest request)
        {
            if (!request.Activo && EsUsuarioAutenticado(guid))
            {
                return BadRequest(new { message = "No puede inhabilitar su propio usuario" });
            }

            var cambiado = await _usuarioNegocio.CambiarEstado(guid, request.Activo);
            if (!cambiado)
            {
                return NotFound(new { message = "El usuario no existe" });
            }
            return Ok(new { message = request.Activo ? "Usuario habilitado" : "Usuario inhabilitado" });
        }

        /// <summary>
        /// Elimina un usuario de forma logica (soft delete): deja de aparecer y no puede iniciar sesion.
        /// </summary>
        [Authorize(Roles = UsuarioClaims.RolAdministrador)]
        [HttpDelete("{guid:guid}")]
        public async Task<ActionResult> Eliminar(Guid guid)
        {
            if (EsUsuarioAutenticado(guid))
            {
                return BadRequest(new { message = "No puede eliminar su propio usuario" });
            }

            var eliminado = await _usuarioNegocio.EliminarUsuario(guid);
            if (!eliminado)
            {
                return NotFound(new { message = "El usuario no existe" });
            }
            return Ok(new { message = "Usuario eliminado" });
        }

        private bool EsUsuarioAutenticado(Guid guid) => User.GetUsuarioGuid() == guid;

        /// <summary>
        /// Valor de la columna Rol: los clientes no llevan rol; el resto es Operador o null (= Administrador).
        /// </summary>
        private static string RolAGuardar(string identificacionTercero, string rolSolicitado)
            => identificacionTercero == null
                && string.Equals(rolSolicitado?.Trim(), UsuarioClaims.RolOperador, StringComparison.OrdinalIgnoreCase)
                ? UsuarioClaims.RolOperador
                : null;

        /// <summary>
        /// Cambia la contrasena del usuario autenticado
        /// </summary>
        [HttpPost("CambiarContrasena")]
        public async Task<ActionResult> CambiarContrasena([FromBody] CambiarContrasenaRequest request)
        {
            var guid = User.GetUsuarioGuid();
            if (guid == null)
            {
                return Unauthorized();
            }
            if (request.ContrasenaActual == request.ContrasenaNueva)
            {
                return BadRequest(new { message = "La contraseña nueva debe ser diferente a la actual" });
            }

            var cambiada = await _usuarioNegocio.CambiarContrasena(guid.Value, request.ContrasenaActual, request.ContrasenaNueva);
            if (!cambiada)
            {
                return BadRequest(new { message = "La contraseña actual es incorrecta" });
            }
            return Ok(new { message = "Contraseña actualizada" });
        }

        /// <summary>
        /// Crear o modificar un usuario
        /// </summary>
        /// <param name="usuario">usuario a crear</param>
        /// <returns>int</returns>
        [Authorize(Roles = UsuarioClaims.RolAdministrador)]
        [HttpPost]
        public async Task<int> Save([FromBody] Usuario usuario)
        {
            var usuarios = new List<Usuario>();
            usuarios.Add(usuario);
            return await _usuarioNegocio.SaveUsuario(usuarios);
        }

        private static UsuarioResponse ToResponse(Usuario usuario) => new UsuarioResponse
        {
            Guid = usuario.guid,
            Nombre = usuario.Nombre,
            Usuario = usuario.NombreUsuario,
            IdentificacionTercero = usuario.IdentificacionTercero,
            Activo = usuario.Activo,
            Rol = UsuarioClaims.ObtenerRol(usuario)
        };
    }
}
