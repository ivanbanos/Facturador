using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using FactoradorEstacionesModelo.Fidelizacion;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Configuration;
using EnviadorInformacionService.Models;

namespace FacturadorEstacionesRepositorio
{
    public class FidelizacionConexionApi : IFidelizacion
    {
        public string Token;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public FidelizacionConexionApi()
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
     (se, cert, chain, sslerror) =>
     {
         return true;
     };
        }

        public async Task SubirPuntops(float total, string documentoFidelizado, string factura)
        {
            try
            {

                var puntos = new Puntos(total, factura, documentoFidelizado, ConfigurationManager.AppSettings["NitCentroVenta"].ToString() );
                using (var client = new HttpClient())
                {
                    var token = await GetToken();
                    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
                    var path = $"/api/Puntos";
                    var content = new StringContent(JsonConvert.SerializeObject(puntos));
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    var response = await client.PostAsync($"{ConfigurationManager.AppSettings["UrlFidelizacion"]}{path}", content);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                }
            } catch(Exception ex)
            {

            }
        }

        private async Task<string> GetToken()
        {
            using (var client = new HttpClient())
            {
                var path = $"/api/Usuarios/{ConfigurationManager.AppSettings["UserFidelizacion"]}/{ConfigurationManager.AppSettings["PasswordFidelizacion"]}";
                Console.WriteLine($"{ConfigurationManager.AppSettings["UrlFidelizacion"]}{path}");
                var response = await client.GetAsync($"{ConfigurationManager.AppSettings["UrlFidelizacion"]}{path}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject token = JObject.Parse(responseBody);
                return token.Value<string>("token");
            }
        }

        public async Task<IEnumerable<Fidelizado>> GetFidelizados(string documentoFidelizado)
        {
            try { 
            using (var client = new HttpClient())
            {
                var token = await GetToken();

                Console.WriteLine($"{token}");
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/Fidelizados/CentroVenta/{ConfigurationManager.AppSettings["CentroVenta"]}/Fidelizado/{documentoFidelizado}";
                Console.WriteLine($"{ConfigurationManager.AppSettings["UrlFidelizacion"]}{path}");
                var response = await client.GetAsync($"{ConfigurationManager.AppSettings["UrlFidelizacion"]}{path}");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<Fidelizado>>(responseBody);
            }
        } catch(Exception ex)
            {
                return null;
            }
}
    }
}
