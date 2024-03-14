using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Extensions;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace FacturadorAPI.Application.Commands
{
    public class AbrirTurnoCommandHandler : IRequestHandler<AbrirTurnoCommand, string>
    {
        private readonly ILogger<AbrirTurnoCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly InfoEstacion _infoEstacion;

        public AbrirTurnoCommandHandler(ILogger<AbrirTurnoCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota,
            IOptions<InfoEstacion> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
            _infoEstacion = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<string> Handle(AbrirTurnoCommand request, CancellationToken cancellationToken)
        {

            var respuesta = send_cmd($"000000ABR0{request.Isla}{request.Codigo}*").Trim();

            if (!respuesta.Contains("ABRA"))
            {
                throw new Exception("¡Error abriendo turno!");
            }
            var turno = await _databaseHandler.ObtenerTurnoPorIsla(request.Isla, cancellationToken);
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
            informacion.Append("Fecha apertura: " + turno.FechaApertura.ToString()+"\n\r");
            var reporteCierrePorTotal = new List<FacturaSiges>();
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
            catch (Exception)
            {
                m_socClient.Close();
                m_socClient.Dispose();
                return "Error";
            }
            //Dispose();
        }
    }
}