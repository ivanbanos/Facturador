using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using FactoradorEstacionesModelo.Fidelizacion;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Dominio.Entidades;
using FacturadorAPI.Models;

namespace FacturadorEstacionesRepositorio
{
    public class FidelizacionConexionApi : IFidelizacion
    {
        public readonly InfoEstacion _infoEstacion;
        public string Token;

        public FidelizacionConexionApi(IOptions<InfoEstacion> options)
        {
            _infoEstacion = options.Value;
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
     (se, cert, chain, sslerror) =>
     {
         return true;
     };
        }

        public async Task SubirPuntops(float total, string documentoFidelizado, string factura)
        {
            var puntos = new Puntos(total, factura, documentoFidelizado, _infoEstacion.NitCentroVenta);
            using (var client = new HttpClient())
            {
                var token = await GetToken();
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/Puntos";
                var content = new StringContent(JsonConvert.SerializeObject(puntos));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = await client.PostAsync($"{_infoEstacion.UrlFidelizacion}{path}", content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> GetToken()
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Usuarios/{_infoEstacion.UserFidelizacion}/{_infoEstacion.PasswordFidelizacion}";
                Console.WriteLine($"{_infoEstacion.UrlFidelizacion}{path}");
                var response = await client.GetAsync($"{_infoEstacion.UrlFidelizacion}{path}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject token = JObject.Parse(responseBody);
                return token.Value<string>("token");
            }
        }

        public async Task<IEnumerable<Fidelizado>> GetFidelizados()
        {
            using (var client = new HttpClient())
            {
                var token = await GetToken();

                Console.WriteLine($"{token}");
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/Fidelizados/CentroVenta/{_infoEstacion.CentroVenta}";
                Console.WriteLine($"{_infoEstacion.UrlFidelizacion}{path}");
                var response = await client.GetAsync($"{_infoEstacion.UrlFidelizacion}{path}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<Fidelizado>>(responseBody);
            }
        }
    }
}
