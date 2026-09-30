using System;
using EstacionesServicio.Modelo;

namespace FacturacionelectronicaCore.Web.Authtentication
{
    public interface IAuthentication
    {
        string GenerateToken(Usuario usuario);
    }
}
