using FacturacionelectronicaCore.Negocio.Modelo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    internal class ResolucionesHandler
    {
        public async Task<IEnumerable<ResolucionElectronica>> GetResolucionesElectronica(Alegra alegra, Repositorio.Entities.ResolucionFacturaElectronica resolucion)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", $"{resolucion.correo}:{resolucion.token}");
                var path = $"{alegra.Url}number-templates/";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<IEnumerable<ResolucionElectronica>>(responseBody);
            }
        }
    }
}