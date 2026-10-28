using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Security.Claims;

namespace FacturacionelectronicaCore.Web.Authtentication
{
    /// <summary>
    /// Roles y claims usados en el token JWT.
    /// Un usuario con identificacion de tercero asociada es "Cliente"; sin ella es "Administrador"
    /// u "Operador" (igual al administrador pero sin gestion de usuarios) segun su columna Rol.
    /// </summary>
    public static class UsuarioClaims
    {
        public const string RolAdministrador = "Administrador";
        public const string RolOperador = "Operador";
        public const string RolCliente = "Cliente";
        public const string IdentificacionTercero = "identificacionTercero";

        /// <summary>
        /// Rol efectivo de un usuario: Cliente si tiene tercero asociado; si no, Operador o Administrador.
        /// Un rol guardado desconocido o vacio se trata como Administrador (comportamiento anterior).
        /// </summary>
        public static string ObtenerRol(EstacionesServicio.Modelo.Usuario usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario.IdentificacionTercero))
            {
                return RolCliente;
            }
            return string.Equals(usuario.Rol?.Trim(), RolOperador, StringComparison.OrdinalIgnoreCase)
                ? RolOperador
                : RolAdministrador;
        }

        public static bool EsCliente(this ClaimsPrincipal user)
            => user?.IsInRole(RolCliente) ?? false;

        public static string GetIdentificacionTercero(this ClaimsPrincipal user)
            => user?.FindFirst(IdentificacionTercero)?.Value;

        public static Guid? GetUsuarioGuid(this ClaimsPrincipal user)
        {
            var valor = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(valor, out var guid) ? guid : null;
        }

        /// <summary>
        /// Indica si la identificacion corresponde al cliente autenticado.
        /// </summary>
        public static bool EsDelCliente(this ClaimsPrincipal user, string identificacion)
        {
            var propia = user.GetIdentificacionTercero();
            return !string.IsNullOrWhiteSpace(propia)
                && string.Equals(propia.Trim(), identificacion?.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Para usuarios cliente devuelve una copia del filtro forzada a su identificacion
        /// (ignora lo que envie el navegador). Para administradores devuelve el filtro sin cambios.
        /// </summary>
        public static FiltroBusqueda RestringirFiltroACliente(this ClaimsPrincipal user, FiltroBusqueda filtro)
        {
            if (!user.EsCliente())
            {
                return filtro;
            }

            return new FiltroBusqueda
            {
                FechaInicial = filtro?.FechaInicial,
                FechaFinal = filtro?.FechaFinal,
                Estacion = filtro?.Estacion ?? Guid.Empty,
                Identificacion = user.GetIdentificacionTercero(),
                NombreTercero = null
            };
        }
    }
}
