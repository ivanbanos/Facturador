using System.ComponentModel.DataAnnotations;

namespace FacturacionelectronicaCore.Web.Controllers
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contrasena es obligatoria")]
        public string Contrasena { get; set; }
    }

    public class CrearUsuarioRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, ErrorMessage = "El nombre no puede superar 50 caracteres")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[^\s/\\?#%]+$", ErrorMessage = "El usuario no puede tener espacios ni los caracteres / \\ ? # %")]
        public string Usuario { get; set; }

        [Required(ErrorMessage = "La contrasena es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contrasena debe tener entre 6 y 100 caracteres")]
        public string Contrasena { get; set; }

        /// <summary>
        /// Identificacion del tercero (cliente). Si viene vacia el usuario es administrador.
        /// </summary>
        [StringLength(250)]
        public string IdentificacionTercero { get; set; }
    }

    public class CambiarContrasenaRequest
    {
        [Required(ErrorMessage = "La contrasena actual es obligatoria")]
        public string ContrasenaActual { get; set; }

        [Required(ErrorMessage = "La contrasena nueva es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contrasena nueva debe tener entre 6 y 100 caracteres")]
        public string ContrasenaNueva { get; set; }
    }

    public class UsuarioResponse
    {
        public System.Guid Guid { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public string IdentificacionTercero { get; set; }
        public string Rol { get; set; }
    }
}
