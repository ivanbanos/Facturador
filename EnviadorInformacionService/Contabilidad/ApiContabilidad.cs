using EnviadorInformacionService.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public class ApiContabilidad : IApiContabilidad
    {

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public IEnumerable<int> EnviarFacturas(IEnumerable<Factura> facturas)
        {
            var facturasEnviadas = new List<int>();
            foreach(var factura in facturas)
            {

                Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                var str = regex.Replace(factura.Tercero.Nombre, @" ");
                Logger.Info($"Cliente {str.Trim()} ");
                RequestContabilidad request = new RequestContabilidad(factura);
                 
                using (var client = new HttpClient())
                {
                    try {

                        Logger.Warn(JsonConvert.SerializeObject(request));
                        MultipartFormDataContent form = new MultipartFormDataContent();

                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));
                    var facturaBase64 = System.Convert.ToBase64String(plainTextBytes);
                    form.Add(new StringContent("generar_factura"), "function_name");
                    form.Add(new StringContent(facturaBase64), "parameter");
                    var response = client.PostAsync(ConfigurationManager.AppSettings["UrlSilog"], form).Result;
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;

                    Logger.Info($"Factura enviada, respuesta de silog: {responseBody}");
                        if (responseBody.ToLower().Contains("error")) {
                            facturasEnviadas.Add(factura.IdVentaLocal);
                            Logger.Warn(responseBody);
                        }
                        else
                        {
                            facturasEnviadas.Add(factura.IdVentaLocal);
                        }
                    } catch(Exception ex)
                    {

                        Logger.Error("Factura no recibida en silog por " + ex.Message);
                        Logger.Error("Factura no recibida en silog por " + ex.InnerException?.Message);
                        Logger.Error ("Ex" + ex.StackTrace);
                    }
                }
            }
            return facturasEnviadas;
        }


        public IEnumerable<int> EnviarFacturas(IEnumerable<FacturaProsoft> facturas)
        {
            var facturasEnviadas = new List<int>();

            using (var client = new HttpClient())
            {
                try
                {

                    Logger.Warn(JsonConvert.SerializeObject(facturas));
                    MultipartFormDataContent form = new MultipartFormDataContent();

                    StringContent content = new StringContent(JsonConvert.SerializeObject(facturas));
                    var response = client.PostAsync(ConfigurationManager.AppSettings["UrlProsoft"], content).Result;
                    response.EnsureSuccessStatusCode();

                    string responseBody = response.Content.ReadAsStringAsync().Result;

                    Logger.Info($"Factura enviada, respuesta de prosfot: {responseBody}");
                    if (responseBody.ToLower().Contains("error"))
                    {

                        Logger.Warn(responseBody);
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {

                    Logger.Error("Factura no recibida en prosfot por " + ex.Message);
                    Logger.Error("Ex" + ex.StackTrace);
                }
            }
            return facturasEnviadas;
        }

    }
}
