import React, { useState } from "react";
import { fetchReporte, downloadReportePdf } from "../services/controladorApi";

const boxStyle = {
  maxWidth: "540px",
  border: "1px solid #d9e1ee",
  borderRadius: "14px",
  padding: "16px",
  backgroundColor: "#ffffff",
};

export default function ReportesPage() {
  const [fechaInicio, setFechaInicio] = useState("");
  const [fechaFin, setFechaFin] = useState("");
  const [loading, setLoading] = useState(false);
  const [resultado, setResultado] = useState(null);
  const [error, setError] = useState("");

  const consultar = async (tipo) => {
    setLoading(true);
    setError("");
    setResultado(null);

    try {
      const data = await fetchReporte(tipo, fechaInicio, fechaFin);
      setResultado(data);
    } catch (err) {
      setError(err.message || "No fue posible consultar el reporte");
    } finally {
      setLoading(false);
    }
  };

  const descargarPdf = async (tipo) => {
    setLoading(true);
    setError("");

    try {
      await downloadReportePdf(tipo, fechaInicio, fechaFin);
    } catch (err) {
      setError(err.message || `No fue posible descargar el reporte ${tipo}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <section>
      <h2>Reportes</h2>
      <p>Consulta de reportes por rango de fechas (vía API .NET 8).</p>

      <div style={boxStyle}>
        <div style={{ marginBottom: "12px" }}>
          <label>Desde</label>
          <input
            type="date"
            value={fechaInicio}
            onChange={(e) => setFechaInicio(e.target.value)}
            style={{ width: "100%", marginTop: "4px" }}
          />
        </div>

        <div style={{ marginBottom: "12px" }}>
          <label>Hasta</label>
          <input
            type="date"
            value={fechaFin}
            onChange={(e) => setFechaFin(e.target.value)}
            style={{ width: "100%", marginTop: "4px" }}
          />
        </div>

        <div style={{ marginBottom: "12px" }}>
          <h4 style={{ margin: "0 0 8px 0" }}>Descargar en PDF</h4>
          <div style={{ display: "flex", gap: "8px" }}>
            <button
              type="button"
              disabled={loading || !fechaInicio || !fechaFin}
              onClick={() => descargarPdf("lecturas")}
              style={{ backgroundColor: "#051D38", color: "#7CC5FC", fontWeight: "600", border: "none", padding: "8px 16px", borderRadius: "6px", cursor: "pointer" }}
            >
              {loading ? "Descargando..." : "PDF Lecturas"}
            </button>

            <button
              type="button"
              disabled={loading || !fechaInicio || !fechaFin}
              onClick={() => descargarPdf("ventas")}
              style={{ backgroundColor: "#051D38", color: "#7CC5FC", fontWeight: "600", border: "none", padding: "8px 16px", borderRadius: "6px", cursor: "pointer" }}
            >
              {loading ? "Descargando..." : "PDF Ventas"}
            </button>
          </div>
        </div>

        <div>
          <h4 style={{ margin: "0 0 8px 0" }}>Ver datos (JSON)</h4>
          <div style={{ display: "flex", gap: "8px" }}>
            <button
              type="button"
              disabled={loading || !fechaInicio || !fechaFin}
              onClick={() => consultar("lecturas")}
              style={{ backgroundColor: "#051D38", color: "#7CC5FC", fontWeight: "600", border: "none", padding: "8px 16px", borderRadius: "6px", cursor: "pointer" }}
            >
              Datos Lecturas
            </button>

            <button
              type="button"
              disabled={loading || !fechaInicio || !fechaFin}
              onClick={() => consultar("ventas")}
              style={{ backgroundColor: "#051D38", color: "#7CC5FC", fontWeight: "600", border: "none", padding: "8px 16px", borderRadius: "6px", cursor: "pointer" }}
            >
              Datos Ventas
            </button>
          </div>
        </div>

        {loading && <p style={{ marginTop: "12px" }}>Procesando...</p>}
        {error && <p style={{ marginTop: "12px", color: "#b00020" }}>{error}</p>}

        {resultado && (
          <pre
            style={{
              marginTop: "12px",
              whiteSpace: "pre-wrap",
              backgroundColor: "#f4f7fb",
              padding: "12px",
              borderRadius: "8px",
            }}
          >
            {JSON.stringify(resultado, null, 2)}
          </pre>
        )}
      </div>
    </section>
  );
}
