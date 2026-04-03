import React, { useEffect, useMemo, useState } from "react";
import SurtidorCard from "../components/SurtidorCard";
import { useSignalRSurtidores } from "../hooks/useSignalRSurtidores";
import { fetchSurtidores } from "../services/controladorApi";

const gridStyle = {
  display: "grid",
  gridTemplateColumns: "repeat(auto-fill, minmax(240px, 1fr))",
  gap: "16px",
};

const normalizeSurtidor = (item) => ({
  id: item.id || item.idEstacion,
  numero: item.numero || item.numeroSurtidor,
  descripcion: item.descripcion || item.descripcionSurtidor,
  estado: item.estado || "",
  turno: item.turno || item.numeroTurno || "",
  islero: item.islero || "",
  estadoParImpar: item.estadoParImpar || 0,
});

export default function DashboardPage() {
  const [surtidores, setSurtidores] = useState([]);
  const [error, setError] = useState("");
  const { connected: signalRConnected, error: signalRError } = useSignalRSurtidores();

  // Initial load of surtidores from API
  useEffect(() => {
    let isActive = true;

    fetchSurtidores()
      .then((data) => {
        if (!isActive) {
          return;
        }

        const mapped = (data || []).map(normalizeSurtidor);
        setSurtidores(mapped);
      })
      .catch((err) => {
        if (!isActive) {
          return;
        }

        setError(err.message || "No fue posible cargar surtidores");
      });

    return () => {
      isActive = false;
    };
  }, []);

  const hasData = useMemo(() => surtidores.length > 0, [surtidores.length]);
  const connectionStatus = signalRConnected ? "Conectado" : "Desconectado";

  return (
    <section>
      <h2>Dashboard de Surtidores</h2>
      <p>Monitoreo en tiempo real por SignalR.</p>
      <p style={{  fontSize: '0.9rem', color: signalRConnected ? '#4caf50' : '#ff9800' }}>
        Estado: {connectionStatus}
      </p>

      {error && <p style={{ color: "#b00020" }}>{error}</p>}
      {signalRError && <p style={{ color: "#ff9800" }}>Alerta: {signalRError}</p>}
      {!error && !hasData && <p>Cargando surtidores...</p>}

      {hasData && (
        <div style={gridStyle}>
          {surtidores.map((surtidor) => (
            <SurtidorCard
              key={surtidor.id}
              surtidor={surtidor}
            />
          ))}
        </div>
      )}
    </section>
  );
}
