using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text;

namespace FacturadorAPI.Application.Commands
{
    public class ReimprimirTurnoCommandHandler : IRequestHandler<ReimprimirTurnoCommand>
    {
        private readonly ILogger<ReimprimirTurnoCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly InfoEstacion _infoEstacion;

        public ReimprimirTurnoCommandHandler(ILogger<ReimprimirTurnoCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota,
            IOptions<InfoEstacion> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
            _infoEstacion = options.Value;
        }

        public async Task<Unit> Handle(ReimprimirTurnoCommand request, CancellationToken cancellationToken)
        {
            //000029IMPCIE012023010101
            var sum = 18;
            var characters = $"0{request.IdIsla}{request.Fecha.ToString("yyyyMMdd")}0{request.Posicion}";
            foreach(var character in characters)
            {
                sum += int.Parse(character.ToString());
            }
            var trama = new StringBuilder("").Append('0', 6-sum.ToString().Length).Append(sum).Append("IMPCIE").Append(characters).Append("*");
            var respuesta = send_cmd(trama.ToString()).Trim();


            if (!respuesta.Contains("IMPCIEA"))
            {
                throw new Exception("¡Error abriendo turno!");
            }
            return Unit.Value;

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
    }
}