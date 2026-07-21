using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using FacturadorEstacionesRepositorio;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text;
using FacturadorAPI.Application.Commands;

namespace FacturadorAPI.Application.Commands
{
    public class FidelizarVentaCommandHandler : IRequestHandler<FidelizarVentaCommand>
    {
        private readonly ILogger<FidelizarVentaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IFidelizacion _fidelizacion;
        private readonly InfoEstacion _infoEstacion;
        private readonly IMediator _mediator;

        public FidelizarVentaCommandHandler(ILogger<FidelizarVentaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IFidelizacion fidelizacion,
            IOptions<InfoEstacion> options,
            IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _fidelizacion = fidelizacion ?? throw new ArgumentNullException(nameof(fidelizacion));
            _infoEstacion = options.Value;
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Unit> Handle(FidelizarVentaCommand request, CancellationToken cancellationToken)
        {
            if (string.Equals(_infoEstacion.TipoFidelizacion, "Servipunto", StringComparison.OrdinalIgnoreCase))
            {
                return await HandleServipunto(request);
            }

            return await HandleSiges(request, cancellationToken);
        }

        private async Task<Unit> HandleSiges(FidelizarVentaCommand request, CancellationToken cancellationToken)
        {
            var idVenta = request.VentaId;
            _logger.LogInformation(
                "FidelizarVenta INICIO. VentaId={VentaId}, Identificacion={Identificacion}, TipoFidelizacion={TipoFidelizacion}",
                idVenta, request.Identificacion, _infoEstacion.TipoFidelizacion);

            var puntos = await _databaseHandler.GetVentaFidelizarAutomaticaPorVenta(idVenta);
            _logger.LogInformation(
                "GetVentaFidelizarAutomaticaPorVenta resultado. VentaId={VentaId}, ValorVenta={ValorVenta}, Factura={Factura}",
                idVenta, puntos?.ValorVenta, puntos?.Factura);

            var factura = await _databaseHandler.GetFacturaPorIdVenta(idVenta);
            _logger.LogInformation(
                "GetFacturaPorIdVenta resultado. VentaId={VentaId}, FacturaPOSId={FacturaPOSId}, Enviada={Enviada}",
                idVenta, factura?.facturaPOSId, factura?.enviada);

            _logger.LogInformation(
                "Llamando SubirPuntops. Total={Total}, Identificacion={Identificacion}, Factura={Factura}",
                puntos?.ValorVenta, request.Identificacion, puntos?.Factura);

            var ok = await _fidelizacion.SubirPuntops((float)puntos.ValorVenta, request.Identificacion, puntos.Factura);

            _logger.LogInformation(
                "SubirPuntops resultado. VentaId={VentaId}, Identificacion={Identificacion}, Ok={Ok}",
                idVenta, request.Identificacion, ok);

            if (!ok)
            {
                _logger.LogWarning(
                    "SubirPuntops devolvió false. VentaId={VentaId}, Identificacion={Identificacion}. Retornando 400 Venta fidelizada.",
                    idVenta, request.Identificacion);
                throw new Exception("Venta fidelizada");
            }

            await _databaseHandler.ActualizarFacturaFidelizada(request.Identificacion, factura.ventaId);
            _logger.LogInformation(
                "ActualizarFacturaFidelizada ejecutado. VentaId={VentaId}, Identificacion={Identificacion}",
                factura.ventaId, request.Identificacion);

            var fidelizados = await _fidelizacion.GetFidelizados(request.Identificacion);
            _logger.LogInformation(
                "GetFidelizados resultado. Identificacion={Identificacion}, Cantidad={Cantidad}",
                request.Identificacion, fidelizados?.Count());

            foreach (var fidelizado in fidelizados)
            {
                _logger.LogInformation(
                    "AddFidelizado. Documento={Documento}, Puntos={Puntos}",
                    fidelizado.Documento, fidelizado.Puntos);
                await _databaseHandler.AddFidelizado(fidelizado.Documento, fidelizado.Puntos ?? 0);
            }

            _logger.LogInformation(
                "Enviando MandarImprimirCommand. VentaId={VentaId}, FacturaPOSId={FacturaPOSId}, TerceroId={TerceroId}, FormaPago={FormaPago}",
                factura.ventaId, request.FacturaPOSId, request.TerceroId, request.FormaPago);

            await _mediator.Send(new MandarImprimirCommand(
                facturaPOSId: request.FacturaPOSId,
                terceroId: request.TerceroId,
                formaPago: request.FormaPago,
                ventaId: factura.ventaId,
                placa: request.Placa ?? "NP",
                kilometraje: request.Kilometraje ?? "NP",
                numeroTransaccion: request.NumeroTransaccion ?? "NP",
                impresiones: 1,
                formaPago2: request.FormaPago2,
                total1: request.Total1,
                total2: request.Total2), cancellationToken);

            _logger.LogInformation(
                "FidelizarVenta COMPLETADO. VentaId={VentaId}, Identificacion={Identificacion}",
                idVenta, request.Identificacion);
            return Unit.Value;
        }

        private Task<Unit> HandleServipunto(FidelizarVentaCommand request)
        {
            var characters = $"0{request.VentaId}{request.Identificacion}";
            var sum = 119;
            foreach (var character in characters)
            {
                sum += int.Parse(character.ToString());
            }
            var trama = new StringBuilder()
                .Append('0', 6 - sum.ToString().Length)
                .Append(sum)
                .Append("FIDELI")
                .Append(characters)
                .Append("*")
                .ToString();

            var respuesta = send_cmd(trama).Trim();

            if (!respuesta.Contains("FIDELIA"))
            {
                throw new Exception("¡Error fidelizando en Servipunto!");
            }

            return Task.FromResult(Unit.Value);
        }

        private string send_cmd(string szData)
        {
            Socket m_socClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                int alPort = System.Convert.ToInt16(_infoEstacion.puerto, 10);
                System.Net.IPAddress remoteIPAddress = System.Net.IPAddress.Parse(_infoEstacion.ip);
                System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddress, alPort);
                m_socClient.Connect(remoteEndPoint);

                byte[] byData = System.Text.Encoding.ASCII.GetBytes(szData);
                m_socClient.Send(byData);
                byte[] b = new byte[100];
                m_socClient.Receive(b);
                string szReceived = Encoding.ASCII.GetString(b);
                m_socClient.Close();
                m_socClient.Dispose();
                return szReceived;
            }
            catch (Exception)
            {
                m_socClient.Close();
                m_socClient.Dispose();
                return "Error";
            }
        }
    }
}
