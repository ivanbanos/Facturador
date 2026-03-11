using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EstacionesServicio.Modelo;
using EstacionesServicio.Negocio.Usuario;
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
        private readonly IAuthentication _authentication;

        public UsuariosController(IUsuarioNegocio usuarioNegocio, IAuthentication authentication)
        {
            _usuarioNegocio = usuarioNegocio;
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
        public async Task<ActionResult<string>> Login(string nombre, string contrasena)
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
        /// obtiene el usuario buscando por su identificador unico
        /// </summary>
        /// <param name="guid">Identificador unico</param>
        /// <returns>Usuario</returns>
        [HttpGet("{guid}")]
        public async Task<Usuario> Get(Guid guid)
        => await _usuarioNegocio.GetUsuario(guid);

        /// <summary>
        /// Crear o modificar un usuario
        /// </summary>
        /// <param name="usuario">usuario a crear</param>
        /// <returns>int</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<int> Save([FromBody] Usuario usuario)
        {
            var usuarios = new List<Usuario>();
            usuarios.Add(usuario);
            return await _usuarioNegocio.SaveUsuario(usuarios);
        }
    }
}
