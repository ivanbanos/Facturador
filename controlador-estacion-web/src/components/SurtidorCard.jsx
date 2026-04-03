import React from "react";

const cardStyle = {
  border: "1px solid #d9e1ee",
  borderRadius: "14px",
  padding: "16px",
  backgroundColor: "#ffffff",
  boxShadow: "0 6px 18px rgba(14, 45, 92, 0.08)",
};

const titleStyle = {
  margin: 0,
  fontSize: "1.1rem",
  color: "#0a2f63",
};

const lineStyle = {
  marginTop: "6px",
  fontSize: "0.95rem",
  color: "#24364f",
};

export default function SurtidorCard({ surtidor, estado }) {
  return (
    <article style={cardStyle}>
      <h3 style={titleStyle}>{surtidor.descripcion || `Surtidor ${surtidor.numero}`}</h3>
      <div style={lineStyle}>Turno: {estado.turno || "-"}</div>
      <div style={lineStyle}>Islero: {estado.empleado || "-"}</div>
      <div style={lineStyle}>Estado Par: {estado.estadoPar || "-"}</div>
      <div style={lineStyle}>Estado Impar: {estado.estadoImpar || "-"}</div>
    </article>
  );
}
