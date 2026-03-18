using FactoradorEstacionesModelo;
using FactoradorEstacionesModelo.Siges;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControladorEstacion
{
    public partial class Surtidor : UserControl, IObserver<string>
    {
        public Surtidor(FactoradorEstacionesModelo.Siges.SurtidorSiges surtidor)
        {
            InitializeComponent();
            this.Nombre.Text = surtidor.Descripcion.ToLower().Replace("surtidor", "").Replace(" ", "");
            SurtidorControl = surtidor;
        }

        public SurtidorSiges SurtidorControl { get; }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        delegate void SetTurnoCallback(string value);
        private void SetTurno(string value)
        {
            if (this.Turno.InvokeRequired)
            {
                SetTurnoCallback d = new SetTurnoCallback(SetTurno);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                Turno.Text = value;
            }
        }

        delegate void SetIsleroCallback(string value);
        private void SetIslero(string value)
        {
            if (this.Turno.InvokeRequired)
            {
                SetIsleroCallback d = new SetIsleroCallback(SetIslero);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                Islero.Text = value;
            }
        }

        delegate void SetEstadoParCallback(string value);
        private void SetEstadoPar(string value)
        {
            if (this.Turno.InvokeRequired)
            {
                SetEstadoParCallback d = new SetEstadoParCallback(SetEstadoPar);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                this.EstadoPar.Text = value;
            }
        }


        delegate void SetEstadoImparCallback(string value);
        private void SetEstadoImpar(string value)
        {
            if (this.Turno.InvokeRequired)
            {
                SetEstadoImparCallback d = new SetEstadoImparCallback(SetEstadoImpar);
                this.Invoke(d, new object[] { value });
            }
            else
            {
                this.EstadoImpar.Text = value;
            }
        }

        public void OnNext(string value)
        {
            var mensaje = JsonConvert.DeserializeObject<Mensaje>(value);
            if (SurtidorControl.Id == mensaje.SurtidorId)
            {
                SetTurno(mensaje.Turno);
                SetIslero(mensaje.Empleado);
                if (mensaje.Ubicacion == "Par")
                {
                    SetEstadoPar(mensaje.Estado);
                }
                else
                {
                    SetEstadoImpar(mensaje.Estado);
                }
            }
        }
    }
}
