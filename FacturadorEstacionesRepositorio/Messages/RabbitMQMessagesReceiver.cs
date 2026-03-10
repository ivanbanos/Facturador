using Dominio.Entidades;
using FactoradorEstacionesModelo;
using FactoradorEstacionesModelo.Siges;
using FacturadorEstacionesPOSWinForm;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using Modelo;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControladorEstacion.Messages
{
    public class RabbitMQMessagesReceiver : IMessagesReceiver , IObservable<string>, IDisposable
    {
        List<IObserver<string>> observers = new List<IObserver<string>>();
        AsyncEventingBasicConsumer consumer;
        private readonly InfoEstacion _infoEstacion;
        private IConnection _connection;
        private IChannel _channel;
        private bool _initialized = false;
        private bool _disposed = false;
        
        public RabbitMQMessagesReceiver(IOptions<InfoEstacion> infoEstacion)
        {
            _infoEstacion = infoEstacion.Value;
        }

        private async Task EnsureInitializedAsync()
        {
            if (_initialized && _connection != null && _connection.IsOpen)
            {
                return;
            }

            try
            {
                ConnectionFactory factory = new ConnectionFactory();
                factory.UserName = "siges";
                factory.Password = "siges";
                factory.VirtualHost = "/";
                factory.HostName = _infoEstacion.RabbitHost;
                factory.Port = 5672;
                
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.QueueDeclareAsync(queue: _infoEstacion.Isla,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var mensaje = Encoding.UTF8.GetString(body);
                    foreach (var observer in observers)
                    {
                        observer.OnNext(mensaje);
                    }
                };
                await _channel.BasicConsumeAsync(queue: _infoEstacion.Isla,
                                      autoAck: true,
                                      consumer: consumer);
                
                _initialized = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize RabbitMQ connection to {_infoEstacion.RabbitHost}", ex);
            }
        }

        public async void ReceiveMessages(string queue)
        {
            await EnsureInitializedAsync();
        }

        public IDisposable Subscribe(IObserver<string> observer)
        {
            // Initialize asynchronously without blocking
            _ = EnsureInitializedAsync();
            
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
            return new Unsubscriber(observers, observer);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing RabbitMQ resources: {ex.Message}");
            }

            _disposed = true;
        }

        internal class Unsubscriber : IDisposable
        {
            private List<IObserver<string>> _observers;
            private IObserver<string> _observer;

            internal Unsubscriber(List<IObserver<string>> observers, IObserver<string> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
    }
}
