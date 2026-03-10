using System.IO.Ports;
using System.Text;
using FacturadorEstacionesRepositorio;
using FactoradorEstacionesModelo.Siges;
using ManejadorSurtidor.SICOM;
using Microsoft.Extensions.Options;
using ManejadorSurtidor.Messages;
using FactoradorEstacionesModelo;
using NLog;
using Newtonsoft.Json;

namespace ManejadorSurtidor
{
    public class OperadorCara
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;

        SerialPort serialPort1 = null!;
        IEnumerable<SurtidorSiges> Surtidores;
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private int count = 0;
        private LectorIButton lectorIButton;
        private readonly IMessageProducer _messageProducer;
        private bool respondio;
        private bool finalizo;
        private int? _surtidorEsperaId;
        private int? _mangueraEsperaId;
        private readonly Sicom _sicom;
        private readonly ISicomConection _sicomConection;
        private readonly IFidelizacion _fidelizacion;
        private readonly Islas _islas;
        private readonly Dictionary<int, int> _reposoConsecutivoVendiendo = new Dictionary<int, int>();
        private const int MIN_REPOSO_CONSECUTIVO_FIN_VENTA = 2;
        public OperadorCara(Logger logger, IEnumerable<SurtidorSiges> surtidores, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection, IMessageProducer messageProducer, IFidelizacion fidelizacion, Islas islas)
        {
            _sicomConection = sicomConection;
            _logger = logger;
            _estacionesRepositorio = estacionesRepositorio;
            _sicom = options.Value;
            _messageProducer = messageProducer;
            lectorIButton = new LectorIButton();
            Surtidores = surtidores;

            System.ComponentModel.IContainer components = new System.ComponentModel.Container();
            serialPort1 = new SerialPort(components);
            serialPort1.PortName = surtidores.First().Puerto;
            serialPort1.BaudRate = 4800;
            serialPort1.Encoding = Encoding.GetEncoding(28591);
            //serialPort1.Encoding = Encoding.Default;
            serialPort1.Parity = Parity.Even;
            serialPort1.DataBits = 8;
            serialPort1.StopBits = StopBits.One;
            serialPort1.ReceivedBytesThreshold = 10;
            serialPort1.RtsEnable = true;
            serialPort1.Handshake = Handshake.None;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceiverHandler);
            //serialPort1.WriteTimeout = 500;
            //serialPort1.ReadTimeout = 500;
            _fidelizacion = fidelizacion;
            _islas = islas;
        }


        public async Task OperarCara(CancellationToken stoppingToken)
        {

            try
            {
                foreach (var surtidor in Surtidores)
                {
                    surtidor.mangueras = new List<MangueraSiges>();
                    surtidor.mangueras.AddRange(surtidor.caras.Select(x => _estacionesRepositorio.GetMangueras(x.Id).First()));

                    _logger.Log(NLog.LogLevel.Info, $"Inicicando Surtidor {surtidor.Descripcion}");

                }
                _logger.Log(NLog.LogLevel.Info, $"Puerto configurado: {serialPort1.PortName}, BaudRate: {serialPort1.BaudRate}, Parity: {serialPort1.Parity}, DataBits: {serialPort1.DataBits}, StopBits: {serialPort1.StopBits}");
                var puertosDisponibles = SerialPort.GetPortNames();
                _logger.Log(NLog.LogLevel.Info, $"Puertos detectados: {string.Join(",", puertosDisponibles)}");
                if (!puertosDisponibles.Any(x => string.Equals(x, serialPort1.PortName, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.Log(NLog.LogLevel.Warn, $"El puerto configurado {serialPort1.PortName} no aparece en los puertos detectados del sistema.");
                }
                if (!serialPort1.IsOpen)
                {
                    _logger.Log(NLog.LogLevel.Info, $"Intentando abrir puerto {serialPort1.PortName}");
                    serialPort1.Open();
                    _logger.Log(NLog.LogLevel.Info, $"Puerto {serialPort1.PortName} abierto correctamente");
                }
                foreach (var surtidor in Surtidores)
                {
                    foreach (var manguera in surtidor.mangueras)
                    {

                        await desautorizarManguera(surtidor, manguera, stoppingToken);
                        manguera.Estado = "BuscandoUltimaVenta";
                        await venta(surtidor, manguera, stoppingToken);
                        manguera.Estado = "Totalizadores";
                        await totalizadorManguera(surtidor, manguera, stoppingToken);
                        manguera.NuevoTotalizador = manguera.totalizador;
                        manguera.NuevaVenta = manguera.ultimaVenta;
                        manguera.Estado = "Lista";
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex, "Error inicializando surtidores");
                _logger.Log(NLog.LogLevel.Error, $"Fallo inicializando/abriendo puerto serial {serialPort1?.PortName}. Excepción: {ex}");

            }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var surtidor in Surtidores)
                    {
                        try
                        {
                            await ProcesarSurtidorAsync(surtidor, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            LogException(ex, $"Error en ciclo del surtidor {surtidor.Descripcion}");
                            await ResetYDesautorizarTodasMangueras(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogException(ex, "Error general en loop de OperarCara");
                    await ResetYDesautorizarTodasMangueras(stoppingToken);
                    if (serialPort1 != null)
                    {
                        serialPort1.Close();
                    }
                }
            }
        }

        private async Task ProcesarSurtidorAsync(SurtidorSiges surtidor, CancellationToken stoppingToken)
        {
            surtidor.turno = _estacionesRepositorio.ObtenerTurnoSurtidor(surtidor.Id);

            if (surtidor.turno == null)
            {
                await Task.Delay(1000, stoppingToken);
                return;
            }

            surtidor.idTurno = surtidor.turno.Id;
            if (surtidor.turno.IdEstado == 1)
            {
                await ProcesarAperturaTurnoAsync(surtidor, stoppingToken);
            }
            else if (surtidor.turno.IdEstado == 3 || surtidor.turno.IdEstado == 4)
            {
                await ProcesarCierreTurnoAsync(surtidor, stoppingToken);
            }
            else if (surtidor.turno.IdEstado == 2)
            {
                await ProcesarTurnoOperativoAsync(surtidor, stoppingToken);
            }

            await Task.Delay(1000, stoppingToken);
        }

        private async Task ProcesarAperturaTurnoAsync(SurtidorSiges surtidor, CancellationToken stoppingToken)
        {
            _logger.Log(NLog.LogLevel.Info, $"Abriendo turno {surtidor.Descripcion}");
            foreach (var manguera in surtidor.mangueras)
            {
                await sendEstado(surtidor.Id, manguera.Ubicacion, "Abriendo", surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
                manguera.Estado = "Totalizadores";
                _logger.Log(NLog.LogLevel.Info, $"Buscando totalizador Manguera {manguera.Descripcion}");
                await totalizadorManguera(surtidor, manguera, stoppingToken);

                _logger.Log(NLog.LogLevel.Info, $"Abriendo {manguera.Descripcion} {surtidor.turno.Id} {manguera.totalizador}");
                _estacionesRepositorio.EnviarTotalizadorApertura(surtidor.Id, surtidor.turno.Id, manguera.Id, manguera.totalizador);
                await sendEstado(surtidor.Id, manguera.Ubicacion, "Abierta", surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
                _logger.Log(NLog.LogLevel.Info, $"Abrierta {manguera.Descripcion} {surtidor.turno.Id} {manguera.totalizador}");
            }
        }

        private async Task ProcesarCierreTurnoAsync(SurtidorSiges surtidor, CancellationToken stoppingToken)
        {
            _logger.Log(NLog.LogLevel.Info, $"Cerrando turno {surtidor.Descripcion}");
            foreach (var manguera in surtidor.mangueras)
            {
                await sendEstado(surtidor.Id, manguera.Ubicacion, "Cerrando", surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
                manguera.Estado = "Totalizadores";
                _logger.Log(NLog.LogLevel.Info, $"Buscando totalizador Manguera {manguera.Descripcion}");
                await totalizadorManguera(surtidor, manguera, stoppingToken);
                _logger.Log(NLog.LogLevel.Info, $"Cerrando {manguera.Descripcion} {surtidor.turno.Id} {manguera.totalizador}");
                _estacionesRepositorio.EnviarTotalizadorCierre(surtidor.Id, surtidor.turno.Id, manguera.Id, manguera.totalizador);
                await sendEstado(surtidor.Id, manguera.Ubicacion, "Cerrada", surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
                _logger.Log(NLog.LogLevel.Info, $"Cerrada {manguera.Descripcion} {surtidor.turno.Id} {manguera.totalizador}");
            }
        }

        private async Task ProcesarTurnoOperativoAsync(SurtidorSiges surtidor, CancellationToken stoppingToken)
        {
            if (!serialPort1.IsOpen)
            {
                _logger.Log(NLog.LogLevel.Warn, $"Puerto {serialPort1.PortName} cerrado durante operación. Reintentando apertura.");
                serialPort1.Open();
                _logger.Log(NLog.LogLevel.Info, $"Puerto {serialPort1.PortName} reabierto correctamente.");
            }
            if (!serialPort1.IsOpen)
            {
                return;
            }

            try
            {
                if (surtidor.mangueras.Any(x => x.Estado == "Desautorizar" || x.Estado == "BuscarBoton" || (x.Estado == "Colgada" && x.Vendiendo)))
                {
                    foreach (var manguera in surtidor.mangueras)
                    {
                        await ProcesarEstadoMangueraAsync(surtidor, manguera, stoppingToken);
                    }
                }
                else
                {
                    await ProcesarManguerasEnReposoAsync(surtidor, stoppingToken);
                    await estado(surtidor, null, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                LogException(ex, $"Error operando surtidor {surtidor.Descripcion}");
                await ResetYDesautorizarTodasMangueras(stoppingToken);
            }
        }

        private async Task ProcesarEstadoMangueraAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            switch (manguera.Estado)
            {
                case "Desautorizar":
                    await ManejarEstadoDesautorizarAsync(surtidor, manguera, stoppingToken);
                    break;
                case "BuscarBoton":
                    await ManejarEstadoBuscarBotonAsync(surtidor, manguera, stoppingToken);
                    break;
                case "Colgada":
                    await ManejarEstadoColgadaAsync(surtidor, manguera, stoppingToken);
                    break;
            }
        }

        private async Task ManejarEstadoDesautorizarAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            await sendEstado(surtidor.Id, manguera.Ubicacion, "Desautorizando", surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
            manguera.Estado = "Desautorizando";
            manguera.Vendiendo = false;
            manguera.Vehiculo = null;
            await desautorizarManguera(surtidor, manguera, stoppingToken);
            manguera.tiempoOcio = 0;
        }

        private async Task ManejarEstadoBuscarBotonAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            _logger.Log(NLog.LogLevel.Info, $"Entrando a ValidarBoton surtidor {surtidor.Numero} manguera {manguera.Ubicacion} puertoIButton {surtidor.PuertoIButton}");
            manguera.Estado = "Desautorizando";
            await desautorizarManguera(surtidor, manguera, stoppingToken);
            manguera.tiempoOcio = 0;
            await sendEstado(surtidor.Id, manguera.Ubicacion, "Validando botón", surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
            await ValidarBoton(surtidor, manguera);

            if (manguera.Vehiculo == null)
            {
                _logger.Log(NLog.LogLevel.Warn, $"ValidarBoton sin vehículo. surtidor {surtidor.Numero} manguera {manguera.Ubicacion} puertoIButton {surtidor.PuertoIButton}");
            }

            if (manguera.Vehiculo != null && manguera.Vehiculo.estado == 0 && manguera.Vehiculo.fechaFin > DateTime.Now)
            {
                await SincronizarBaselinePreVentaAsync(surtidor, manguera, stoppingToken);
                await sendEstado(surtidor.Id, manguera.Ubicacion, "Autorizando - " + manguera.Vehiculo.placa, surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
                manguera.Estado = "Autorizando";
                await autorizarManguera(surtidor, manguera, stoppingToken);
                manguera.Estado = "Vendiendo";
                manguera.Vendiendo = true;
                manguera.Date = DateTime.Now;
                await sendEstado(surtidor.Id, manguera.Ubicacion, "Vendiendo - " + manguera.Vehiculo.placa, surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
                return;
            }

            await sendEstado(surtidor.Id, manguera.Ubicacion, "Desautorizando", surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
            manguera.Vendiendo = false;
            manguera.Vehiculo = null;
            manguera.Estado = "Desautorizando";
            await desautorizarManguera(surtidor, manguera, stoppingToken);
        }

        private async Task SincronizarBaselinePreVentaAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            try
            {
                await LeerUltimaVentaConEsperaAsync(surtidor, manguera, stoppingToken);
                await LeerTotalizadorConEsperaAsync(surtidor, manguera, stoppingToken);

                manguera.NuevaVenta = manguera.ultimaVenta;
                manguera.NuevoTotalizador = manguera.totalizador;

                _logger.Log(NLog.LogLevel.Info, $"Baseline pre-venta sincronizado. Surtidor {surtidor.Numero}, manguera {manguera.Ubicacion}, ultimaVentaBase {manguera.NuevaVenta}, totalizadorBase {manguera.NuevoTotalizador}");
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Warn, $"No fue posible sincronizar baseline pre-venta en surtidor {surtidor.Numero} manguera {manguera.Ubicacion}: {ex.Message}");
                manguera.NuevaVenta = manguera.ultimaVenta;
                manguera.NuevoTotalizador = manguera.totalizador;
            }
        }

        private async Task ManejarEstadoColgadaAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            if (manguera.Vendiendo)
            {
                await ManejarFinVentaMangueraAsync(surtidor, manguera, stoppingToken);
                return;
            }

            manguera.Vendiendo = false;
            manguera.Vehiculo = null;
            manguera.tiempoOcio++;
            if (manguera.Estado == "Colgada" && !manguera.Vendiendo)
            {
                await DesautorizarMangueraEnReposoAsync(surtidor, manguera, stoppingToken);
            }
        }

        private async Task ManejarFinVentaMangueraAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            var operationId = CrearOperacionVentaId(surtidor, manguera);
            _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Inicio fin de venta. Surtidor {surtidor.Numero}, manguera {manguera.Ubicacion}, vendiendo {manguera.Vendiendo}, ultimaVentaMemoria {manguera.NuevaVenta}, totalizadorMemoria {manguera.NuevoTotalizador}");
            await sendEstado(surtidor.Id, manguera.Ubicacion, "Fin venta - " + manguera.Vehiculo.placa, surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
            manguera.Estado = "Desautorizando";
            await desautorizarManguera(surtidor, manguera, stoppingToken);
            manguera.Estado = "BuscandoUltimaVenta";
            manguera.CambioVenta = false;
            await venta(surtidor, manguera, stoppingToken);
            await EsperarCambioVentaAsync(manguera, stoppingToken);
            _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Lectura ultimaVenta completada. ultimaVentaLeida {manguera.ultimaVenta}, ultimaVentaMemoria {manguera.NuevaVenta}");

            if (manguera.Vehiculo != null && manguera.ultimaVenta > 0)
            {
                var resultado = await DeterminarSiDebeVenderAsync(surtidor, manguera, stoppingToken, operationId);
                if (resultado.vender)
                {
                    await sendEstado(surtidor.Id, manguera.Ubicacion, "Total ultima venta " + manguera.ultimaVenta, surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
                    _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Decision GUARDAR venta. motivo {resultado.motivo}, mangueraId {manguera.Id}, idrom {manguera.Vehiculo.idrom}, valorVenta {manguera.ultimaVenta}, totalizadorConfirmado {manguera.totalizador}");
                    _estacionesRepositorio.AgregarVenta(manguera.Id, manguera.ultimaVenta, manguera.Vehiculo.idrom);
                    await Fidelizar(manguera.Id);
                    manguera.NuevaVenta = manguera.ultimaVenta;
                }
                else
                {
                    _logger.Log(NLog.LogLevel.Warn, $"[{operationId}] Decision BLOQUEAR venta. motivo {resultado.motivo}. Surtidor {surtidor.Numero}, manguera {manguera.Ubicacion}, ultimaVenta {manguera.ultimaVenta}, referenciaNuevaVenta {manguera.NuevaVenta}, totalizadorActual {manguera.totalizador}, referenciaTotalizador {manguera.NuevoTotalizador}");
                }
            }
            else
            {
                _logger.Log(NLog.LogLevel.Warn, $"[{operationId}] Venta no evaluada por precondiciones. VehiculoNull {manguera.Vehiculo == null}, ultimaVentaLeida {manguera.ultimaVenta}");
            }

            manguera.Estado = "FinVenta";
            manguera.Vendiendo = false;
            manguera.Vehiculo = null;
            _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Fin de operacion de venta. EstadoFinal {manguera.Estado}, vendiendo {manguera.Vendiendo}");
        }

        private async Task<(bool vender, string motivo)> DeterminarSiDebeVenderAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken, string operationId)
        {
            var totalizadorAnterior = manguera.NuevoTotalizador;
            var ultimaVentaAnterior = manguera.NuevaVenta;
            var ultimaVentaObjetivo = manguera.ultimaVenta;
            const int maxIntentosConfirmacion = 5;

            _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Inicia validacion de venta. totalizadorAnterior {totalizadorAnterior}, ultimaVentaAnterior {ultimaVentaAnterior}, ultimaVentaObjetivo {ultimaVentaObjetivo}");

            if (ultimaVentaObjetivo < ultimaVentaAnterior)
            {
                return (false, "ultima_venta_decreciente");
            }

            if (ultimaVentaObjetivo > ultimaVentaAnterior)
            {
                _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Venta validada por cambio de ultimaVenta sin validacion de totalizador. ultimaVentaAnterior {ultimaVentaAnterior}, ultimaVentaObjetivo {ultimaVentaObjetivo}");
                return (true, "ultima_venta_distinta");
            }

            _logger.Log(NLog.LogLevel.Warn, $"[{operationId}] Ultima venta igual a la referencia ({ultimaVentaObjetivo}). Se valida por totalizador para evitar duplicado.");

            for (var intento = 1; intento <= maxIntentosConfirmacion; intento++)
            {
                await LeerTotalizadorConEsperaAsync(surtidor, manguera, stoppingToken);
                var cambioTotalizador = CambioTotalizador(totalizadorAnterior, manguera.totalizador);
                _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Intento {intento}/{maxIntentosConfirmacion} validacion totalizador. totalizadorLeido {manguera.totalizador}, cambioTotalizador {cambioTotalizador}");

                if (cambioTotalizador)
                {
                    await EsperarEstabilidadTotalizadorAsync(surtidor, manguera, stoppingToken, operationId);
                    if (CambioTotalizador(totalizadorAnterior, manguera.totalizador))
                    {
                        manguera.NuevoTotalizador = manguera.totalizador;
                        _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Venta validada por cambio de totalizador. NuevoTotalizadorMemoria {manguera.NuevoTotalizador}, ultimaVentaActual {manguera.ultimaVenta}");
                        return (true, "totalizador_confirmado");
                    }
                }

                if (intento < maxIntentosConfirmacion)
                {
                    await Task.Delay(500, stoppingToken);
                }
            }

            return (false, "mismo_valor_sin_confirmacion_totalizador");
        }

        private static bool CambioTotalizador(double valorAnterior, double valorActual)
        {
            return Math.Abs(valorActual - valorAnterior) > 0.0001;
        }

        private static bool CambioVenta(double valorAnterior, double valorActual)
        {
            return Math.Abs(valorActual - valorAnterior) > 0.0001;
        }

        private async Task LeerTotalizadorConEsperaAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            manguera.Estado = "Totalizadores";
            manguera.CambioVenta = false;
            await totalizadorManguera(surtidor, manguera, stoppingToken);
            await EsperarCambioVentaAsync(manguera, stoppingToken);
        }

        private async Task LeerUltimaVentaConEsperaAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            manguera.Estado = "BuscandoUltimaVenta";
            manguera.CambioVenta = false;
            await venta(surtidor, manguera, stoppingToken);
            await EsperarCambioVentaAsync(manguera, stoppingToken);
        }

        private async Task EsperarEstabilidadTotalizadorAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken, string operationId)
        {
            var totalizador = 0d;
            const int maxIntentosEstabilidad = 4;
            for (var intentos = 1; intentos <= maxIntentosEstabilidad; intentos++)
            {
                await LeerTotalizadorConEsperaAsync(surtidor, manguera, stoppingToken);
                _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Lectura estabilizacion totalizador intento {intentos}/{maxIntentosEstabilidad}. totalizadorLeido {manguera.totalizador}, totalizadorPrevioIntento {totalizador}");
                if (!CambioTotalizador(totalizador, manguera.totalizador))
                {
                    _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Totalizador estabilizado en intento {intentos}. totalizadorEstable {manguera.totalizador}");
                    return;
                }
                totalizador = manguera.totalizador;
            }

            _logger.Log(NLog.LogLevel.Warn, $"[{operationId}] Totalizador no estabilizo dentro del maximo de intentos. Se continua con ultimo valor leido {manguera.totalizador}");
        }

        private static string CrearOperacionVentaId(SurtidorSiges surtidor, MangueraSiges manguera)
        {
            return $"VEN-{DateTime.UtcNow:yyyyMMddHHmmssfff}-S{surtidor.Numero}-M{manguera.Id}";
        }

        private async Task ProcesarManguerasEnReposoAsync(SurtidorSiges surtidor, CancellationToken stoppingToken)
        {
            foreach (var manguera in surtidor.mangueras)
            {
                manguera.tiempoOcio++;
                if (manguera.Estado == "Colgada" && !manguera.Vendiendo)
                {
                    await DesautorizarMangueraEnReposoAsync(surtidor, manguera, stoppingToken);
                }
            }
        }

        private async Task DesautorizarMangueraEnReposoAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            await sendEstado(surtidor.Id, manguera.Ubicacion, "Desautorizando", surtidor.turno.FechaApertura.ToString(), surtidor.turno.Empleado);
            manguera.Estado = "Desautorizando";
            await desautorizarManguera(surtidor, manguera, stoppingToken);
            manguera.tiempoOcio = 0;
        }

        private async Task Fidelizar(int id)
        {
            try
            {
                if (_fidelizacion == null)
                {
                    _logger.Log(NLog.LogLevel.Warn, "Fidelizacion no configurada. Se omite proceso de fidelizacion.");
                    return;
                }

                //Verificar si tercero fidelizado venta manguera get Punto
                var puntos = _estacionesRepositorio.GetVentaFidelizarAutomatica(id);
                if (puntos != null)
                {
                    await _fidelizacion.SubirPuntops(puntos.ValorVenta, puntos.DocumentoFidelizado, puntos.Factura);
                    var fidelizados = await _fidelizacion.GetFidelizados(puntos.DocumentoFidelizado);
                    if (fidelizados == null)
                    {
                        _logger.Log(NLog.LogLevel.Info, $"Sin fidelizados para documento {puntos.DocumentoFidelizado}");
                        return;
                    }

                    foreach (var fidelizado in fidelizados)
                    {
                        _estacionesRepositorio.AddFidelizado(fidelizado.Documento, fidelizado.Puntos??0);
                    }
                }
            }
            catch (Exception ex )
            {
                LogException(ex, $"Error en fidelización de manguera {id}");
            }
        }

        private async Task ValidarBoton(SurtidorSiges surtidor, MangueraSiges manguera)
        {

            var intentos = 0;
            VehiculoSuic? vehiculo = null;
            var lecturaIButtonExitosa = false;

            surtidor = Surtidores.First(x => x.Id == surtidor.Id);
            manguera = surtidor.mangueras.First(x => x.Id == manguera.Id);
            while (++intentos <= 3)
            {
                try
                {
                    var puertos = SerialPort.GetPortNames();
                    _logger.Log(NLog.LogLevel.Info, $"ValidarBoton intento {intentos}. Puerto configurado: {surtidor.PuertoIButton}. Puertos detectados: {string.Join(",", puertos)}");
                    var (vehiculoDetectado, iButtonValido) = await ObtenerVehiculoPorBotonAsync(surtidor, manguera);
                    vehiculo = vehiculoDetectado;
                    lecturaIButtonExitosa = lecturaIButtonExitosa || iButtonValido;
                    if (vehiculo != null)
                    {
                        vehiculo.surtidor = surtidor.Numero;
                        vehiculo.isla = surtidor.turno.Isla;
                        await sendVehiculo(vehiculo);
                        _logger.Log(NLog.LogLevel.Info, $"ValidarBoton exitoso. Vehículo {vehiculo.placa} surtidor {surtidor.Numero} manguera {manguera.Ubicacion}");
                        break;
                    }

                }
                catch (Exception ex)
                {
                    LogException(ex, "Error validando botón");
                }
            }
            if (vehiculo == null)
            {
                if (lecturaIButtonExitosa)
                {
                    _logger.Log(NLog.LogLevel.Warn, $"ValidarBoton leyó iButton, pero no encontró vehículo asociado (SICOM/local). surtidor {surtidor.Numero} manguera {manguera.Ubicacion} puertoIButton {surtidor.PuertoIButton}");
                }
                else
                {
                    _logger.Log(NLog.LogLevel.Warn, $"ValidarBoton agotó intentos sin lectura válida de iButton. surtidor {surtidor.Numero} manguera {manguera.Ubicacion} puertoIButton {surtidor.PuertoIButton}");
                }
            }
            manguera.Vehiculo = vehiculo;
        }

        private async Task<(VehiculoSuic? vehiculo, bool iButtonValido)> ObtenerVehiculoPorBotonAsync(SurtidorSiges surtidor, MangueraSiges manguera)
        {
            var boton = await lectorIButton.leerBoton(surtidor.PuertoIButton, surtidor.Numero, manguera.Ubicacion == "Par", _logger);
            _logger.Log(NLog.LogLevel.Info, $"Resultado lectura iButton en {surtidor.PuertoIButton}: {boton}");
            if (string.IsNullOrWhiteSpace(boton) || boton == "fail")
            {
                return (null, false);
            }

            VehiculoSuic? vehiculoOnline = null;
            if (_sicom.ValidarOnline)
            {
                vehiculoOnline = await _sicomConection.validateIButton(boton);
            }

            var vehiculoLocal = _estacionesRepositorio.GetVehiculoSuic(boton);
            var vehiculoCombinado = CombinarVehiculo(vehiculoOnline, vehiculoLocal);
            if (vehiculoCombinado == null)
            {
                _logger.Log(NLog.LogLevel.Warn, $"iButton leído sin coincidencia de vehículo. iButton: {boton}, surtidor: {surtidor.Numero}, manguera: {manguera.Ubicacion}");
            }
            return (vehiculoCombinado, true);
        }

        private VehiculoSuic? CombinarVehiculo(VehiculoSuic? vehiculoOnline, VehiculoSuic? vehiculoLocal)
        {
            if (vehiculoOnline == null)
            {
                return vehiculoLocal;
            }

            if (vehiculoLocal != null && vehiculoOnline.fechaFin == DateTime.MinValue)
            {
                vehiculoOnline.fechaFin = vehiculoLocal.fechaFin;
            }

            return vehiculoOnline;
        }

        private async Task estado(SurtidorSiges surtidor, MangueraSiges? manguera, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            //_logger.Log(NLog.LogLevel.Info, "PreguntandoEstados ");
            string trama = $"0230303{surtidor.Numero}{(146 + surtidor.Numero).ToString("X")}";
            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken);
        }

        private async Task venta(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {

            serialPort1.ReceivedBytesThreshold = 54;
            string trama = $"0130303{surtidor.Numero}{(145 + surtidor.Numero).ToString("X")}";

            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken);
        }

        private async Task autorizarManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            //_logger.Log(NLog.LogLevel.Info, $"Preguntando autorización {manguera.Ubicacion}");
            string trama = $"1830303{surtidor.Numero}0{ubicacion}00{(168 + surtidor.Numero + ubicacion).ToString("X")}";//Desautorizar

            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken, false);


        }

        private async Task desautorizarManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken, bool debeEsperar = true)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            // _logger.Log(NLog.LogLevel.Info, $"Preguntando Desautorizando {manguera.Ubicacion}");
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string trama = $"1830303{surtidor.Numero}0{ubicacion}FF{(167 + surtidor.Numero + ubicacion).ToString("X")}";//Desautorizar

            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken);

        }


        private async Task totalizadorManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            serialPort1.ReceivedBytesThreshold = 10;
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string trama = $"1630303{surtidor.Numero}0{ubicacion}{(166 + surtidor.Numero + ubicacion).ToString("X")}";//totalizador

            // _logger.Log(NLog.LogLevel.Info, $"Preguntando totalizador {manguera.Ubicacion} {trama}");
            await EnviarTramaAsync(surtidor, manguera, trama, stoppingToken);
        }
        private async Task EnviarTramaAsync(SurtidorSiges s, MangueraSiges? manguera, string trama, CancellationToken stoppingToken, bool debeEsperar = true)
        {
            var surtidor = Surtidores.FirstOrDefault(x => x.Numero == s.Numero);
            if (surtidor == null)
            {
                _logger.Log(NLog.LogLevel.Warn, $"No se encontró surtidor número {s.Numero} para enviar trama {trama}.");
                return;
            }
            surtidor.esperando = true;
            respondio = false;
            finalizo = false;
            _surtidorEsperaId = surtidor.Id;
            _mangueraEsperaId = manguera?.Id;
            if (manguera != null)
            {
                var mang = surtidor.mangueras.FirstOrDefault(x => x.Id == manguera.Id);
                if (mang != null)
                {
                    mang.esperando = true;
                }
                else
                {
                    _logger.Log(NLog.LogLevel.Warn, $"No se encontró manguera id {manguera.Id} en surtidor {surtidor.Numero} para trama {trama}.");
                }
            }
            byte[] tramaByte = FromHex(trama);
            _logger.Log(NLog.LogLevel.Info, $"Enviando trama {trama} a surtidor {surtidor.Numero} manguera {(manguera?.Ubicacion ?? "N/A")} - Threshold {serialPort1.ReceivedBytesThreshold}");
            count = 0;
            var inicioEsperaRespuesta = DateTime.Now;
            var bufferLimpiado = false;
            while (!respondio && (debeEsperar || ++count < 6))
            {
                if (!serialPort1.IsOpen)
                {
                    _logger.Log(NLog.LogLevel.Error, $"No se puede enviar trama {trama}: puerto {serialPort1.PortName} está cerrado.");
                    break;
                }
                if (!bufferLimpiado)
                {
                    try
                    {
                        serialPort1.DiscardInBuffer();
                        bufferLimpiado = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(NLog.LogLevel.Warn, $"No fue posible limpiar buffer de entrada del puerto {serialPort1.PortName} antes de enviar trama {trama}: {ex.Message}");
                    }
                }
                serialPort1.Write(tramaByte, 0, tramaByte.Length); //ENVIO DE LA TRAMA
                await Task.Delay(400, stoppingToken);
                if ((DateTime.Now - inicioEsperaRespuesta).TotalSeconds > 12)
                {
                    _logger.Log(NLog.LogLevel.Error, $"Timeout esperando respuesta de trama {trama} en surtidor {surtidor.Numero}.");
                    break;
                }
            }
            if (!respondio)
            {
                _logger.Log(NLog.LogLevel.Warn, $"Sin respuesta serial para trama {trama} en surtidor {surtidor.Numero}. Estado manguera: {manguera?.Estado}");
            }

            var inicioEsperaFinal = DateTime.Now;
            while (!finalizo)
            {
                if ((DateTime.Now - inicioEsperaFinal).TotalSeconds > 15)
                {
                    _logger.Log(NLog.LogLevel.Error, $"Timeout esperando finalización de lectura serial para trama {trama} en surtidor {surtidor.Numero}.");
                    surtidor.esperando = false;
                    if (manguera != null)
                    {
                        var mang = surtidor.mangueras.FirstOrDefault(x => x.Id == manguera.Id);
                        if (mang != null)
                            mang.esperando = false;
                    }
                    break;
                }
                await Task.Delay(400, stoppingToken);
            }

            _surtidorEsperaId = null;
            _mangueraEsperaId = null;
        }

        public void DataReceiverHandler(object sender,
            SerialDataReceivedEventArgs e)
        {
            var huboLecturaProcesada = false;
            try
            {

                SerialPort sp = (SerialPort)sender;
                _logger.Log(NLog.LogLevel.Info, $"DataReceived en {sp.PortName}. BytesToRead: {sp.BytesToRead}, Threshold: {sp.ReceivedBytesThreshold}");

                var surtidor = ObtenerSurtidorEnEspera();
                if (surtidor == null)
                {
                    var descartados = sp.ReadExisting();
                    _logger.Log(NLog.LogLevel.Warn, $"DataReceived sin surtidor en espera. Se descarta lectura de {descartados.Length} caracteres.");
                    return;
                }
                surtidor.esperando = false;
                var manguera = ObtenerMangueraEnEspera(surtidor);

                if (manguera != null)
                {
                    manguera.esperando = false;
                }
                NormalizarContadorRespuesta(surtidor);

                if (manguera != null)
                {
                    ProcesarDataManguera(sp, surtidor, manguera);
                }
                else
                {
                    VerificarEstado(sp, surtidor);
                }

                huboLecturaProcesada = true;

            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, $"Error en DataReceiverHandler: {ex}");
                if (_surtidorEsperaId.HasValue)
                {
                    respondio = true;
                    finalizo = true;
                }
            }

            if (huboLecturaProcesada)
            {
                respondio = true;
                finalizo = true;
            }



        }

        private SurtidorSiges? ObtenerSurtidorEnEspera()
        {
            if (_surtidorEsperaId.HasValue)
            {
                return Surtidores.FirstOrDefault(x => x.Id == _surtidorEsperaId.Value && x.esperando);
            }

            return Surtidores.FirstOrDefault(x => x.esperando);
        }

        private MangueraSiges? ObtenerMangueraEnEspera(SurtidorSiges surtidor)
        {
            if (_mangueraEsperaId.HasValue)
            {
                return surtidor.mangueras.FirstOrDefault(x => x.Id == _mangueraEsperaId.Value && x.esperando);
            }

            return surtidor.mangueras.FirstOrDefault(x => x.esperando);
        }

        private void NormalizarContadorRespuesta(SurtidorSiges surtidor)
        {
            if (++count == 500)
            {
                count = 0;
                respondio = true;
                surtidor.esperando = false;
            }
        }

        private void ProcesarDataManguera(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {
            switch (manguera.Estado)
            {
                case "Desautorizando":
                    GetTRamaDesautorizar(sp, surtidor, manguera);
                    break;
                case "Autorizando":
                    GetTRamaAutorizar(sp, surtidor, manguera);
                    break;
                case "BuscandoUltimaVenta":
                    manguera.ultimaVenta = GetTRamaVenta(sp, surtidor, manguera);
                    manguera.CambioVenta = true;
                    break;
                case "Totalizadores":
                    manguera.totalizador = GetTRamaTotalizador(sp, surtidor, manguera);
                    manguera.CambioVenta = true;
                    break;
            }
        }

        private void GetTRamaAutorizar(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {
            DrenarRespuestaCorta(sp, "autorizar", surtidor, manguera);
        }

        private double GetTRamaTotalizador(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {

            surtidor = Surtidores.First(x => x.Id == surtidor.Id);
            manguera = surtidor.mangueras.First(x => x.Id == manguera.Id);
            var hexString = "";
            var expectedHeader = $"1630303{surtidor.Numero}";
            const string fallbackHeader = "163030";
            while ((!hexString.Contains(expectedHeader) && !hexString.Contains(fallbackHeader)) || hexString.Length < 37)
            {
                if (sp.BytesToRead > 0)
                {
                    string intdata = sp.ReadExisting();
                    byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                    hexString += BitConverter.ToString(response);
                    if (hexString.Contains("-"))
                        hexString = hexString.Replace("-", "");
                    if (hexString.Contains("3F"))
                        hexString = hexString.Replace("3F", "");

                    if (hexString.Contains(expectedHeader))
                        hexString = hexString.Substring(hexString.LastIndexOf(expectedHeader, StringComparison.Ordinal));
                    else if (hexString.Contains(fallbackHeader))
                        hexString = hexString.Substring(hexString.LastIndexOf(fallbackHeader, StringComparison.Ordinal));
                }
                Thread.Sleep(250);
            }

            //_logger.Log(NLog.LogLevel.Info, $"estado bruto {hexString}");
            respondio = true;
            surtidor.esperando = false;
            count = 0;
            string totalventa = "";
            var iniciolector = 27;
            totalventa += hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa +=  hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1);



            return double.Parse(totalventa)/100;



        }

        private void VerificarEstado(SerialPort sp, SurtidorSiges surtidor)
        {
            surtidor = Surtidores.First(x => x.Id == surtidor.Id);
            var hexString = "";
            var expectedHeader = $"0230303{surtidor.Numero}";
            const string fallbackHeader = "023030";
            while ((!hexString.Contains(expectedHeader) && !hexString.Contains(fallbackHeader)) || hexString.Length < 15)
            {
                if (sp.BytesToRead > 0)
                {
                    string intdata = sp.ReadExisting();
                    byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                    hexString += BitConverter.ToString(response);
                    if (hexString.Contains("-"))
                        hexString = hexString.Replace("-", "");
                    if (hexString.Contains("3F"))
                        hexString = hexString.Replace("3F", "");

                    if (hexString.Contains(expectedHeader))
                        hexString = hexString.Substring(hexString.LastIndexOf(expectedHeader, StringComparison.Ordinal));
                    else if (hexString.Contains(fallbackHeader))
                        hexString = hexString.Substring(hexString.LastIndexOf(fallbackHeader, StringComparison.Ordinal));
                }
                Thread.Sleep(250);
            }
            // _logger.Log(NLog.LogLevel.Info, $"estado bruto {hexString}");
            count = 0;
            var mangueraPar = surtidor.mangueras.FirstOrDefault(x => x.Ubicacion == "Par");
            var mangueraImpar = surtidor.mangueras.FirstOrDefault(x => x.Ubicacion == "Impar");
            if (mangueraPar == null || mangueraImpar == null)
            {
                _logger.Log(NLog.LogLevel.Warn, $"Surtidor {surtidor.Numero} no tiene mangueras Par/Impar configuradas para verificar estado.");
                return;
            }
            string estadoImPar = hexString.Substring(12, 2);
            string estadoPar = hexString.Substring(14, 2);
            // _logger.Log(NLog.LogLevel.Info, $"estado impar {estadoImPar}.");
            // _logger.Log(NLog.LogLevel.Info, $"estado par {estadoPar}.");

            if ((estadoPar.Contains("00") || estadoPar.Contains("20")) && mangueraPar.Vendiendo)
            {
                var reposos = IncrementarReposoConsecutivo(mangueraPar.Id);
                if (reposos >= MIN_REPOSO_CONSECUTIVO_FIN_VENTA)
                {
                    _logger.Log(NLog.LogLevel.Warn, $"Manguera Par en vendiendo confirmó estado de reposo {estadoPar} ({reposos}/{MIN_REPOSO_CONSECUTIVO_FIN_VENTA}). Se fuerza Colgada para ejecutar fin de venta en surtidor {surtidor.Numero}.");
                    mangueraPar.Estado = "Colgada";
                    ReiniciarReposoConsecutivo(mangueraPar.Id);
                }
            }
            else if ((estadoPar.Contains("00") || estadoPar.Contains("20")) && !mangueraPar.Vendiendo)
            {
                ReiniciarReposoConsecutivo(mangueraPar.Id);
                mangueraPar.Estado = "Desautorizar";
            }
            if (estadoPar.Contains("B2") && !mangueraPar.Vendiendo)
            {
                ReiniciarReposoConsecutivo(mangueraPar.Id);
                if (mangueraPar.Estado != "BuscarBoton")
                {
                    _logger.Log(NLog.LogLevel.Info, $"Cambio estado manguera Par a BuscarBoton en surtidor {surtidor.Numero}");
                }
                mangueraPar.Estado = "BuscarBoton";
            }
            if (estadoPar.Contains("80"))
            {
                ReiniciarReposoConsecutivo(mangueraPar.Id);
                mangueraPar.Estado = "Colgada";
            }

            if ((estadoImPar.Contains("00") || estadoImPar.Contains("20")) && mangueraImpar.Vendiendo)
            {
                var reposos = IncrementarReposoConsecutivo(mangueraImpar.Id);
                if (reposos >= MIN_REPOSO_CONSECUTIVO_FIN_VENTA)
                {
                    _logger.Log(NLog.LogLevel.Warn, $"Manguera Impar en vendiendo confirmó estado de reposo {estadoImPar} ({reposos}/{MIN_REPOSO_CONSECUTIVO_FIN_VENTA}). Se fuerza Colgada para ejecutar fin de venta en surtidor {surtidor.Numero}.");
                    mangueraImpar.Estado = "Colgada";
                    ReiniciarReposoConsecutivo(mangueraImpar.Id);
                }
            }
            else if ((estadoImPar.Contains("00") || estadoImPar.Contains("20")) && mangueraImpar.Estado != "Vendiendo")
            {
                ReiniciarReposoConsecutivo(mangueraImpar.Id);
                mangueraImpar.Estado = "Desautorizar";
            }
            if (estadoImPar.Contains("B2") && !mangueraImpar.Vendiendo)
            {
                ReiniciarReposoConsecutivo(mangueraImpar.Id);
                if (mangueraImpar.Estado != "BuscarBoton")
                {
                    _logger.Log(NLog.LogLevel.Info, $"Cambio estado manguera Impar a BuscarBoton en surtidor {surtidor.Numero}");
                }
                mangueraImpar.Estado = "BuscarBoton";
            }
            if (estadoImPar.Contains("80"))
            {
                ReiniciarReposoConsecutivo(mangueraImpar.Id);
                mangueraImpar.Estado = "Colgada";
            }

        }

        private int IncrementarReposoConsecutivo(int mangueraId)
        {
            if (!_reposoConsecutivoVendiendo.ContainsKey(mangueraId))
            {
                _reposoConsecutivoVendiendo[mangueraId] = 0;
            }

            _reposoConsecutivoVendiendo[mangueraId]++;
            return _reposoConsecutivoVendiendo[mangueraId];
        }

        private void ReiniciarReposoConsecutivo(int mangueraId)
        {
            if (_reposoConsecutivoVendiendo.ContainsKey(mangueraId))
            {
                _reposoConsecutivoVendiendo[mangueraId] = 0;
            }
        }

        private double GetTRamaVenta(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {
            surtidor = Surtidores.First(x => x.Id == surtidor.Id);
            manguera = surtidor.mangueras.First(x => x.Id == manguera.Id);
            var hexString = "";
            var expectedHeader = $"0130303{surtidor.Numero}";
            const string fallbackHeader = "0130303";
            while ((!hexString.Contains(expectedHeader) && !hexString.Contains(fallbackHeader)) || hexString.Length < 54)
            {
                if (sp.BytesToRead > 0)
                {
                    string intdata = sp.ReadExisting();
                    byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                    hexString += BitConverter.ToString(response);
                    if (hexString.Contains("-"))
                        hexString = hexString.Replace("-", "");

                    if (hexString.Contains(expectedHeader))
                        hexString = hexString.Substring(hexString.LastIndexOf(expectedHeader, StringComparison.Ordinal));
                    else if (hexString.Contains(fallbackHeader))
                        hexString = hexString.Substring(hexString.LastIndexOf(fallbackHeader, StringComparison.Ordinal));
                }
                Thread.Sleep(250);
            }
            // _logger.Log(NLog.LogLevel.Info, $"venta bruto {hexString}");
            count = 0;
            string totalventa = "";
            //0130303130303030303030303030303F31303639323F3F34383603013030313030303030303030303030 
            var iniciolector = manguera.Ubicacion == "Par" ? 43 : 21;

            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1) == "F" ? "0" : hexString.Substring(iniciolector, 1);
            iniciolector += 2;

            //_logger.Log(NLog.LogLevel.Info, $"venta bruto {totalventa}");
            return double.Parse(totalventa) / 100;

        }

        private void GetTRamaDesautorizar(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {
            surtidor = Surtidores.First(x => x.Id == surtidor.Id);
            manguera = surtidor.mangueras.First(x => x.Id == manguera.Id);
            DrenarRespuestaCorta(sp, "desautorizar", surtidor, manguera);
            manguera.Estado = "Desautorizada";
        }

        private void DrenarRespuestaCorta(SerialPort sp, string operacion, SurtidorSiges surtidor, MangueraSiges manguera)
        {
            var acumulado = string.Empty;
            var inicio = DateTime.UtcNow;

            while ((DateTime.UtcNow - inicio).TotalMilliseconds < 350)
            {
                if (sp.BytesToRead > 0)
                {
                    acumulado += sp.ReadExisting();
                    Thread.Sleep(35);
                    continue;
                }

                if (!string.IsNullOrEmpty(acumulado))
                {
                    break;
                }

                Thread.Sleep(20);
            }

            if (!string.IsNullOrEmpty(acumulado))
            {
                var bytes = Encoding.GetEncoding(28591).GetBytes(acumulado);
                var hex = BitConverter.ToString(bytes).Replace("-", string.Empty);
                _logger.Log(NLog.LogLevel.Info, $"Respuesta serial {operacion} capturada. Surtidor {surtidor.Numero}, manguera {manguera.Ubicacion}, bytes {bytes.Length}, hex {hex}");
            }
        }

        public byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }


        private async Task sendEstado(int id, string ubicacion, string estado, string  turno, string empleado)
        {
            try
            {
                await _messageProducer.SendMessage(new Mensaje()
                {
                    SurtidorId = id,
                    Estado = estado,
                    Ubicacion = ubicacion,
                    Turno = turno,
                    Empleado = empleado
                }, "controlador");
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Warn, $"No fue posible publicar estado '{estado}' surtidor {id} manguera {ubicacion}: {ex.Message}");
            }
        }

        private async Task sendVehiculo(VehiculoSuic vehiculoSuic)
        {
            try
            {
                _logger.Log(NLog.LogLevel.Info, $"Enviando vehículo a cola VehiculosSICOM. idrom: {vehiculoSuic.idrom}, placa: {vehiculoSuic.placa}, surtidor: {vehiculoSuic.surtidor}, isla: {vehiculoSuic.isla}");
                await _messageProducer.SendMessage(vehiculoSuic, "VehiculosSICOM");
                await _messageProducer.SendMessage(vehiculoSuic, "VehiculosSICOM");
                _logger.Log(NLog.LogLevel.Info, $"Vehículo enviado a cola VehiculosSICOM (2 publicaciones). idrom: {vehiculoSuic.idrom}, placa: {vehiculoSuic.placa}");
            }
            catch (Exception ex)
            {
                LogException(ex, "Error enviando vehículo");
            }
        }

        private async Task EsperarCambioVentaAsync(MangueraSiges manguera, CancellationToken stoppingToken)
        {
            while (!manguera.CambioVenta && !stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
        }

        private async Task ResetYDesautorizarTodasMangueras(CancellationToken stoppingToken)
        {
            foreach (var surtidor in Surtidores)
            {
                foreach (var manguera in surtidor.mangueras)
                {
                    manguera.Vendiendo = false;
                    manguera.Vehiculo = null;
                    await desautorizarManguera(surtidor, manguera, stoppingToken);
                }
            }
        }

        private void LogException(Exception ex, string contexto)
        {
            _logger.Log(NLog.LogLevel.Error, $"{contexto}. Error: {ex.Message}");
            _logger.Log(NLog.LogLevel.Error, $"StackTrace: {ex.StackTrace}");
        }
    }
}
