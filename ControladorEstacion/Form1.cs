using ControladorEstacion.Messages;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;
using Modelo;

namespace ControladorEstacion
{
    public partial class Form1 : Form
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly InfoEstacion _infoEstacion;
        private IMessagesReceiver _messageReceiver;

        public Form1(IEstacionesRepositorio estacionesRepositorio, IOptions<InfoEstacion> infoEstacion)
        {
            _estacionesRepositorio = estacionesRepositorio;
            InitializeComponent();
            _infoEstacion = infoEstacion.Value;
            _messageReceiver = new RabbitMQMessagesReceiver(infoEstacion);
            var surtidores = _estacionesRepositorio.GetSurtidoresSiges();
            var surtidoresComponets = new List<Surtidor>();
            var posActual = 0;
            foreach (var surtidor in surtidores)
            {
                var newSurtidor = new Surtidor(surtidor);
                surtidoresComponets.Add(newSurtidor);
                newSurtidor.Location = new System.Drawing.Point(131, 208 + (209 * posActual));
                newSurtidor.Name = surtidor.Descripcion;
                this.Controls.Add(newSurtidor);
                (_messageReceiver as IObservable<string>)?.Subscribe(newSurtidor);
                newSurtidor.BringToFront();
                posActual++;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var formreporte = new FormReporte("lecturas", _estacionesRepositorio, _infoEstacion);
            formreporte.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            var formreporte = new FormReporte("ventas", _estacionesRepositorio, _infoEstacion);
            formreporte.ShowDialog();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}