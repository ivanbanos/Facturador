import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

/**
 * Hook to connect to SignalR hub and listen for surtidor updates.
 * Replaces STOMP connection - credentials are no longer exposed to client.
 */
export const useSignalRSurtidores = () => {
  const [surtidores, setSurtidores] = useState([]);
  const [connected, setConnected] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    const apiUrl = window.CONTROLADOR_API_URL || 'https://localhost:7198';
    const hubUrl = `${apiUrl}/ws/surtidores`;

    // Create connection
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: retryCount => {
          return Math.min(1000 * Math.pow(2, retryCount), 30000);
        },
      })
      .withHubProtocol(new signalR.JsonHubProtocol())
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // When connected
    connection.onconnected = () => {
      console.log('[SignalR] Conectado al hub de surtidores');
      setConnected(true);
      setError(null);
    };

    // When disconnected
    connection.onclose = (error) => {
      console.log('[SignalR] Desconectado', error);
      setConnected(false);
    };

    // Listen for surtidor updates
    connection.on('surtidorUpdate', (message) => {
      console.log('[SignalR] Actualización recibida:', message);
      
      // Update surtidores list with new/updated item
      setSurtidores((prev) => {
        const updated = [...prev];
        const idx = updated.findIndex((s) => s.IdEstacion === message.IdEstacion && s.NumeroSurtidor === message.NumeroSurtidor);
        
        if (idx >= 0) {
          updated[idx] = {...updated[idx], ...message};
        } else {
          updated.push(message);
        }
        
        return updated;
      });
    });

    // Start connection
    connection
      .start()
      .catch((err) => {
        console.error('[SignalR] Error al conectar:', err);
        setError(`Error de conexión: ${err.message}`);
      });

    // Cleanup on unmount
    return () => {
      console.log('[SignalR] Limpiando conexión...');
      connection.stop();
    };
  }, []);

  return { surtidores, connected, error, setSurtidores };
};
