using FacturadorEstacionesPOSWinForm;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using Modelo;
using NLog;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManejadorSurtidor.Messages
{
    public class RabbitMQProducer : IMessageProducer, IDisposable
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly InfoEstacion _infoEstacion;
        private IConnection _connection;
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
        private bool _disposed = false;

        public RabbitMQProducer(IOptions<InfoEstacion> infoEstacion)
        {
            _infoEstacion = infoEstacion.Value;
        }

        private async Task EnsureConnectionAsync()
        {
            if (_connection != null && _connection.IsOpen)
            {
                return;
            }

            await _connectionLock.WaitAsync();
            try
            {
                if (_connection != null && _connection.IsOpen)
                {
                    return;
                }

                try
                {
                    _connection?.Dispose();
                }
                catch
                {
                }

                ConnectionFactory factory = new ConnectionFactory
                {
                    UserName = "siges",
                    Password = "siges",
                    VirtualHost = "/",
                    HostName = _infoEstacion.RabbitHost,
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                };

                _connection = await factory.CreateConnectionAsync();
                _logger.Log(LogLevel.Info, $"RabbitMQ conexión abierta en host {_infoEstacion.RabbitHost}");
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, $"Error creando conexión RabbitMQ a host {_infoEstacion.RabbitHost}: {ex.Message}");
                throw new InvalidOperationException($"Failed to create RabbitMQ connection to {_infoEstacion.RabbitHost}", ex);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task ResetConnectionAsync()
        {
            await _connectionLock.WaitAsync();
            try
            {
                try
                {
                    _connection?.Dispose();
                }
                catch
                {
                }

                _connection = null;
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async Task SendMessage<T>(T message, string queue)
        {
            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);
            
            int maxRetries = 3;
            Exception? lastException = null;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    _logger.Log(LogLevel.Info, $"Publicando mensaje en cola '{queue}'. Intento {i + 1}/{maxRetries}");
                    await EnsureConnectionAsync();

                    if (_connection == null || _connection.IsOpen == false)
                    {
                        throw new InvalidOperationException("RabbitMQ connection is not available");
                    }

                    using var channel = await _connection.CreateChannelAsync();
                    await channel.QueueDeclareAsync(queue: queue,
                                           durable: true,
                                          exclusive: false,
                                          autoDelete: false,
                                          arguments: null);
                    await channel.BasicPublishAsync(exchange: "", routingKey: queue, body: body);
                    _logger.Log(LogLevel.Info, $"Mensaje publicado correctamente en cola '{queue}'");
                    return;
                }
                catch (AlreadyClosedException ex)
                {
                    lastException = ex;
                    _logger.Log(LogLevel.Warn, $"Conexión/canal RabbitMQ cerrado al publicar en '{queue}' (intento {i + 1}/{maxRetries}): {ex.Message}");
                    await ResetConnectionAsync();
                }
                catch (OperationInterruptedException ex)
                {
                    lastException = ex;
                    _logger.Log(LogLevel.Warn, $"Operación RabbitMQ interrumpida al publicar en '{queue}' (intento {i + 1}/{maxRetries}): {ex.Message}");
                    await ResetConnectionAsync();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i < maxRetries - 1)
                    {
                        _logger.Log(LogLevel.Warn, $"Falló publicación en cola '{queue}' intento {i + 1}: {ex.Message}. Reintentando...");
                    }
                }

                if (i < maxRetries - 1)
                {
                    await Task.Delay(1000);
                }
            }

            _logger.Log(LogLevel.Error, $"Falló publicación en cola '{queue}' tras {maxRetries} intentos: {lastException?.Message}");
            throw new InvalidOperationException($"Failed to send message to queue '{queue}' after {maxRetries} attempts", lastException);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing RabbitMQ connection: {ex.Message}");
            }

            _connectionLock.Dispose();

            _disposed = true;
        }
    }
}
