using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ControladorEstacion.WebApi.Hubs;

namespace ControladorEstacion.WebApi.Services;

/// <summary>
/// Background service that consumes surtidor messages from RabbitMQ
/// and broadcasts them to connected SignalR clients.
/// </summary>
public class RabbitMQConsumerService : BackgroundService
{
    private readonly ILogger<RabbitMQConsumerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHubContext<SurtidorHub> _hubContext;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMQConsumerService(
        ILogger<RabbitMQConsumerService> logger,
        IConfiguration configuration,
        IHubContext<SurtidorHub> hubContext
    )
    {
        _logger = logger;
        _configuration = configuration;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Read RabbitMQ config
            var host = _configuration["RabbitMQ:Host"] ?? "localhost";
            var port = _configuration.GetValue<int>("RabbitMQ:Port", 5672);
            var username = _configuration["RabbitMQ:Username"] ?? "guest";
            var password = _configuration["RabbitMQ:Password"] ?? "guest";
            var queueName = _configuration["RabbitMQ:QueueName"] ?? "surtidores";

            _logger.LogInformation(
                "Iniciando consumidor RabbitMQ: {Host}:{Port}, Queue: {QueueName}",
                host,
                port,
                queueName
            );

            // Create factory and connection
            var factory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                UserName = username,
                Password = password,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare queue (idempotent if exists)
            _channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += (model, ea) => HandleMessageAsync(ea, stoppingToken);

            // Start consuming
            _channel.BasicConsume(
                queue: queueName,
                autoAck: true,
                consumerTag: "controlador-consumer",
                consumer: consumer
            );

            _logger.LogInformation("Consumidor RabbitMQ iniciado exitosamente");

            // Keep service running until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumidor RabbitMQ cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fatal en consumidor RabbitMQ");
            throw;
        }
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken)
    {
        try
        {
            var body = ea.Body.ToArray();
            var message = System.Text.Encoding.UTF8.GetString(body);

            _logger.LogDebug("Mensaje RabbitMQ recibido: {Message}", message);

            // Parse JSON message
            var surtidorMessage = JsonSerializer.Deserialize<SurtidorMessage>(message);
            if (surtidorMessage != null)
            {
                // Broadcast to all connected SignalR clients
                await _hubContext.Clients.All.SendAsync("surtidorUpdate", surtidorMessage, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando mensaje RabbitMQ");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo consumidor RabbitMQ");
        
        if (_channel != null)
        {
            _channel.Close();
            _channel.Dispose();
        }
        
        if (_connection != null)
        {
            _connection.Close();
            _connection.Dispose();
        }

        await base.StopAsync(cancellationToken);
    }
}
