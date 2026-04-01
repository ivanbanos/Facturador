using EnviadorInformacionService.Models;
using EnviadorInformacionService.Models.Externos;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Web.Controllers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using ReporteFacturas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    public class ConexionEstacionRemota : IConexionEstacionRemota
    {
        private string url;
        private string user;
        private string password;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public ConexionEstacionRemota()
        {
            url = ConfigurationManager.AppSettings["url"].ToString();
            user = ConfigurationManager.AppSettings["user"].ToString();
            password = ConfigurationManager.AppSettings["password"].ToString();
        }
        public bool EnviarFacturas(IEnumerable<FactoradorEstacionesModelo.Objetos.Factura> facturas, IEnumerable<FormasPagos> formas, Guid estacion, string token)
        {
           
            RequestEnviarFacturas request = new RequestEnviarFacturas();
            request.facturas = new List<FacturacionelectronicaCore.Negocio.Modelo.Factura>();
            request.ordenDeDespachos = facturas.Select(x =>
            {
                var forma1 = formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).FirstOrDefault();
                var forma2 = x.codigoFormaPago2.HasValue
                    ? formas.Where(y => y.Id == x.codigoFormaPago2.Value).Select(y => y.Descripcion).FirstOrDefault()
                    : null;

                var orden = new FacturacionelectronicaCore.Negocio.Modelo.OrdenDeDespacho(x, forma1, forma2);
                if (x?.Tercero != null && orden?.Tercero != null)
                {
                    AplicarNombreYApellidosSeparados(x.Tercero, orden.Tercero);
                }

                return orden;
            });
            request.Estacion = estacion;
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/EnviarFacturas";
                var requestJson = JsonConvert.SerializeObject(request);
                var content = new StringContent(requestJson);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                Logger.Info($"EnviarFacturas request. Estacion={estacion}, Url={url}{path}, Ordenes={request.ordenDeDespachos?.Count() ?? 0}");
                var response = client.PostAsync($"{url}{path}", content).Result;
                string responseBody = response.Content.ReadAsStringAsync().Result;
                if(!response.IsSuccessStatusCode)
                {
                    Logger.Error($"EnviarFacturas fallo. Estacion={estacion}, Url={url}{path}, Status={(int)response.StatusCode} {response.ReasonPhrase}, ResponseBody={TruncarParaLog(responseBody)}, RequestBody={TruncarParaLog(requestJson)}");
                    return false;
                }

                //Logger.Info($"EnviarFacturas OK. Estacion={estacion}, Url={url}{path}, ResponseBody={TruncarParaLog(responseBody)}");
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
                        var terceroEnviar = new FacturacionelectronicaCore.Negocio.Modelo.Tercero(t);
                        AplicarNombreYApellidosSeparados(t, terceroEnviar);
                        tercerosEnviar.Add(terceroEnviar);
                    }
                }
                var content = new StringContent(JsonConvert.SerializeObject(tercerosEnviar));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();

                string responseBody = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }

        private static string ObtenerNombreCompleto(string nombre, string apellidos)
        {
            return string.Join(" ", new[] { nombre?.Trim(), apellidos?.Trim() }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        }

        private static void AplicarNombreYApellidosSeparados(FactoradorEstacionesModelo.Objetos.Tercero origen, FacturacionelectronicaCore.Negocio.Modelo.Tercero destino)
        {
            if (origen == null || destino == null)
            {
                return;
            }

            var nombre = string.IsNullOrWhiteSpace(origen.Nombre) ? null : origen.Nombre.Trim();
            var apellidos = string.IsNullOrWhiteSpace(origen.Apellidos) ? null : origen.Apellidos.Trim();

            // Keep nombre and apellidos separated; only split when natural person arrives with full name and missing apellido.
            if (string.IsNullOrWhiteSpace(apellidos)
                && !string.IsNullOrWhiteSpace(nombre)
                && !string.Equals(origen.tipoIdentificacionS, "Nit", StringComparison.OrdinalIgnoreCase))
            {
                var partes = nombre.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length > 1)
                {
                    apellidos = partes[partes.Length - 1];
                    nombre = string.Join(" ", partes.Take(partes.Length - 1));
                }
            }

            destino.Nombre = string.IsNullOrWhiteSpace(nombre) ? "No informado" : nombre;
            destino.Apellidos = string.IsNullOrWhiteSpace(apellidos) ? null : apellidos;
        }

        private static string TruncarParaLog(string texto, int maximo = 4000)
        {
            if (string.IsNullOrEmpty(texto))
            {
                return string.Empty;
            }

            if (texto.Length <= maximo)
            {
                return texto;
            }

            return texto.Substring(0, maximo) + $"... [truncado {texto.Length - maximo} caracteres]";
        }

        public IEnumerable<string> GetGuidsFacturasPendientes(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetGuidsFacturasPendientes/{estacion}";

                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<string>>(responseBody);
            }
        }

        public string getToken()
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Usuarios/{user}/{password}";
                Console.WriteLine($"{url}{path}");
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                JObject token = JObject.Parse(responseBody);
                return token.Value<string>("token");
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
                var response = client.GetAsync($"{url}{path}").Result;
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
                var response = client.GetAsync($"{url}{path}").Result;
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
                var response = client.GetAsync($"{url}{path}").Result;
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
                var response = client.GetAsync($"{url}{path}").Result;
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
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                var resoluciones = JsonConvert.DeserializeObject< IEnumerable <FacturacionelectronicaCore.Negocio.Modelo.Resolucion> >(responseBody);
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
                var response = client.GetAsync($"{url}{path}").Result;
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
           // Logger.Info(JsonConvert.SerializeObject(request));
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/AgregarFechaReporteFactura";
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
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
                var response = client.GetAsync($"{url}{path}").Result;
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

                Logger.Info(JsonConvert.SerializeObject(request));
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
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
                var response = client.GetAsync($"{url}{path}").Result;
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
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<ResolucionElectronica>(response.Content.ReadAsStringAsync().Result);
            }
        }

        public bool SetTurnoFactura(int ventaId, DateTime fechaApertura, string isla, int numero, Guid estacionFuente, string token)
        {
            using (var client = new HttpClient())
            {
                var paths = new[]
                {
                    "/api/Factura/AgregarTurnoAFactura",
                    "/api/Facturas/AgregarTurnoAFactura",
                    "/api/OrdenesDeDespacho/AgregarTurnoAFactura"
                };

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                var request = new RequestFacturaTurno()
                {
                    estacion = estacionFuente,
                    fecha = fechaApertura,
                    isla = string.IsNullOrWhiteSpace(isla) ? isla : isla.Trim(),
                    idVentaLocal = ventaId,
                    numero = numero
                };
                var content = new StringContent(JsonConvert.SerializeObject(request));
                //Logger.Info(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                for (var i = 0; i < paths.Length; i++)
                {
                    var path = paths[i];
                    //Logger.Info(path);

                    // Content can be sent only once; recreate it on each attempt.
                    using (var attemptContent = new StringContent(JsonConvert.SerializeObject(request)))
                    {
                        attemptContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = client.PostAsync($"{url}{path}", attemptContent).Result;
                        var responseBody = response.Content.ReadAsStringAsync().Result;

                        //Logger.Info($"SetTurnoFactura respuesta ({path}): status={(int)response.StatusCode} {response.StatusCode}, body={(string.IsNullOrWhiteSpace(responseBody) ? "<vacio>" : responseBody)}");

                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }

                        if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                        {
                            return false;
                        }

                        if (i == paths.Length - 1)
                        {
                            return false;
                        }
                    }
                }

                return false;
            }
        }

        public void SubirTurno(Turno turno, Guid estacionFuente, string token)
        {
            turno.EstacionGuid = estacionFuente.ToString();
            if (string.IsNullOrWhiteSpace(turno.Id))
            {
                turno.Id = Guid.NewGuid().ToString();
            }
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("bearer", token);
                var path = $"/api/Turnos";

                //Logger.Info(JsonConvert.SerializeObject(turno));
                var content = new StringContent(JsonConvert.SerializeObject(turno));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responsebody = response.Content.ReadAsStringAsync().Result;
            }
        }

        public void SubirInfoCupos(CuposRequest cuposInfo, Guid estacionFuente, string token)
        {
            cuposInfo.EstacionGuid = estacionFuente.ToString();
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("bearer", token);
                var path = $"/api/CuposInfo";

                //Logger.Info(JsonConvert.SerializeObject(cuposInfo));
                var content = new StringContent(JsonConvert.SerializeObject(cuposInfo));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{url}{path}", content).Result;
                response.EnsureSuccessStatusCode();
                string responsebody = response.Content.ReadAsStringAsync().Result;
            }
        }

        public string GetInfoFacturaElectronicaCanastilla(int consecutivo, Guid estacionFuente, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/ManejadorInformacionLocal/GetInfoFacturaElectronicaCanastilla/{consecutivo}/estacion/{estacionFuente}";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                return response.Content.ReadAsStringAsync().Result;
            }
        }

        public IEnumerable<Combustible> GetCombustiblesEstacion(Guid estacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Estaciones/{estacion}/Combustibles";

                client.Timeout = new TimeSpan(0, 0, 0, 5, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = client.GetAsync($"{url}{path}").Result;
                response.EnsureSuccessStatusCode();
                var responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<Combustible>>(responseBody);
            }
        }
    }
}
