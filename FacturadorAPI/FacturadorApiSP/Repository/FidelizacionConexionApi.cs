using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using FactoradorEstacionesModelo.Fidelizacion;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using Dominio.Entidades;
using FacturadorAPI.Models;
using System.Net;
using Microsoft.Extensions.Logging;

namespace FacturadorEstacionesRepositorio
{
    public class FidelizacionConexionApi : IFidelizacion
    {
        public readonly InfoEstacion _infoEstacion;
        private readonly ILogger<FidelizacionConexionApi> _logger;
        public string Token;

        public FidelizacionConexionApi(IOptions<InfoEstacion> options, ILogger<FidelizacionConexionApi> logger)
        {
            _infoEstacion = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
     (se, cert, chain, sslerror) =>
     {
         return true;
     };

            _logger.LogInformation(
                "Fidelizacion config loaded. Url={Url}, User={User}, CentroVenta={CentroVenta}, NitCentroVenta={NitCentroVenta}",
                _infoEstacion.UrlFidelizacion,
                _infoEstacion.UserFidelizacion,
                _infoEstacion.CentroVenta,
                _infoEstacion.NitCentroVenta);
        }

        public async Task<bool> SubirPuntops(float total, string documentoFidelizado, string factura)
        {
            var puntos = new Puntos(total, factura, documentoFidelizado, _infoEstacion.NitCentroVenta, _infoEstacion.CentroVenta);
            var jsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var requestJson = JsonConvert.SerializeObject(puntos, jsonSettings);
            _logger.LogInformation(
                "SubirPuntops REQUEST. Url={Url}, Documento={Documento}, Factura={Factura}, NitCentroVenta={NitCentroVenta}, CentroVenta={CentroVenta}, Total={Total}, Body={Body}",
                BuildAbsoluteUrl("/api/Puntos"), documentoFidelizado, factura, _infoEstacion.NitCentroVenta, _infoEstacion.CentroVenta, total, requestJson);

            using (var client = new HttpClient())
            {
                var token = await GetToken();
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/Puntos";
                var content = new StringContent(requestJson);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = await client.PostAsync(BuildAbsoluteUrl(path), content);

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(
                    "SubirPuntops RESPONSE. StatusCode={StatusCode}, Documento={Documento}, Factura={Factura}, Body={Body}",
                    (int)response.StatusCode, documentoFidelizado, factura, responseBody);

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    _logger.LogWarning(
                        "Fidelizacion POST /api/Puntos returned 403. Documento={Documento}, Factura={Factura}, CentroVenta={CentroVenta}, NitCentroVenta={NitCentroVenta}, Body={Body}",
                        documentoFidelizado,
                        factura,
                        _infoEstacion.CentroVenta,
                        _infoEstacion.NitCentroVenta,
                        responseBody);
                    return false;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Fidelizacion API error while posting points. StatusCode={(int)response.StatusCode}. Body={responseBody}",
                        null,
                        response.StatusCode);
                }

                var result = JsonConvert.DeserializeObject<bool>(responseBody);
                if (!result)
                {
                    _logger.LogWarning(
                        "SubirPuntops: API respondió 200 pero devolvió false. Posiblemente total=0 o venta ya fidelizada. Documento={Documento}, Factura={Factura}, Total={Total}, Body={Body}",
                        documentoFidelizado, factura, total, responseBody);
                }
                return result;
            }
        }

        private async Task<string> GetToken()
        {
            using (var client = new HttpClient())
            {
                var user = Uri.EscapeDataString(_infoEstacion.UserFidelizacion ?? string.Empty);
                var password = Uri.EscapeDataString(_infoEstacion.PasswordFidelizacion ?? string.Empty);
                var path = $"/api/Usuarios/{user}/{password}";
                var response = await client.GetAsync(BuildAbsoluteUrl(path));

                if (!response.IsSuccessStatusCode)
                {
                    var tokenErrorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Fidelizacion token request failed. StatusCode={StatusCode}, User={User}, Body={Body}",
                        (int)response.StatusCode,
                        _infoEstacion.UserFidelizacion,
                        tokenErrorBody);

                    throw new HttpRequestException(
                        $"Fidelizacion token request failed. StatusCode={(int)response.StatusCode}. Body={tokenErrorBody}",
                        null,
                        response.StatusCode);
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                JObject token = JObject.Parse(responseBody);
                var tokenValue = token.Value<string>("token");

                if (string.IsNullOrWhiteSpace(tokenValue))
                {
                    _logger.LogWarning(
                        "Fidelizacion token response does not contain token property. Body={Body}",
                        responseBody);
                    throw new HttpRequestException("Fidelizacion token response is missing token field.");
                }

                return tokenValue;
            }
        }

        public async Task<IEnumerable<Fidelizado>> GetFidelizados(string documentoFidelizado)
        {
            using (var client = new HttpClient())
            {
                var token = await GetToken();

                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", token);
                var path = $"/api/Fidelizados/CentroVenta/{_infoEstacion.CentroVenta}/Fidelizado/{documentoFidelizado}";
                var response = await client.GetAsync(BuildAbsoluteUrl(path));

                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    var forbiddenBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Fidelizacion GET /api/Fidelizados returned 403. Documento={Documento}, CentroVenta={CentroVenta}, Body={Body}",
                        documentoFidelizado,
                        _infoEstacion.CentroVenta,
                        forbiddenBody);
                    return Enumerable.Empty<Fidelizado>();
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException(
                        $"Fidelizacion API error while querying fidelizados. StatusCode={(int)response.StatusCode}. Body={errorBody}",
                        null,
                        response.StatusCode);
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<Fidelizado>>(responseBody);
            }
        }

        private string BuildAbsoluteUrl(string relativePath)
        {
            return $"{_infoEstacion.UrlFidelizacion?.TrimEnd('/')}{relativePath}";
        }
    }
}
