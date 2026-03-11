using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class ContactsHandler
    {

        public async Task<int> CrearCliente(Contacts contact, Alegra alegra)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", alegra.Auth);
                var path = $"{alegra.Url}contacts";
                var contacto = JsonConvert.SerializeObject(contact);
                var content = new StringContent(contacto);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = await client.PostAsync(path, content);
                string responseBody = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
                return (JsonConvert.DeserializeObject<ResponseContact>(responseBody)).id;
            }
        }

        internal async Task ActualizarCliente(string idFacturacion, Contacts contact, Alegra alegra)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", alegra.Auth);
                var path = $"{alegra.Url}contacts/{idFacturacion}";
                var contacto = JsonConvert.SerializeObject(contact);
                var content = new StringContent(contacto);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = await client.PutAsync(path, content);
                string responseBody = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
