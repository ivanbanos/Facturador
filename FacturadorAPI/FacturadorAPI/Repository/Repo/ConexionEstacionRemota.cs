using FacturadorAPI.Models;
using FacturadorAPI.Models.Externos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http.Headers;

namespace FacturadorAPI.Repository.Repo
{
    public class ConexionEstacionRemota : IConexionEstacionRemota
    {
        private readonly InfoEstacion _infoEstacion;

        public ConexionEstacionRemota(IOptions<InfoEstacion> infoEstacion)
        {
            _infoEstacion = infoEstacion.Value;
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
     (se, cert, chain, sslerror) =>
     {
         return true;
     };
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

        public bool GetIsTerceroValidoPorIdentificacion(string  identificacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Terceros/GetIsTerceroValidoPorIdentificacion/{identificacion}";

                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return bool.Parse(responseBody);
            }
        }

        public async Task<string> ObtenerFacturaPorIdVentaLocal(int idVentaLocal, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Factura/ObtenerFacturaPorIdVentaLocal/{idVentaLocal}/{_infoEstacion.EstacionFuente}";

                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                var responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody;
            }
        }

        public async Task<string> ObtenerOrdenDespachoPorIdVentaLocal(int identificacion, string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/OrdenesDeDespacho/ObtenerOrdenDespachoPorIdVentaLocal/{identificacion}/{_infoEstacion.EstacionFuente}";
                HttpResponseMessage response = null;
                try
                {

                    
                    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

                    response = await client.GetAsync($"{_infoEstacion.Url}{path}");
                   // response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return responseBody;
                }
                catch (Exception ex) {
                    throw new Exception($"{_infoEstacion.Url}{path}"+ response.ReasonPhrase+ response.Content.ReadAsStringAsync().Result);
                }
            }
        }
        public async Task CrearFacturaOrdenesDeDespacho(string guid, string token)
        {
            List<FacturasEntity> guids = new List<FacturasEntity>() { new FacturasEntity() { Guid = Guid.Parse(guid) } };


            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 10, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/OrdenesDeDespacho/EnviarFacturacion/{guid}";
                var response = await client.GetAsync($"{_infoEstacion.Url}{path}");
                string responseBody = await response.Content.ReadAsStringAsync();
            }
        }

        public async Task CrearFacturaFacturas(string guid, string token)
        {
            List<FacturasEntity> guids = new List<FacturasEntity>() { new FacturasEntity() { Guid = Guid.Parse(guid) } };

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 10, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/Factura/EnviarFacturacion/{guid}";
                var response = await client.GetAsync($"{_infoEstacion.Url}{path}");
                //string responseBody = response.Content.ReadAsStringAsync().Result;
            }
        }
        public class FacturasEntity
        {
            public Guid Guid { get; set; }
        }
        public bool EnviarFacturas(IEnumerable<FacturaSiges> facturas, IEnumerable<FormaPagoSiges> formas, string token)
        {
            RequestEnviarFacturas request = new RequestEnviarFacturas();
            request.facturas = facturas.Where(x => x.Consecutivo != 0).Select(x => new FacturaExterna(x, formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).Single()));
            request.ordenDeDespachos = facturas.Where(x => x.Consecutivo == 0).Select(x => new OrdenDeDespacho(x, formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).Single()));
            request.Estacion = Guid.Parse(_infoEstacion.EstacionFuente);
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/EnviarFacturas";
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync($"{_infoEstacion.Url}{path}", content).Result;

                try
                {
                    response.EnsureSuccessStatusCode();
                }catch(Exception ex)
                {
                    throw new Exception(response.Content.ReadAsStringAsync().Result);
                }
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return response.StatusCode == System.Net.HttpStatusCode.OK;
            }
        }
        public async Task<string> GetInfoFacturaElectronica(int idVentaLocal, Guid estacionGuid, string token)
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

        public IEnumerable<Canastilla> RecibirCanastilla(string token)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Canastilla";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = client.GetAsync($"{_infoEstacion.Url}{path}").Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<IEnumerable<Canastilla>>(responseBody);
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

        public async Task<string> GetToken(CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Usuarios/{_infoEstacion.User}/{_infoEstacion.Password}";
                Console.WriteLine($"{_infoEstacion.Url}{path}");
                var response = await client.GetAsync($"{_infoEstacion.Url}{path}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject token = JObject.Parse(responseBody);
                return token.Value<string>("token");
            }
        }

        public async Task<IEnumerable<Canastilla>> RecibirCanastilla(string token, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Canastilla";

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = await client.GetAsync($"{_infoEstacion.Url}{path}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<Canastilla>>(responseBody);
            }
        }

        public async Task EnviarFacturas(List<FacturaSiges> facturaSiges, IEnumerable<FormaPagoSiges> formas, string token)
        {
            RequestEnviarFacturas request = new RequestEnviarFacturas();
            request.facturas = facturaSiges.Where(x => x.Consecutivo != 0).Select(x => new FacturaExterna(x, formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).Single()));
            request.ordenDeDespachos = facturaSiges.Where(x => x.Consecutivo == 0).Select(x => new OrdenDeDespacho(x, formas.Where(y => y.Id == x.codigoFormaPago).Select(y => y.Descripcion).Single()));
            request.Estacion = Guid.Parse(_infoEstacion.EstacionFuente);
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/ManejadorInformacionLocal/EnviarFacturas";
                var content = new StringContent(JsonConvert.SerializeObject(request));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = await client.PostAsync($"{_infoEstacion.Url}{path}", content);

                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    throw new Exception(await response.Content.ReadAsStringAsync());
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                
            }
        }


    }
}
