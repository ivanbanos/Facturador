using System.IO.Ports;
using System.Text;
using FactoradorEstacionesModelo.Siges;
using ManejadorSurtidor.SICOM;
using NLog;

namespace ManejadorSurtidor.Protocols
{
    /// <summary>
    /// ASPRO pump server protocol implementation.
    /// Encapsulates all ASPRO-specific serial communication, frame parsing, and state management.
    /// This allows OperadorCara to remain protocol-agnostic and testable.
    /// </summary>
    public class AsproProtocol : IPumpProtocol
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private SerialPort? _serialPort;
        private readonly string _portName;
        private readonly int _baudRate;
        private readonly IEnumerable<SurtidorSiges> _surtidores;

        // ASPRO Protocol State Management
        private bool _respondio;
        private bool _finalizo;
        private int _count = 0;
        
        // Pending Request Tracking - maintains correlation between request and response
        private int? _surtidorEsperaId;
        private int? _mangueraEsperaId;

        // ASPRO Frame Constants
        private const string ASPRO_VENTA_HEADER = "0130305F";
        private const string ASPRO_VENTA_FALLBACK = "0130303";
        private const string ASPRO_TOTALIZADOR_HEADER = "163031";
        private const string ASPRO_TOTALIZADOR_FALLBACK = "163030";
        private const string ASPRO_ESTADO_HEADER = "023031";
        private const string ASPRO_ESTADO_FALLBACK = "023030";

        public event EventHandler<ProtocolDataReceivedEventArgs>? DataReceived;

        public bool IsConnected => _serialPort?.IsOpen ?? false;
        public string ConnectionInfo => _portName;

        public AsproProtocol(string portName, IEnumerable<SurtidorSiges> surtidores, int baudRate = 4800)
        {
            _portName = portName;
            _baudRate = baudRate;
            _surtidores = surtidores;
        }

        public async Task InitializeAsync(CancellationToken stoppingToken)
        {
            try
            {
                _serialPort = new SerialPort
                {
                    PortName = _portName,
                    BaudRate = _baudRate,
                    Encoding = Encoding.GetEncoding(28591),
                    Parity = Parity.Even,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    ReceivedBytesThreshold = 10,
                    RtsEnable = true,
                    Handshake = Handshake.None
                };

                _serialPort.DataReceived += OnSerialDataReceived;
                _serialPort.Open();

                _logger.Info($"ASPRO Protocol initialized on {_portName}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Failed to initialize ASPRO protocol on {_portName}");
                throw;
            }
        }

        public async Task CloseAsync(CancellationToken stoppingToken)
        {
            try
            {
                if (_serialPort?.IsOpen == true)
                {
                    _serialPort.Close();
                    _logger.Info($"ASPRO Protocol closed");
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error closing ASPRO protocol");
            }
        }

        public async Task AuthorizeHoseAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string frame = $"1433303{surtidor.Numero}0{ubicacion}{(140 + surtidor.Numero + ubicacion).ToString("X")}";
            await EnviarTramaAsync(surtidor, manguera, frame, stoppingToken);
            ParseAuthorizationResponse(_serialPort!);
        }

        public async Task DeauthorizeHoseAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken, bool waitForResponse = true)
        {
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string frame = $"1533303{surtidor.Numero}0{ubicacion}{(150 + surtidor.Numero + ubicacion).ToString("X")}";
            await EnviarTramaAsync(surtidor, manguera, frame, stoppingToken, waitForResponse);
            ParseDeauthorizationResponse(_serialPort!);
        }

        public async Task<double> ReadLastSaleAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            _serialPort!.ReceivedBytesThreshold = 10;
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string frame = $"0130303{surtidor.Numero}0{ubicacion}{(98 + surtidor.Numero + ubicacion).ToString("X")}";
            await EnviarTramaAsync(surtidor, manguera, frame, stoppingToken);
            return ParseVentaFrame(_serialPort, surtidor, manguera);
        }

        public async Task<double> ReadTotalizerAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
        {
            _serialPort!.ReceivedBytesThreshold = 10;
            var ubicacion = manguera.Ubicacion == "Par" ? 1 : 0;
            string frame = $"1630303{surtidor.Numero}0{ubicacion}{(166 + surtidor.Numero + ubicacion).ToString("X")}";
            await EnviarTramaAsync(surtidor, manguera, frame, stoppingToken);
            return ParseTotalizadorFrame(_serialPort, surtidor, manguera);
        }

        public async Task<Dictionary<string, string>> GetStatusAsync(SurtidorSiges surtidor, CancellationToken stoppingToken)
        {
            _serialPort!.ReceivedBytesThreshold = 10;
            string frame = $"0230303{surtidor.Numero}00{(104 + surtidor.Numero).ToString("X")}";
            await EnviarTramaAsync(surtidor, null, frame, stoppingToken);
            return ParseStatusFrame(_serialPort, surtidor);
        }

        // ===== Internal ASPRO Protocol Methods =====

        /// <summary>
        /// Sends a frame and waits for response with request/response correlation.
        /// Moved from OperadorCara.EnviarTramaAsync
        /// </summary>
        private async Task EnviarTramaAsync(SurtidorSiges s, MangueraSiges? manguera, string trama, CancellationToken stoppingToken, bool debeEsperar = true)
        {
            var surtidor = _surtidores.FirstOrDefault(x => x.Numero == s.Numero);
            if (surtidor == null)
            {
                _logger.Warn($"No se encontró surtidor número {s.Numero} para enviar trama {trama}.");
                return;
            }

            surtidor.esperando = true;
            _respondio = false;
            _finalizo = false;
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
                    _logger.Warn($"No se encontró manguera id {manguera.Id} en surtidor {surtidor.Numero} para trama {trama}.");
                }
            }

            byte[] tramaByte = FromHex(trama);
            _logger.Info($"Enviando trama {trama} a surtidor {surtidor.Numero} manguera {(manguera?.Ubicacion ?? "N/A")} - Threshold {_serialPort?.ReceivedBytesThreshold}");
            
            _count = 0;
            var inicioEsperaRespuesta = DateTime.Now;
            var bufferLimpiado = false;

            while (!_respondio && (debeEsperar || ++_count < 6))
            {
                if (!_serialPort!.IsOpen)
                {
                    _logger.Error($"No se puede enviar trama {trama}: puerto {_serialPort.PortName} está cerrado.");
                    break;
                }

                if (!bufferLimpiado)
                {
                    try
                    {
                        _serialPort.DiscardInBuffer();
                        bufferLimpiado = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn($"No fue posible limpiar buffer de entrada del puerto {_serialPort.PortName} antes de enviar trama {trama}: {ex.Message}");
                    }
                }

                _serialPort.Write(tramaByte, 0, tramaByte.Length);
                await Task.Delay(400, stoppingToken);

                if ((DateTime.Now - inicioEsperaRespuesta).TotalSeconds > 12)
                {
                    _logger.Error($"Timeout esperando respuesta de trama {trama} en surtidor {surtidor.Numero}.");
                    break;
                }
            }

            if (!_respondio)
            {
                _logger.Warn($"Sin respuesta serial para trama {trama} en surtidor {surtidor.Numero}. Estado manguera: {manguera?.Estado}");
            }

            var inicioEsperaFinal = DateTime.Now;
            while (!_finalizo)
            {
                if ((DateTime.Now - inicioEsperaFinal).TotalSeconds > 15)
                {
                    _logger.Error($"Timeout esperando finalización de lectura serial para trama {trama} en surtidor {surtidor.Numero}.");
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

        /// <summary>
        /// Parses ASPRO last sale (venta) frame.
        /// Moved from OperadorCara.GetTRamaVenta
        /// </summary>
        private double ParseVentaFrame(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {
            surtidor = _surtidores.First(x => x.Id == surtidor.Id);
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

            _respondio = true;
            surtidor.esperando = false;
            _count = 0;
            string totalventa = "";
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

            manguera.CambioVenta = true;
            _finalizo = true;
            return double.Parse(totalventa) / 100;
        }

        /// <summary>
        /// Parses ASPRO totalizer frame.
        /// Moved from OperadorCara.GetTRamaTotalizador
        /// </summary>
        private double ParseTotalizadorFrame(SerialPort sp, SurtidorSiges surtidor, MangueraSiges manguera)
        {
            surtidor = _surtidores.First(x => x.Id == surtidor.Id);
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

            _respondio = true;
            surtidor.esperando = false;
            _count = 0;
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
            totalventa += hexString.Substring(iniciolector, 1);
            iniciolector += 2;
            totalventa += hexString.Substring(iniciolector, 1);

            _finalizo = true;
            return double.Parse(totalventa) / 100;
        }

        /// <summary>
        /// Parses ASPRO authorization response.
        /// Moved from OperadorCara.GetTRamaAutorizar
        /// </summary>
        private void ParseAuthorizationResponse(SerialPort sp)
        {
            DrainShortResponse(sp, "autorizar");
        }

        /// <summary>
        /// Parses ASPRO deauthorization response.
        /// Moved from OperadorCara.GetTRamaDesautorizar
        /// </summary>
        private void ParseDeauthorizationResponse(SerialPort sp)
        {
            DrainShortResponse(sp, "desautorizar");
            _respondio = true;
            _finalizo = true;
        }

        /// <summary>
        /// Parses ASPRO status frame and extracts hose state codes.
        /// Moved from OperadorCara.VerificarEstado
        /// Returns Dictionary mapping hose location to status code (B2, 80, 00, 20, etc.)
        /// </summary>
        private Dictionary<string, string> ParseStatusFrame(SerialPort sp, SurtidorSiges surtidor)
        {
            surtidor = _surtidores.First(x => x.Id == surtidor.Id);
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

            _respondio = true;
            _count = 0;
            _finalizo = true;

            string estadoImPar = hexString.Substring(12, 2);
            string estadoPar = hexString.Substring(14, 2);

            return new Dictionary<string, string>
            {
                { "Par", estadoPar },
                { "Impar", estadoImPar }
            };
        }

        /// <summary>
        /// Drains short response frames (authorization/deauthorization).
        /// Moved from OperadorCara.DrenarRespuestaCorta
        /// </summary>
        private void DrainShortResponse(SerialPort sp, string operacion, int timeoutMs = 350)
        {
            var acumulado = string.Empty;
            var inicio = DateTime.UtcNow;

            while ((DateTime.UtcNow - inicio).TotalMilliseconds < timeoutMs)
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
                _logger.Info($"Respuesta serial {operacion} capturada. Bytes {bytes.Length}, hex {hex}");
            }
        }

        /// <summary>
        /// Converts hex string to byte array.
        /// Moved from OperadorCara.FromHex
        /// </summary>
        private byte[] FromHex(string hex)
        {
            hex = hex.Replace("-", "");
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        // ===== Serial Port Event Handler =====

        private void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Handler is driven by the polling in EnviarTramaAsync and frame parsing methods
            // The frame parsing methods read directly from the SerialPort on demand
            // This architecture maintains compatibility with the current synchronous design
        }

        public void Dispose()
        {
            _serialPort?.Dispose();
        }
    }
}
