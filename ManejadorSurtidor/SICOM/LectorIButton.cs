using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManejadorSurtidor.SICOM
{
    public class LectorIButton
    {
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task<string> leerBoton(string puerto, int surtidorNumero, bool caraPAr, Logger _logger)
        {
            var resultado = new StringBuilder();
            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

            System.ComponentModel.IContainer components = null;
            components = new System.ComponentModel.Container();
            using var serialPort = new SerialPort(components)
            {
                PortName = puerto,
                BaudRate = 9600,
                Encoding = Encoding.GetEncoding(28591),
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                ReceivedBytesThreshold = 10,
                RtsEnable = true,
                Handshake = Handshake.None,
                ReadTimeout = 2000,
                WriteTimeout = 2000
            };

            SerialDataReceivedEventHandler handler = (sender, args) =>
            {
                try
                {
                    var sp = (SerialPort)sender;
                    if (sp.BytesToRead <= 0)
                    {
                        return;
                    }

                    string intdata = sp.ReadExisting();
                    byte[] response = Encoding.GetEncoding(28591).GetBytes(intdata);
                    string hexString = BitConverter.ToString(response);

                    var chunk = hexString
                        .Replace("4e-42-", "", StringComparison.OrdinalIgnoreCase)
                        .Replace("4E-42-", "", StringComparison.OrdinalIgnoreCase);

                    lock (resultado)
                    {
                        resultado.Append(chunk);
                        var clean = resultado.ToString().Replace("-", "");
                        if (clean.Length >= 16)
                        {
                            tcs.TrySetResult(resultado.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(NLog.LogLevel.Info, $"Error Leyendo boton {ex.Message}");
                    _logger.Log(NLog.LogLevel.Info, $"Error Leyendo boton {ex.StackTrace}");
                }
            };

            try
            {
                serialPort.DataReceived += handler;

                var puertosDisponibles = SerialPort.GetPortNames();
                _logger.Log(NLog.LogLevel.Info, $"LectorIButton iniciado. Puerto configurado: {puerto}. Puertos detectados: {string.Join(",", puertosDisponibles)}");
                if (Array.IndexOf(puertosDisponibles, puerto) < 0)
                {
                    _logger.Log(NLog.LogLevel.Warn, $"Puerto iButton {puerto} no aparece en puertos detectados.");
                }

                if (!serialPort.IsOpen)
                {
                    _logger.Log(NLog.LogLevel.Info, $"Abriendo puerto iButton {puerto}");
                    serialPort.Open();
                    _logger.Log(NLog.LogLevel.Info, $"Puerto iButton {puerto} abierto correctamente");
                }

                _logger.Log(NLog.LogLevel.Info, $"Leyendo botón. surtidor {surtidorNumero} caraPar {caraPAr}");
                string trama = GetTrama(surtidorNumero, caraPAr);
                if (string.IsNullOrWhiteSpace(trama))
                {
                    _logger.Log(NLog.LogLevel.Info, "Trama vacia para lectura de boton");
                    return "fail";
                }
                _logger.Log(NLog.LogLevel.Info, $"Trama iButton a enviar: {trama}");

                var intentos = 0;
                while (!tcs.Task.IsCompleted && intentos++ < 3)
                {
                    byte[] tramaByte = FromHex(trama);
                    _logger.Log(NLog.LogLevel.Info, $"Enviando trama iButton intento {intentos} por puerto {puerto}");
                    serialPort.Write(tramaByte, 0, tramaByte.Length);
                    await Task.WhenAny(tcs.Task, Task.Delay(2000));
                }

                if (!tcs.Task.IsCompleted)
                {
                    _logger.Log(NLog.LogLevel.Info, "Timeout leyendo boton");
                    return "fail";
                }

                var resultadoFinal = await tcs.Task;
                var codigos = resultadoFinal.Split("-", StringSplitOptions.RemoveEmptyEntries);
                if (codigos.Length < 8)
                {
                    _logger.Log(NLog.LogLevel.Info, "Lectura de boton incompleta");
                    return "fail";
                }

                var iButton = "";
                for (int i = 0; i < 8; i++)
                {
                    iButton = codigos[i] + iButton;
                }
                _logger.Log(NLog.LogLevel.Info, $"Leyendo boton {iButton}");
                return iButton;
            }
            catch (Exception ex)
            {
                _logger.Log(NLog.LogLevel.Error, $"Error Leyendo boton en puerto {puerto}. {ex.Message}");
                _logger.Log(NLog.LogLevel.Error, $"Error Leyendo boton stacktrace {ex.StackTrace}");
                return "fail";
            }
            finally
            {
                try
                {
                    serialPort.DataReceived -= handler;
                    if (serialPort.IsOpen)
                    {
                        serialPort.Close();
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log(NLog.LogLevel.Warn, $"Error cerrando puerto de boton {puerto}. {ex.Message}");
                }
            }
        }

        private string GetTrama(int surtidorNumero, bool caraPAr)
        {
            if (caraPAr)
            {
                switch (surtidorNumero)
                {
                    case 1:
                        return "3B31324971";
                    case 2:
                        return "3B32324972";
                    case 3:
                        return "3B33324973";
                    case 4:
                        return "3B34324974";
                    case 5:
                        return "3B35324975";
                    case 6:
                        return "3B36324976";
                    case 7:
                        return "3B37324977";
                    case 8:
                        return "3B38324972";
                }
            }
            else
            {
                switch (surtidorNumero)
                {
                    case 1:
                        return "3B31314972";
                    case 2:
                        return "3B32314971";
                    case 3:
                        return "3B33314970";
                    case 4:
                        return "3B34314977";
                    case 5:
                        return "3B35314976";
                    case 6:
                        return "3B36314975";
                    case 7:
                        return "3B37314974";
                    case 8:
                        return "3B38314972";
                }
            }
            return "";
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
    }
}
