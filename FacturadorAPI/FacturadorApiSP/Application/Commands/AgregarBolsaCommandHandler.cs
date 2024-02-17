using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using FacturadorApiSP.Application.Commands;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text;

namespace FacturadorAPI.Application.Commands
{
    public class AgregarBolsaCommandHandler : IRequestHandler<AgregarBolsaCommand>
    {
        private readonly ILogger<AgregarBolsaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly InfoEstacion _infoEstacion;

        public AgregarBolsaCommandHandler(ILogger<AgregarBolsaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota,
            IOptions<InfoEstacion> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
            _infoEstacion = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<Unit> Handle(AgregarBolsaCommand request, CancellationToken cancellationToken)
        {
            //000178REGBST0181990610;200
            var sum = 141;
            var num = int.Parse(request.Numero) < 10 ? "0" + request.Numero : request.Numero;
            var characters = $"0{request.Isla}{request.Codigo}{num}{request.Moneda};{request.Cantidad}";
            foreach (var character in characters)
            {
                if(character != ';')
                {
                    sum += int.Parse(character.ToString());
                }
            }
            var trama = new StringBuilder("").Append('0', 6 - sum.ToString().Length).Append(sum).Append("REGBST").Append(characters).Append("*");
            var respuesta = send_cmd(trama.ToString()).Trim();


            if (!respuesta.Contains("REGBSTA"))
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