using Microsoft.AspNetCore.SignalR;

namespace ControladorEstacion.WebApi.Hubs;

/// <summary>
/// SignalR hub to relay RabbitMQ surtidor messages to connected clients.
/// Eliminates the need for frontend to hold Rabbit credentials.
/// </summary>
public class SurtidorHub : Hub
{
    private readonly ILogger<SurtidorHub> _logger;

    public SurtidorHub(ILogger<SurtidorHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Cliente SignalR conectado: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(
            exception,
            "Cliente SignalR desconectado: {ConnectionId}",
            Context.ConnectionId
        );
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Broadcasts a surtidor message to all connected clients.
    /// Called by RabbitMQConsumerService when message arrives.
    /// </summary>
    public async Task BroadcastSurtidorUpdate(SurtidorMessage message)
    {
        await Clients.All.SendAsync("surtidorUpdate", message);
    }
}

/// <summary>
/// Message model for surtidor updates from RabbitMQ.
/// Maps from queue payload.
/// </summary>
public class SurtidorMessage
{
    public int IdEstacion { get; set; }
    public int NumeroSurtidor { get; set; }
    public string? DescripcionSurtidor { get; set; }
    public int IdTurno { get; set; }
    public int NumeroTurno { get; set; }
    public string? Islero { get; set; }
    public int EstadoParImpar { get; set; }
    public string? KmInicial { get; set; }
    public string? KmFinal { get; set; }
    public decimal? VentaTotal { get; set; }
    public decimal? VentaContado { get; set; }
    public long FechaHoraActualizacion { get; set; }
}
