using FactoradorEstacionesModelo.Siges;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modelo;
using Newtonsoft.Json;
using NLog;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ManejadorSurtidor.SICOM
{
    public class SicomConection : ISicomConection
    {
        private Sicom sicom;
        private readonly InfoEstacion _infoEstacion;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public SicomConection(IOptions<Sicom> options, ILogger<SicomConection> logger, IOptions<InfoEstacion> infoEstacion)
        {
            sicom = options.Value;
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
     (se, cert, chain, sslerror) =>
     {
         return true;
     };
            _infoEstacion = infoEstacion.Value;
        }

        public async Task<VehiculoSuic> validateIButton(string iButton)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    var path = $"/chip/{iButton}";

                    Uri uri = new Uri(sicom.Url + path);
                    var authenticationString = $"{sicom.Usuario}:{sicom.Contrasena}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

                    client.DefaultRequestHeaders.Add("APIKey", sicom.APIKey);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                    var response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    //Logger.Log(NLog.LogLevel.Info, $"Sicom respuesta {responseBody}");
                    var chipResponse = JsonConvert.DeserializeObject<VehiculoSuic>(responseBody);
                    return chipResponse;
                } catch(Exception ex)
                {

                    //Logger.Log(NLog.LogLevel.Error, $"Sicom error {ex.Message}");
                    return null;
                }
            }
        }


        public async Task<bool> enviarVenta(string iButton, float consumo, DateTime fecha)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    var path = $"/consumo/{iButton}";

                    Uri uri = new Uri(sicom.Url + path);
                    var authenticationString = $"{sicom.Usuario}:{sicom.Contrasena}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

                    client.DefaultRequestHeaders.Add("APIKey", sicom.APIKey);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);

                    var person = new Consumo() { volumen = consumo, fecha = fecha.ToString("yyyy-MM-ddTHH:mm:ss.902Z") };

                    var json = JsonConvert.SerializeObject(person);

                    //Logger.Log(NLog.LogLevel.Info, $"Enviando {json}");
                    var data = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(uri, data);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    //Logger.Log(NLog.LogLevel.Info, $"Sicom respuesta {responseBody}");
                    //var chipResponse = JsonConvert.DeserializeObject<ChipResponse>(responseBody);
                    return true;
                }
                catch (Exception ex)
                {

                    //Logger.Log(NLog.LogLevel.Error, $"Sicom error {ex.Message}");
                    return false;
                }
            }
        }

        public async Task<string> GetInfoCarros()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    System.Net.ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    var path = $"/certificados/";

                    Uri uri = new Uri(sicom.Url + path);
                    var authenticationString = $"{sicom.Usuario}:{sicom.Contrasena}";
                    var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

                    client.DefaultRequestHeaders.Add("APIKey", sicom.APIKey);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
                    var response = await client.GetAsync(uri);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    //_logger.LogInformation( $"Sicom respuesta {responseBody}");

                    var suicFilePath = GetSuicFilePath();
                    var suicDirectory = Path.GetDirectoryName(suicFilePath);
                    if (!string.IsNullOrWhiteSpace(suicDirectory))
                    {
                        Directory.CreateDirectory(suicDirectory);
                    }

                    File.WriteAllText(suicFilePath, responseBody);
                    return responseBody;
                }
                catch (Exception ex)
                {

                    //Logger.Log(NLog.LogLevel.Error, $"Sicom error {ex.Message}");
                    return "Fail";
                }
            }
        }

        private string GetSuicFilePath()
        {
            var configuredPath = _infoEstacion.ArchivoSiCOM;
            var basePath = string.IsNullOrWhiteSpace(configuredPath)
                ? AppContext.BaseDirectory
                : configuredPath;

            if (!Path.IsPathRooted(basePath))
            {
                basePath = Path.Combine(AppContext.BaseDirectory, basePath);
            }

            var fullDirectory = Path.GetFullPath(basePath);
            return Path.Combine(fullDirectory, "SUIC.txt");
        }

    }
}
