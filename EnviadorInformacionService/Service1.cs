using EnviadorInformacion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    public partial class Service1 : ServiceBase
    {
        private readonly IEnviadorDeInformacion enviadorDeInformacion;
        private Thread envioThread;
        private readonly ImpresionService impresionService;
        private Thread impresionThread;
        private readonly ProtocoloSiesa protocoloSiesa;
        private Thread siesaThread;
        private readonly ProtocoloSiesaCanastilla protocoloSiesaCanastilla;
        private Thread siesaCanastillaThread;
        private readonly ICanastillaService canastillaService;
        private Thread canastillaServiceThread;
        private Thread canastillaWebServiceThread;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public Service1()
        {
            InitializeComponent();
            // enviadorDeInformacion = new EnviadorDeInformacion();
            // impresionService = new ImpresionService();
            protocoloSiesa = new ProtocoloSiesa();
            protocoloSiesaCanastilla = new ProtocoloSiesaCanastilla();
            // canastillaService = new CanastikllaService();
        }

        protected override void OnStart(string[] args)
        {
            try
            {

                Logger.Error("Iniciando ");
                // if (ConfigurationManager.AppSettings["EnvioInformacion"] == "true")
                // {
                //     envioThread = new Thread(new ThreadStart(enviadorDeInformacion.EnviarInformacion));
                //     envioThread.Start();
                // }

                ////Logger.Info(ConfigurationManager.AppSettings["EnvioASilog"]);
                ////if (ConfigurationManager.AppSettings["EnvioASilog"] == "true")
                ////{

                ////    Logger.Info("Iniciando interfaz Silog");
                ////    enviadorProsoftThread = new Thread(new ThreadStart(enviadorFacturas.EnviarInformacion));
                ////    enviadorProsoftThread.Start();
                ////}
                // impresionThread = new Thread(new ThreadStart(impresionService.Execute));
                // impresionThread.Start();
                // if (canastillaService != null)
                // {
                //     canastillaServiceThread = new Thread(new ThreadStart(canastillaService.ProcesoCanastilla));
                //     canastillaServiceThread.Start();
                //     canastillaWebServiceThread = new Thread(new ThreadStart(canastillaService.WebCanastilla));
                //     canastillaWebServiceThread.Start();
                // }
                // else
                // {
                //     Logger.Warn("canastillaService no esta inicializado. No se iniciaran hilos de canastilla.");
                // }
                siesaThread = new Thread(new ThreadStart(protocoloSiesa.Ejecutar));
                siesaThread.Start();
                // siesaCanastillaThread = new Thread(new ThreadStart(protocoloSiesaCanastilla.Ejecutar));
                // siesaCanastillaThread.Start();

            }
            catch (Exception ex)
            {
                Logger.Error("Error " + ex.Message);
                Logger.Error("Error " + ex.StackTrace);
            }


        }



        protected override void OnStop()
        {
            try
            {
                if (envioThread != null)
                {
                    envioThread.Abort();
                }
                if (impresionThread != null)
                {
                    impresionThread.Abort();
                }
                if (siesaThread != null)
                {
                    siesaThread.Abort();
                }
                if (canastillaServiceThread != null)
                {
                    canastillaServiceThread.Abort();
                }
                if (siesaCanastillaThread != null)
                {
                    siesaCanastillaThread.Abort();
                }
                //enviadorProsoftThread.Abort();
                //siesaCanastillaThread.Abort();
                // siesaThread.Abort();
                //canastillaServiceThread.Abort(); canastillaWebServiceThread.Abort();    
            }
            catch (Exception ex)
            {
                Logger.Error("Error " + ex.Message);
                Logger.Error("Error " + ex.StackTrace);
            }
        }
    }
}
