using FactoradorEstacionesModelo.Siges;
using ManejadorSurtidor.SICOM;
using System.IO.Ports;

namespace ManejadorSurtidor.Protocols
{
    /// <summary>
    /// Abstraction for pump server communication protocols (ASPRO, Silog, Prosoft, etc.)
    /// Separates operational logic from technical protocol implementation details.
    /// </summary>
    public interface IPumpProtocol : IDisposable
    {
        /// <summary>
        /// Initializes the protocol connection (serial port, network, etc.)
        /// </summary>
        Task InitializeAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Closes the protocol connection gracefully
        /// </summary>
        Task CloseAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Checks if the protocol connection is open and operational
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the configuration/port name of the connection
        /// </summary>
        string ConnectionInfo { get; }

        // ===== Operational Commands =====
        
        /// <summary>
        /// Authorizes a hose to dispense fuel
        /// </summary>
        Task AuthorizeHoseAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken);

        /// <summary>
        /// Deauthorizes a hose from dispensing
        /// </summary>
        Task DeauthorizeHoseAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken, bool waitForResponse = true);

        /// <summary>
        /// Reads the last sale amount from a hose
        /// </summary>
        Task<double> ReadLastSaleAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken);

        /// <summary>
        /// Reads the totalizer value from a hose
        /// </summary>
        Task<double> ReadTotalizerAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken);

        /// <summary>
        /// Gets the current status of all hoses in a pump
        /// Returns a dictionary where key = hose location and value = status code (B2, 80, 00, 20, etc.)
        /// Protocol should interpret raw bytes into standard status codes
        /// </summary>
        Task<Dictionary<string, string>> GetStatusAsync(SurtidorSiges surtidor, CancellationToken stoppingToken);

        // ===== Event Handling =====

        /// <summary>
        /// Event fired when data is received from protocol
        /// Allows protocol to notify OperadorCara of unsolicited messages or state changes
        /// </summary>
        event EventHandler<ProtocolDataReceivedEventArgs>? DataReceived;
    }

    /// <summary>
    /// Event args for protocol data reception
    /// </summary>
    public class ProtocolDataReceivedEventArgs : EventArgs
    {
        public int? SurtidorId { get; set; }
        public int? MangueraId { get; set; }
        public string RawData { get; set; } = string.Empty;
        public Dictionary<string, object> ParsedData { get; set; } = new();
    }
}
