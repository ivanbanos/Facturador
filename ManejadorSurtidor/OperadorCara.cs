using System.IO.Ports;
using System.Text;
using FacturadorEstacionesRepositorio;
using FactoradorEstacionesModelo.Siges;
using ManejadorSurtidor.SICOM;
using ManejadorSurtidor.Protocols;
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
        private readonly IPumpProtocol _pumpProtocol;
        IEnumerable<SurtidorSiges> Surtidores;
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private LectorIButton lectorIButton;
        private readonly IMessageProducer _messageProducer;
        private readonly Sicom _sicom;
        private readonly ISicomConection _sicomConection;
        private readonly IFidelizacion _fidelizacion;
        private readonly Islas _islas;
        private readonly Dictionary<int, int> _reposoConsecutivoValidarFinVenta = new Dictionary<int, int>();
        private const int MIN_REPOSO_CONSECUTIVO_VALIDAR_FIN = 5;
        public OperadorCara(
            Logger logger,
            IEnumerable<SurtidorSiges> surtidores,
            IEstacionesRepositorio estacionesRepositorio,
            IOptions<Sicom> options,
            ISicomConection sicomConection,
            IMessageProducer messageProducer,
            IFidelizacion fidelizacion,
            Islas islas,
            IPumpProtocol pumpProtocol)
        {
            _sicomConection = sicomConection;
            _logger = logger;
            _estacionesRepositorio = estacionesRepositorio;
            _sicom = options.Value;
            _messageProducer = messageProducer;
            _pumpProtocol = pumpProtocol;
            lectorIButton = new LectorIButton();
            Surtidores = surtidores;
            
            _pumpProtocol.DataReceived += OnProtocolDataReceived;
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
                _logger.Log(NLog.LogLevel.Info, "Inicializando protocolo de comunicación con surtidores");
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
            try
            {
                if (surtidor.mangueras.Any(x => x.Estado == "Desautorizar" || x.Estado == "BuscarBoton" || x.Estado == "ValidarFinVenta" || (x.Estado == "Colgada" && x.Vendiendo)))
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
                case "ValidarFinVenta":
                    await ManejarEstadoValidarFinVentaAsync(surtidor, manguera, stoppingToken);
                    break;
                case "Colgada":
                    await ManejarEstadoColgadaAsync(surtidor, manguera, stoppingToken);
                    break;
            }
        }

        private async Task ManejarEstadoValidarFinVentaAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            if (!manguera.Vendiendo)
            {
                manguera.Estado = "Desautorizar";
                ReiniciarReposoValidarFinVenta(manguera.Id);
                return;
            }

            var operationId = CrearOperacionVentaId(surtidor, manguera);
            _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Inicio validación segura de fin de venta por reposo. Surtidor {surtidor.Numero}, manguera {manguera.Ubicacion}, referenciaUltimaVenta {manguera.NuevaVenta}, referenciaTotalizador {manguera.NuevoTotalizador}");

            await LeerUltimaVentaConEsperaAsync(surtidor, manguera, stoppingToken);
            var ultimaVenta1 = manguera.ultimaVenta;
            await LeerTotalizadorConEsperaAsync(surtidor, manguera, stoppingToken);
            var totalizador1 = manguera.totalizador;

            await Task.Delay(1200, stoppingToken);

            await LeerUltimaVentaConEsperaAsync(surtidor, manguera, stoppingToken);
            var ultimaVenta2 = manguera.ultimaVenta;
            await LeerTotalizadorConEsperaAsync(surtidor, manguera, stoppingToken);
            var totalizador2 = manguera.totalizador;

            var ventaEstable = !CambioVenta(ultimaVenta1, ultimaVenta2);
            var totalizadorEstable = !CambioTotalizador(totalizador1, totalizador2);
            var evidenciaDeVenta = CambioVenta(ultimaVenta2, manguera.NuevaVenta) || CambioTotalizador(totalizador2, manguera.NuevoTotalizador);

            if (ventaEstable && totalizadorEstable && evidenciaDeVenta)
            {
                _logger.Log(NLog.LogLevel.Warn, $"[{operationId}] Fin de venta confirmado por estabilidad en reposo. ultimaVenta1 {ultimaVenta1}, ultimaVenta2 {ultimaVenta2}, totalizador1 {totalizador1}, totalizador2 {totalizador2}");
                manguera.Estado = "Colgada";
                ReiniciarReposoValidarFinVenta(manguera.Id);
                return;
            }

            _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Validación de fin descartada. Continúa vendiendo. ventaEstable {ventaEstable}, totalizadorEstable {totalizadorEstable}, evidenciaDeVenta {evidenciaDeVenta}, ultimaVenta1 {ultimaVenta1}, ultimaVenta2 {ultimaVenta2}, totalizador1 {totalizador1}, totalizador2 {totalizador2}");
            manguera.Estado = "Vendiendo";
            ReiniciarReposoValidarFinVenta(manguera.Id);
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
                    try
                    {
                        _estacionesRepositorio.AgregarVenta(manguera.Id, manguera.ultimaVenta, manguera.Vehiculo.idrom);
                        _logger.Log(NLog.LogLevel.Info, $"[{operationId}] Venta persistida en BD. mangueraId {manguera.Id}, idrom {manguera.Vehiculo.idrom}, valorVenta {manguera.ultimaVenta}");
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(NLog.LogLevel.Error, $"[{operationId}] Error persistiendo venta en BD. mangueraId {manguera.Id}, idrom {manguera.Vehiculo.idrom}, valorVenta {manguera.ultimaVenta}. Error: {ex.Message}");
                        throw;
                    }
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

            if (CambioVenta(ultimaVentaObjetivo, ultimaVentaAnterior))
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
            var statuses = await _pumpProtocol.GetStatusAsync(surtidor, stoppingToken);
            ProcessHoseStatusChanges(surtidor, statuses);
        }

        private async Task venta(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            manguera.ultimaVenta = await _pumpProtocol.ReadLastSaleAsync(surtidor, manguera, stoppingToken);
            manguera.CambioVenta = true;
        }

        private async Task autorizarManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            await _pumpProtocol.AuthorizeHoseAsync(surtidor, manguera, stoppingToken);
        }

        private async Task desautorizarManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken, bool debeEsperar = true)
        {
            await _pumpProtocol.DeauthorizeHoseAsync(surtidor, manguera, stoppingToken, debeEsperar);
            manguera.Estado = "Desautorizada";
        }

        private async Task totalizadorManguera(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            manguera.totalizador = await _pumpProtocol.ReadTotalizerAsync(surtidor, manguera, stoppingToken);
        }

        /// <summary>
        /// Handler for protocol data received events (status changes, unsolicited messages)
        /// </summary>
        private void OnProtocolDataReceived(object? sender, ProtocolDataReceivedEventArgs e)
        {
            if (e.ParsedData.TryGetValue("HoseStatuses", out var statusesObj))
            {
                var statuses = (Dictionary<string, string>)statusesObj;
                var surtidor = Surtidores.FirstOrDefault(s => s.Id == e.SurtidorId);
                if (surtidor != null)
                {
                    ProcessHoseStatusChanges(surtidor, statuses);
                }
            }
        }

        /// <summary>
        /// Processes hose status changes from GetStatusAsync response
        /// Maps ASPRO status codes (B2, 80, 00, 20) to OperadorCara state transitions
        /// Moved from VerificarEstado logic
        /// </summary>
        private void ProcessHoseStatusChanges(SurtidorSiges surtidor, Dictionary<string, string> statuses)
        {
            var mangueraPar = surtidor.mangueras.FirstOrDefault(x => x.Ubicacion == "Par");
            var mangueraImpar = surtidor.mangueras.FirstOrDefault(x => x.Ubicacion == "Impar");

            if (mangueraPar == null || mangueraImpar == null)
            {
                _logger.Log(NLog.LogLevel.Warn, $"Surtidor {surtidor.Numero} no tiene mangueras Par/Impar configuradas para verificar estado.");
                return;
            }

            // Process Par hose status
            if (statuses.TryGetValue("Par", out var estadoPar))
            {
                ProcessIndividualHoseStatus(surtidor, mangueraPar, estadoPar);
            }

            // Process Impar hose status
            if (statuses.TryGetValue("Impar", out var estadoImpar))
            {
                ProcessIndividualHoseStatus(surtidor, mangueraImpar, estadoImpar);
            }
        }

        /// <summary>
        /// Processes individual hose status transitions
        /// </summary>
        private void ProcessIndividualHoseStatus(SurtidorSiges surtidor, MangueraSiges manguera, string estado)
        {
            if ((estado.Contains("00") || estado.Contains("20")) && manguera.Vendiendo)
            {
                var reposos = IncrementarReposoValidarFinVenta(manguera.Id);
                if (reposos >= MIN_REPOSO_CONSECUTIVO_VALIDAR_FIN)
                {
                    if (manguera.Estado != "ValidarFinVenta")
                    {
                        _logger.Log(NLog.LogLevel.Warn, $"Estado {estado} sostenido en manguera {manguera.Ubicacion} durante venta ({reposos}/{MIN_REPOSO_CONSECUTIVO_VALIDAR_FIN}). Se inicia ValidarFinVenta sin desautorizar. Surtidor {surtidor.Numero}.");
                    }
                    manguera.Estado = "ValidarFinVenta";
                }
            }
            else if ((estado.Contains("00") || estado.Contains("20")) && !manguera.Vendiendo)
            {
                ReiniciarReposoValidarFinVenta(manguera.Id);
                manguera.Estado = "Desautorizar";
            }

            if (estado.Contains("B2") && !manguera.Vendiendo)
            {
                ReiniciarReposoValidarFinVenta(manguera.Id);
                if (manguera.Estado != "BuscarBoton")
                {
                    _logger.Log(NLog.LogLevel.Info, $"Cambio estado manguera {manguera.Ubicacion} a BuscarBoton en surtidor {surtidor.Numero}");
                }
                manguera.Estado = "BuscarBoton";
            }

            if (estado.Contains("80"))
            {
                ReiniciarReposoValidarFinVenta(manguera.Id);
                manguera.Estado = "Colgada";
            }
            else if (!(estado.Contains("00") || estado.Contains("20")))
            {
                ReiniciarReposoValidarFinVenta(manguera.Id);
            }
        }

        private int IncrementarReposoValidarFinVenta(int mangueraId)
        {
            if (!_reposoConsecutivoValidarFinVenta.ContainsKey(mangueraId))
            {
                _reposoConsecutivoValidarFinVenta[mangueraId] = 0;
            }

            _reposoConsecutivoValidarFinVenta[mangueraId]++;
            return _reposoConsecutivoValidarFinVenta[mangueraId];
        }

        private void ReiniciarReposoValidarFinVenta(int mangueraId)
        {
            if (_reposoConsecutivoValidarFinVenta.ContainsKey(mangueraId))
            {
                _reposoConsecutivoValidarFinVenta[mangueraId] = 0;
            }
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
