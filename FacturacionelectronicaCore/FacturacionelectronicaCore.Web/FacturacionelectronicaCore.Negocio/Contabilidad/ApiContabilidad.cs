using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public class ApiContabilidad : IApiContabilidad
    {
        public void EnviarFacturas(IEnumerable<FacturaSilog> facturas)
        {
            foreach(var factura in facturas)
            {
                RequestContabilidad request = new RequestContabilidad(factura,"",null, "");
                using (var client = new HttpClient())
                {
                    var path = $"/api/ManejadorInformacionLocal/EnviarFacturas";
                    MultipartFormDataContent form = new MultipartFormDataContent();
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                    var facturaBase64 = System.Convert.ToBase64String(plainTextBytes);
                    form.Add(new StringContent(facturaBase64), "function_name");
                    form.Add(new StringContent(facturaBase64), "parameter");
                    var response = client.PostAsync($"https://silogcootranshuilaerp.serviciosproductivos.com.co/api/inventario/factura.php/generar_factura.php", form).Result;
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                }
            }
        }
    }
}
