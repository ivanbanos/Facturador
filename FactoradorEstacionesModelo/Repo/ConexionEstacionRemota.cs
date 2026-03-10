
using EnviadorInformacionService.Models;
using EnviadorInformacionService.Models.Externos;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Web.Controllers;
using FacturadorEstacionesPOSWinForm;
using FacturadorEstacionesPOSWinForm.Repo;
using Microsoft.Extensions.Options;
using Modelo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace EnviadorInformacionService
{
    public class ConexionEstacionRemota : IConexionEstacionRemota
    {
        public readonly InfoEstacion _infoEstacion;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public ConexionEstacionRemota(IOptions<InfoEstacion> options)
        {
            _infoEstacion = options.Value;
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
     (se, cert, chain, sslerror) =>
     {
         return true;
     };
        }


        public bool GetIsTerceroValidoPorIdentificacion(string  identificacion)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/GetIsTerceroValidoPorIdentificacion/{identificacion}";

                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return bool.Parse(responseBody);
            }
        }

        public string ObtenerFacturaPorIdVentaLocal(int idVentaLocal)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/ObtenerFacturaPorIdVentaLocal/{idVentaLocal}";

                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }

        public string ObtenerOrdenDespachoPorIdVentaLocal(int identificacion)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/ObtenerOrdenDespachoPorIdVentaLocal/{identificacion}";

                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }
        public string CrearFacturaOrdenesDeDespacho(string guid)
        {
            List<string> guids = new List<string>() { guid };
            
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/CrearFacturaOrdenesDeDespacho";
                var content = new StringContent(JsonConvert.SerializeObject(guids));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{_infoEstacion.Url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }

        public string CrearFacturaFacturas(string guid)
        {
            List<string> guids = new List<string>() { guid };

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/CrearFacturaFacturas";
                var content = new StringContent(JsonConvert.SerializeObject(guids));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{_infoEstacion.Url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }
        public string EnviarFactura(FactoradorEstacionesModelo.Objetos.Factura factura)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/EnviarFacturaElectronica";
                var content = new StringContent(JsonConvert.SerializeObject(factura));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{_infoEstacion.Url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }

        public List<Canastilla> GetCanastilla()
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/GetCanastilla";

                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<Canastilla>>(responseBody);
            }
        }

        public int GenerarFacturaCanastilla(FacturaCanastilla factura)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                var path = $"/api/Ventas/GenerarFacturaCanastilla";
                var content = new StringContent(JsonConvert.SerializeObject(factura));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{_infoEstacion.Url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return int.Parse(responseBody);
            }
        }

        public object EnviarFacturaSiges(FacturaSiges factura)
        {
            return "";
        }
        public bool EnviarFacturas(IEnumerable<FacturaSiges> facturas, IEnumerable<FormaPagoSiges> formas, Guid estacion, string token)
        {
            RequestEnviarFacturas request = new RequestEnviarFacturas();
            request.facturas = new List<FacturacionelectronicaCore.Negocio.Modelo.Factura>();
            request.ordenDeDespachos = facturas.Select(x =>
            {
                var forma1 = formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).FirstOrDefault();
                var forma2 = x.codigoFormaPago2.HasValue
                    ? formas.Where(y => y.Id == x.codigoFormaPago2.Value).Select(y => y.Descripcion).FirstOrDefault()
                    : null;

                return new FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho(x, forma1, forma2);
            });
            request.Estacion = estacion;
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/EnviarFacturas";
                Logger.Info(JsonConvert.SerializeObject(request));
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = client.PostAsync($"{_infoEstacion.Url}{path}", content).Result;
                
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        public bool EnviarTerceros(IEnumerable<FactoradorEstacionesModelo.Objetos.Tercero> terceros, string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/EnviarTerceros";
                var tercerosEnviar = new List<FacturacionelectronicaCore.Negocio.Modelo.Tercero>();
                foreach (FactoradorEstacionesModelo.Objetos.Tercero t in terceros)
                {
                    if (!tercerosEnviar.Any(x => x.Identificacion == t.identificacion))
                    {
                        tercerosEnviar.Add(new FacturacionelectronicaCore.Negocio.Modelo.Tercero(t));
                    }
                }
                var content = new StringContent(JsonConvert.SerializeObject(tercerosEnviar));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{_infoEstacion.Url}{path}", content).Result;
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        public IEnumerable<string> GetGuidsFacturasPendientes(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetGuidsFacturasPendientes/{estacion}";

                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<string>>(responseBody);
            }
        }

        public string getToken()
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Usuarios/{_infoEstacion.User}/{_infoEstacion.Password}";
                var baseUrl = _infoEstacion.Url?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    throw new InvalidOperationException("InfoEstacion:Url no está configurada.");
                }

                baseUrl = baseUrl.TrimEnd('/');
                if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
                {
                    throw new InvalidOperationException($"InfoEstacion:Url no es válida: '{baseUrl}'.");
                }

                var requestUri = new Uri(baseUri, path);
                Console.WriteLine(requestUri.ToString());

                try
                {
                    var response = client.GetAsync(requestUri).Result;
                    response.EnsureSuccessStatusCode();
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    JObject token = JObject.Parse(responseBody);
                    return token.Value<string>("token");
                }
                catch (HttpRequestException ex)
                {
                    Logger.Error($"Error de conectividad al solicitar token en {requestUri}: {ex.Message}");
                    throw;
                }
            }
        }

        public IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.Factura> RecibirFacturasImprimir(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetFacturasImprimir/{estacion}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.Factura>>(responseBody);
            }
        }


        public IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho> RecibirOrdenesImprimir(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetOrdenesDeDespachoImprimir/{estacion}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho>>(responseBody);
            }
        }

        public IEnumerable<FactoradorEstacionesModelo.Objetos.Tercero> RecibirTercerosActualizados(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetTercerosActualizados/{estacion}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var terceros = JsonConvert.DeserializeObject<IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.Tercero>>(responseBody);
                return terceros.Select(x => new FactoradorEstacionesModelo.Objetos.Tercero(x));
            }
        }



        public IEnumerable<Canastilla> RecibirCanastilla(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/RecibirCanastilla/{estacion}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<Canastilla>>(responseBody);
            }
        }


        public IEnumerable<FactoradorEstacionesModelo.Objetos.Resolucion> GetResolucionEstacion(Guid estacionFuente, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Resolucion/{estacionFuente}";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var resoluciones = JsonConvert.DeserializeObject<IEnumerable<FacturacionelectronicaCore.Negocio.Modelo.Resolucion>>(responseBody);
                var resolucionesLocal = new List<FactoradorEstacionesModelo.Objetos.Resolucion>();
                foreach (var resolucion in resoluciones) { resolucionesLocal.Add(new FactoradorEstacionesModelo.Objetos.Resolucion(resolucion)); }
                return resolucionesLocal;
            }
        }

        public InfoEstacion getInfoEstacion(Guid estacionFuente, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Estaciones/{estacionFuente}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var estacion = JsonConvert.DeserializeObject<Estacion>(responseBody);
                return new InfoEstacion(estacion);
            }
        }

        public bool AgregarFechaReporteFactura(List<FacturaFechaReporte> facturasFechas, Guid estacionFuente, string token)
        {
            RequestCambiarFechasReporte request = new RequestCambiarFechasReporte();
            request.facturas = facturasFechas;
            request.Estacion = estacionFuente;
            Logger.Info(JsonConvert.SerializeObject(request));
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/AgregarFechaReporteFactura";
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{_infoEstacion.Url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        public string GetInfoFacturaElectronica(int idVentaLocal, Guid estacionGuid, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetInfoFacturaElectronica/{idVentaLocal}/estacion/{estacionGuid}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public FactoradorEstacionesModelo.Objetos.Resolucion GetResolucionEstacionCanastilla(Guid estacionFuente, string token)
        {
            throw new NotImplementedException();
        }

        public bool EnviarFacturasCanastilla(IEnumerable<FacturaCanastilla> facturas, Guid estacionFuente, string token)
        {
            RequestfacturasCanastilla request = new RequestfacturasCanastilla();
            request.facturas = facturas;
            request.estacion = estacionFuente;
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("bearer", token);
                var path = $"/api/manejadorinformacionlocal/AddFacturaCanastilla";

                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{_infoEstacion.Url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responsebody = response.Content.ReadAsStringAsync().Result;
                return responsebody != "-1";
            }

        }

        public int ObtenerParaImprimir(Guid idEstacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/FacturasCanastilla/ObtenerParaImprimir/Estacion/{idEstacion}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                return int.Parse(response.Content.ReadAsStringAsync().Result);
            }
        }

        public ResolucionElectronica GetResolucionElectronica(string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetResolucionElectronica";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<ResolucionElectronica>(response.Content.ReadAsStringAsync().Result);
            }
        }

        public string GetInfoFacturaElectronicaCanastilla(int facturasCanastillaId, Guid estacionFuente, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetInfoFacturaElectronicaCanastilla/{facturasCanastillaId}/estacion/{estacionFuente}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
        }
    }
}
