// Índices MongoDB para ordenes de despacho y facturas canastilla.
// Ejecutar con mongosh sobre la base configurada en RepositorioConfig.Cliente:
//   mongosh "<cadena de conexión>/<base>" IndicesOrdenesYCanastilla.js
// createIndex es idempotente: si el índice ya existe con la misma definición no hace nada.
// En colecciones grandes ejecutar fuera de horas pico.
// Verificar uso con: db.ordenes.find({...}).explain("executionStats") → debe mostrar IXSCAN.

// ---- ordenes (OrdenDeDespachoRepositorio) ----

// AddRange, ObtenerOrdenDespachoPorIdVentaLocal, AgregarTurnoAOrdenDeDespacho, AgregarFechaReporteFactura
db.ordenes.createIndex({ EstacionGuid: 1, IdVentaLocal: 1 }, { name: "ix_estacion_idventalocal" });

// GetOrdenesDeDespacho: rama por FechaReporte
db.ordenes.createIndex({ EstacionGuid: 1, FechaReporte: 1 }, { name: "ix_estacion_fechareporte" });

// GetOrdenesDeDespacho: rama por Fecha y consulta de respaldo
db.ordenes.createIndex({ EstacionGuid: 1, Fecha: 1 }, { name: "ix_estacion_fecha" });

// ObtenerOrdenesPorTurno
db.ordenes.createIndex({ TurnoGuid: 1 }, { name: "ix_turno" });

// Vista de clientes en todas las estaciones (POST OrdenesDeDespacho/TodasLasEstaciones):
// filtra por Identificacion sin estación, una rama por FechaReporte y otra por Fecha
db.ordenes.createIndex({ Identificacion: 1, FechaReporte: 1 }, { name: "ix_identificacion_fechareporte" });
db.ordenes.createIndex({ Identificacion: 1, Fecha: 1 }, { name: "ix_identificacion_fecha" });

// ---- facturasCanastillas (FacturaCanastillaRepository) ----

// Add, FacturaGenerada, GetFacturaPorIdCanastilla
db.facturasCanastillas.createIndex({ FacturasCanastillaId: 1, IdEstacion: 1 }, { name: "ix_facturaid_estacion" });

// GetFacturas: reporte por estación y rango de fechas
db.facturasCanastillas.createIndex({ IdEstacion: 1, fecha: 1 }, { name: "ix_estacion_fecha" });

// Reporte por estación filtrado por identificación del tercero
db.facturasCanastillas.createIndex({ IdEstacion: 1, "terceroId.Identificacion": 1, fecha: 1 }, { name: "ix_estacion_tercero_fecha" });

// Vista de clientes en todas las estaciones (POST FacturasCanastilla/TodasLasEstaciones)
db.facturasCanastillas.createIndex({ "terceroId.Identificacion": 1, fecha: 1 }, { name: "ix_tercero_fecha" });

print("Índices de ordenes:");
printjson(db.ordenes.getIndexes().map(i => i.name));
print("Índices de facturasCanastillas:");
printjson(db.facturasCanastillas.getIndexes().map(i => i.name));
