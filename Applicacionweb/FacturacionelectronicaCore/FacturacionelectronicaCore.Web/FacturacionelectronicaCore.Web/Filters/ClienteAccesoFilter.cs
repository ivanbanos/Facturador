using FacturacionelectronicaCore.Web.Authtentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Web.Filters
{
    /// <summary>
    /// Los usuarios con rol Cliente solo pueden invocar los endpoints de esta lista
    /// (ordenes de despacho, facturas de canastilla, reporte fiscal y cambio de contrasena).
    /// Cualquier otro endpoint responde 403. Los administradores no se ven afectados.
    /// </summary>
    public class ClienteAccesoFilter : IAuthorizationFilter
    {
        private static readonly HashSet<string> AccionesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "OrdenesDeDespacho.GetOrdenesDeDespacho",
            "OrdenesDeDespacho.GetConsolidado",
            "OrdenesDeDespacho.GetOrdenesTodasLasEstaciones",
            "FacturasCanastilla.BuscarFacturas",
            "FacturasCanastilla.GetFacturasReporte",
            "FacturasCanastilla.GetFacturasReporteTodasLasEstaciones",
            "FacturasCanastilla.Get",
            "FacturasCanastilla.GetDEtalle",
            "ManejadorInformacionLocal.GetInfoFacturaElectronica",
            "Estaciones.BuscarEstaciones",
            "Usuarios.Login",
            "Usuarios.IniciarSesion",
            "Usuarios.CambiarContrasena",
            "Usuarios.Me",
        };

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.EsCliente())
            {
                return;
            }

            if (context.ActionDescriptor is ControllerActionDescriptor accion
                && AccionesPermitidas.Contains($"{accion.ControllerName}.{accion.ActionName}"))
            {
                return;
            }

            context.Result = new ObjectResult(new { message = "El usuario cliente no tiene acceso a este recurso" })
            {
                StatusCode = 403
            };
        }
    }
}
