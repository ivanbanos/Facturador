using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class ItemHandler
    {
        public async Task<Item> GetItem(string name, Alegra alegra)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", alegra.Auth);
                var path = $"{alegra.Url}items/?name={name}";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<IEnumerable<Item>>(responseBody).FirstOrDefault();
            }
        }

        public async Task<Item> AddItem(string name, string description, string reference, decimal price, string tax, Alegra alegra)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", alegra.Auth);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var path = $"{alegra.Url}items";
                
                var itemData = new
                {
                    name = name,
                    description = description,
                    reference = reference,
                    price = price,
                    tax = tax,
                    type = "product"
                };

                var jsonContent = JsonConvert.SerializeObject(itemData);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(path, content);
                string responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                return JsonConvert.DeserializeObject<Item>(responseBody);
            }
        }

    }
}
