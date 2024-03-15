using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Extensions;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace FacturadorAPI.Application.Commands
{
    public class CerrarTurnoCommandHandler : IRequestHandler<CerrarTurnoCommand, string>
    {
        private readonly ILogger<CerrarTurnoCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly InfoEstacion _infoEstacion;

        public CerrarTurnoCommandHandler(ILogger<CerrarTurnoCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota,
            IOptions<InfoEstacion> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
            _infoEstacion = options.Value;
        }

        public async Task<string> Handle(CerrarTurnoCommand request, CancellationToken cancellationToken)
        {


            var turnoA = await _databaseHandler.ObtenerTurnoPorIsla(request.Isla, cancellationToken);
            var respuesta = send_cmd($"000000CER0{request.Isla}{request.Codigo}*").Trim();
            if (!respuesta.Contains("CERA"))
            {
                throw new Exception("¡Error cerrando turno!");
            }
            var turno = await _databaseHandler.ObtenerTurnoIslaYFecha(request.Isla, turnoA.FechaApertura, turnoA.numero);
            var facturas = await _databaseHandler.getFacturaPorTurno(request.Isla, turno.FechaApertura, turno.numero);

            var formas = await _databaseHandler.ListarFormasPagoSP(cancellationToken);

            var informacion = new StringBuilder();
            var guiones = new StringBuilder();
            guiones.Append('-', _infoEstacion.CaracteresPorPagina);
            // Iterate over the file, printing each line.
            informacion.Append("." + "\n\r");
            informacion.Append(_infoEstacion.Razon.Centrar() + "\n\r");
            informacion.Append(("NIT " + _infoEstacion.NIT).Centrar() + "\n\r");
            informacion.Append(_infoEstacion.Nombre.Centrar() + "\n\r");
            informacion.Append(_infoEstacion.Direccion.Centrar() + "\n\r");
            informacion.Append(_infoEstacion.Telefono.Centrar() + "\n\r");
            informacion.Append(guiones.ToString() + "\n\r");
            informacion.Append("Empleado:       " + turno.Empleado + "\n\r");
            informacion.Append("Isla:           " + turno.Isla + "\n\r");
            informacion.Append("Fecha apertura: " + turno.FechaApertura.ToString() + "\n\r");

            informacion.Append("Fecha cierre:   " + turno.FechaCierre.Value.ToString() + "\n\r");

            var totalCantidad = 0m;
            var totalVenta = 0m;

            informacion.Append(guiones.ToString() + "\n\r");
            foreach (var turnosurtidor in turno.turnoSurtidores)
            {
                informacion.Append(formatoTotales("Manguera :", turnosurtidor.Manguera.Descripcion) + "\n\r");
                informacion.Append(formatoTotales("Combustible :", turnosurtidor.Combustible.Descripcion) + "\n\r");
                informacion.Append(formatoTotales("Precio :", $"${string.Format("{0:N2}", turnosurtidor.Combustible.Precio)}") + "\n\r");
                informacion.Append(formatoTotales("Apertura :", turnosurtidor.Apertura.ToString()) + "\n\r");
                if (turno.FechaCierre.HasValue)
                {
                    informacion.Append(formatoTotales("Cierre :", turnosurtidor.Cierre.ToString()) + "\n\r");

                    totalCantidad += Convert.ToDecimal(turnosurtidor.Cierre.Value - turnosurtidor.Apertura);
                    totalVenta += Convert.ToDecimal((turnosurtidor.Cierre.Value - turnosurtidor.Apertura) * turnosurtidor.Combustible.Precio);
                    informacion.Append(formatoTotales("Cantidad :", string.Format("{0:N2}", turnosurtidor.Cierre - turnosurtidor.Apertura)) + "\n\r");
                    informacion.Append(formatoTotales("Total :", $"${string.Format("{0:N2}", (turnosurtidor.Cierre - turnosurtidor.Apertura) * turnosurtidor.Combustible.Precio)}") + "\n\r");

                }

                informacion.Append(guiones.ToString() + "\n\r");
            }
            if (turno.FechaCierre.HasValue)
            {
                if (facturas != null && facturas.Any())
                {

                    //Por forma
                    informacion.Append($"Resumen por forma de pago" + "\n\r");
                    informacion.Append(guiones.ToString() + "\n\r");
                    var groupForma = facturas.GroupBy(x => x.codigoFormaPago);

                    var cantidadTotalmenosEfectivo = 0m;
                    var ventaTotalmenosEfectivo = 0m;
                    foreach (var forma in groupForma)
                    {
                        if (formas.Any(x => x.Id == forma.Key) && forma.Key != 1)
                        {
                            cantidadTotalmenosEfectivo += forma.Sum(x => x.Venta.CANTIDAD);
                            ventaTotalmenosEfectivo += forma.Sum(x => x.Venta.SUBTOTAL);
                            informacion.Append(formatoTotales($"{formas.First(x => x.Id == forma.Key).Descripcion.Trim()} :", $"${string.Format("{0:N2}", forma.Sum(x => x.Venta.SUBTOTAL))}") + "\n\r");

                        }
                    }
                    informacion.Append(formatoTotales($"{formas.First(x => x.Id == 1).Descripcion.Trim()} :", $"${string.Format("{0:N2}", totalVenta - ventaTotalmenosEfectivo)}") + "\n\r");

                    informacion.Append(formatoTotales("Total :", $"${string.Format("{0:N2}", totalVenta)}") + "\n\r");


                    informacion.Append(guiones.ToString() + "\n\r");

                    informacion.Append($"Resumen por Combustibles" + "\n\r");
                    //Totalizador
                    informacion.Append(guiones.ToString() + "\n\r");
                    informacion.Append(formatoTotales("Combustible :", facturas.First().Venta.Combustible) + "\n\r");
                    informacion.Append(formatoTotales("Precio :", $"${string.Format("{0:N2}", facturas.First().Venta.PRECIO_UNI)}") + "\n\r");
                    informacion.Append(formatoTotales("Subtotal :", $"${string.Format("{0:N2}", totalVenta)}") + "\n\r");
                    informacion.Append(formatoTotales("Calibracion :", "$0,00") + "\n\r");
                    informacion.Append(formatoTotales("Descuento :", $"${string.Format("{0:N2}", totalVenta)}") + "\n\r");
                    informacion.Append(formatoTotales("Total :", $"${string.Format("{0:N2}", totalVenta)}") + "\n\r");


                }

            }
            informacion.Append(guiones.ToString() + "\n\r");

            informacion.Append("Fabricado por: SIGES SOLUCIONES SAS ".Centrar() + "\n\r");
            informacion.Append("Nit: 901430393-2 ".Centrar() + "\n\r");
            informacion.Append("Nombre:  Facturador SIGES ".Centrar() + "\n\r");
            var firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();
            firstMacAddress = firstMacAddress ?? "Mac Unknown";
            informacion.Append("SERIAL MAQUINA: ".formatoTotales(firstMacAddress).Centrar() + "\n\r");
            informacion.Append("." + "\n\r");


            return informacion.ToString();

        }
        public string send_cmd(string szData)
        {
            Socket m_socClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {

                Console.WriteLine("Enviando por socket");
                Console.WriteLine($"Trama {szData}");


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
                Console.WriteLine($"REspuesta {szReceived}");
                return szReceived;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"REspuesta {ex.Message}");
                m_socClient.Close();
                m_socClient.Dispose();
                return "Error";
            }
            //Dispose();
        }

        private string formatoTotales(string v1, string v2)
        {
            var result = v1;
            var tabs = new StringBuilder();
            tabs.Append(v1);
            var whitespaces = 40 - v1.Length - v2.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v2);
            return tabs.ToString();
        }
    }
}