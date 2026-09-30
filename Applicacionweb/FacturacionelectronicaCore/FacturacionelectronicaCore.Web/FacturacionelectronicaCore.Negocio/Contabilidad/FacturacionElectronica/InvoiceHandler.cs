using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class InvoiceHandler
    {
        public async Task<string> CrearFatura(Invoice invoice, Alegra alegra)
        {

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", alegra.Auth);
                var path = $"{alegra.Url}invoices";
                var content = new StringContent(JsonConvert.SerializeObject(invoice));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = client.PostAsync(path, content).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                
                

                return responseBody;
            }

        }

        public async Task<ResponseInvoice> GetFatura(string id, Alegra alegra)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", alegra.Auth);
                var path = $"{alegra.Url}invoices/{id}?";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                    throw new AlegraException(responseBody);
                }

                return JsonConvert.DeserializeObject<ResponseInvoice>(responseBody);
            }
        }
    }
}
