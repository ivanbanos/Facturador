using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using FacturadorEstacionesRepositorio;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text;

namespace FacturadorAPI.Application.Commands
{
    public class FidelizarVentaCommandHandler : IRequestHandler<FidelizarVentaCommand>
    {
        private readonly ILogger<FidelizarVentaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IFidelizacion _fidelizacion;
        private readonly InfoEstacion _infoEstacion;

        public FidelizarVentaCommandHandler(ILogger<FidelizarVentaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IFidelizacion fidelizacion,
            IOptions<InfoEstacion> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _fidelizacion = fidelizacion ?? throw new ArgumentNullException(nameof(fidelizacion));
            _infoEstacion = options.Value;
        }

        public async Task<Unit> Handle(FidelizarVentaCommand request, CancellationToken cancellationToken)
        {
            //000170FIDELI019099599
            var sum = 119;
            var characters = $"0{request.IdCara}{request.Identificacion}";
            foreach (var character in characters)
            {
                sum += int.Parse(character.ToString());
            }
            var trama = new StringBuilder("").Append('0', 6 - sum.ToString().Length).Append(sum).Append("FIDELI").Append(characters).Append("*");
            var respuesta = send_cmd(trama.ToString()).Trim();


            if (!respuesta.Contains("FIDELIA"))
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
            //Dispose();
        }
    }

}
