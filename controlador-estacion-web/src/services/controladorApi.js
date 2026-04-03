import { runtimeConfig } from "../config/runtimeConfig";

const defaultHeaders = {
  "Content-Type": "application/json",
};

const buildUrl = (path) => `${runtimeConfig.apiBaseUrl}${path}`;

const handleResponse = async (response) => {
  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || "Error en API");
  }

  return response.json();
};

export const fetchSurtidores = async () => {
  const response = await fetch(buildUrl("/api/v1/controlador/surtidores"));
  return handleResponse(response);
};

export const fetchReporte = async (tipo, fechaInicio, fechaFin) => {
  const endpoint = tipo === "lecturas" ? "lecturas" : "ventas";

  const response = await fetch(
    buildUrl(`/api/v1/controlador/reportes/${endpoint}`),
    {
      method: "POST",
      headers: defaultHeaders,
      body: JSON.stringify({ fechaInicio, fechaFin }),
    }
  );

  return handleResponse(response);
};

export const downloadReportePdf = async (tipo, fechaInicio, fechaFin) => {
  const endpoint = tipo === "lecturas" ? "lecturas.pdf" : "ventas.pdf";

  const response = await fetch(
    buildUrl(`/api/v1/controlador/reportes/${endpoint}`),
    {
      method: "POST",
      headers: defaultHeaders,
      body: JSON.stringify({ fechaInicio, fechaFin }),
    }
  );

  if (!response.ok) {
    const body = await response.text();
    throw new Error(body || `Error descargando reporte ${tipo}`);
  }

  // Get the filename from the content-disposition header or generate one
  const contentDisposition = response.headers.get("content-disposition");
  let filename = `reporte_${tipo}_${new Date().getTime()}.pdf`;
  
  if (contentDisposition) {
    const match = contentDisposition.match(/filename="?([^"]+)"?/);
    if (match) {
      filename = match[1];
    }
  }

  // Get PDF binary data and trigger download
  const pdfBlob = await response.blob();
  const url = window.URL.createObjectURL(pdfBlob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  window.URL.revokeObjectURL(url);
};
