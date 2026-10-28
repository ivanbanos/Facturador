// Exportación a PDF y Excel de las órdenes de despacho filtradas
import * as XLSX from 'xlsx'
import {
  formatCurrency,
  formatDate,
  formatIdFacturaElectronica,
  getEstadoDisplay,
  getFormaPago1,
  getFormaPago2,
  getFormasPagoDisplay,
  getTotalPago1,
  getTotalPago2,
} from './ordenesUtils'

var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

/**
 * @typedef {Object} OpcionesExportacion
 * @property {string} fechaInicial
 * @property {string} fechaFinal
 * @property {Object} estadisticas resultado de calcularEstadisticas
 * @property {string} encabezado nombre que va en el encabezado (estación o cliente)
 * @property {string} [nit]
 * @property {boolean} [mostrarEstacion] agrega la columna Estación (vista de todas las estaciones)
 */

const nombreArchivo = ({ fechaInicial, fechaFinal }, extension) => {
  const fechaReporte = new Date().toISOString().split('T')[0]
  return `ordenes_despacho_${fechaInicial}_${fechaFinal}_${fechaReporte}.${extension}`
}

const idFacturaTexto = (orden) => {
  const facturaInfo = formatIdFacturaElectronica(orden.idFacturaElectronica)
  return typeof facturaInfo === 'object' ? facturaInfo.completo : facturaInfo
}

/**
 * @param {Array} ordenes
 * @param {OpcionesExportacion} opciones
 */
export const descargarOrdenesPDF = (ordenes, opciones) => {
  const { fechaInicial, fechaFinal, estadisticas, encabezado, nit, mostrarEstacion } = opciones

  const encabezados = [
    'ID Transacción',
    ...(mostrarEstacion ? ['Estación'] : []),
    'Fecha y Hora',
    'Cliente',
    'Formas de Pago',
    'Combustible',
    'Placa',
    'Cantidad',
    'Precio',
    'ID Factura',
    'Estado',
    'Total',
  ]

  const tableRows = [
    encabezados.map((text) => ({ text, style: 'tableHeader' })),
    ...ordenes.map((orden) => [
      orden.idVentaLocal,
      ...(mostrarEstacion ? [orden.estacionNombre || 'N/A'] : []),
      formatDate(orden.fecha),
      `${orden.nombreTercero || 'N/A'}\n${orden.identificacion || ''}`,
      getFormasPagoDisplay(orden),
      orden.combustible || 'N/A',
      orden.placa || 'N/A',
      orden.cantidad || '0',
      formatCurrency(orden.precio),
      idFacturaTexto(orden),
      getEstadoDisplay(orden.estado),
      formatCurrency(orden.total),
    ]),
  ]

  const anchos = encabezados.map((titulo) => (titulo === 'Cliente' ? '*' : 'auto'))

  const docDefinition = {
    pageSize: 'A4',
    pageOrientation: 'landscape',
    pageMargins: [40, 60, 40, 60],
    content: [
      {
        columns: [
          {
            width: '*',
            stack: [
              { text: encabezado, style: 'header' },
              ...(nit ? [{ text: `NIT: ${nit}`, style: 'subheader' }] : []),
            ],
          },
          {
            width: 'auto',
            stack: [
              { text: 'REPORTE DE ÓRDENES DE DESPACHO', style: 'title', alignment: 'right' },
              {
                text: `Período: ${fechaInicial} - ${fechaFinal}`,
                style: 'normal',
                alignment: 'right',
              },
              {
                text: `Generado: ${new Date().toLocaleDateString('es-ES')}`,
                style: 'normal',
                alignment: 'right',
              },
            ],
          },
        ],
        margin: [0, 0, 0, 20],
      },
      { text: 'RESUMEN', style: 'sectionHeader', margin: [0, 10, 0, 5] },
      {
        columns: [
          {
            width: '25%',
            text: [{ text: 'Total Órdenes: ', bold: true }, estadisticas.total.toString()],
          },
          {
            width: '25%',
            text: [{ text: 'Pendientes: ', bold: true }, estadisticas.pendientes.toString()],
          },
          {
            width: '25%',
            text: [{ text: 'Completadas: ', bold: true }, estadisticas.completadas.toString()],
          },
          {
            width: '25%',
            text: [{ text: 'Total Monto: ', bold: true }, formatCurrency(estadisticas.totalMonto)],
          },
        ],
        margin: [0, 0, 0, 20],
      },
      { text: 'DETALLE DE ÓRDENES', style: 'sectionHeader', margin: [0, 10, 0, 5] },
      {
        table: { headerRows: 1, widths: anchos, body: tableRows },
        layout: {
          fillColor: (rowIndex) => (rowIndex === 0 ? '#CCCCCC' : null),
          hLineWidth: (i, node) => (i === 0 || i === 1 || i === node.table.body.length ? 1 : 0.5),
          vLineWidth: () => 0.5,
        },
      },
    ],
    styles: {
      header: { fontSize: 16, bold: true },
      subheader: { fontSize: 12, bold: true },
      title: { fontSize: 14, bold: true },
      sectionHeader: {
        fontSize: 12,
        bold: true,
        color: '#333333',
        fillColor: '#f0f0f0',
        margin: [0, 5, 0, 5],
      },
      tableHeader: { bold: true, fontSize: 9, color: 'black', alignment: 'center' },
      normal: { fontSize: 10 },
    },
    defaultStyle: { fontSize: 8 },
  }

  pdfMake.createPdf(docDefinition).download(nombreArchivo(opciones, 'pdf'))
}

const agrupar = (ordenes, clave, acumular) =>
  ordenes.reduce((acc, orden) => {
    const llave = clave(orden)
    return { ...acc, [llave]: acumular(acc[llave], orden) }
  }, {})

/**
 * @param {Array} ordenes
 * @param {OpcionesExportacion} opciones
 */
export const descargarOrdenesExcel = (ordenes, opciones) => {
  const { fechaInicial, fechaFinal, estadisticas, encabezado, nit, mostrarEstacion } = opciones
  const workbook = XLSX.utils.book_new()

  const resumenSheet = XLSX.utils.aoa_to_sheet([
    ['REPORTE DE ÓRDENES DE DESPACHO'],
    [''],
    [mostrarEstacion ? 'Cliente:' : 'Estación:', encabezado],
    ['NIT:', nit || 'N/A'],
    ['Período:', `${fechaInicial} - ${fechaFinal}`],
    ['Generado:', new Date().toLocaleDateString('es-ES')],
    [''],
    ['RESUMEN ESTADÍSTICAS'],
    ['Total Órdenes:', estadisticas.total],
    ['Pendientes:', estadisticas.pendientes],
    ['Facturadas:', estadisticas.anuladas],
    ['Completadas:', estadisticas.completadas],
    ['Total Monto:', estadisticas.totalMonto],
  ])
  resumenSheet['!cols'] = [{ width: 20 }, { width: 30 }]
  XLSX.utils.book_append_sheet(workbook, resumenSheet, 'Resumen General')

  const columnas = [
    ['ID Transacción', 15, (o) => o.numeroTransaccion || o.idVentaLocal || 'N/A'],
    ...(mostrarEstacion ? [['Estación', 22, (o) => o.estacionNombre || 'N/A']] : []),
    ['Fecha y Hora', 18, (o) => (o.fecha ? formatDate(o.fecha) : 'N/A')],
    ['Cliente', 25, (o) => o.nombreTercero || 'N/A'],
    ['Identificación', 15, (o) => o.identificacion || 'N/A'],
    ['Forma de Pago 1', 16, (o) => getFormaPago1(o)],
    ['Valor Pago 1', 14, (o) => getTotalPago1(o)],
    ['Forma de Pago 2', 16, (o) => getFormaPago2(o) || 'N/A'],
    ['Valor Pago 2', 14, (o) => getTotalPago2(o)],
    ['Combustible', 15, (o) => o.combustible || 'N/A'],
    ['Placa', 12, (o) => o.placa || 'N/A'],
    ['Cantidad', 10, (o) => o.cantidad || 0],
    ['Precio', 12, (o) => o.precio || 0],
    ['Consecutivo Factura', 18, (o) => facturaCampo(o, 'consecutivo')],
    ['CUFE', 35, (o) => facturaCampo(o, 'cufe')],
    ['Estado Factura', 15, (o) => facturaCampo(o, 'estado')],
    ['Estado', 12, (o) => getEstadoDisplay(o.estado)],
    ['Descuento', 14, (o) => o.descuento || 0],
    ['Total', 15, (o) => o.total || 0],
    ['Vendedor', 20, (o) => o.vendedor || 'N/A'],
  ]
  const ordenesSheet = XLSX.utils.aoa_to_sheet([
    columnas.map(([titulo]) => titulo),
    ...ordenes.map((orden) => columnas.map(([, , valor]) => valor(orden))),
  ])
  ordenesSheet['!cols'] = columnas.map(([, width]) => ({ width }))
  XLSX.utils.book_append_sheet(workbook, ordenesSheet, 'Detalle Órdenes')

  const porEstado = agrupar(
    ordenes,
    (o) => getEstadoDisplay(o.estado) || 'Sin Estado',
    (acc = { cantidad: 0, monto: 0 }, o) => ({
      cantidad: acc.cantidad + 1,
      monto: acc.monto + (o.total || 0),
    }),
  )
  const estadosSheet = XLSX.utils.aoa_to_sheet([
    ['ÓRDENES POR ESTADO'],
    [''],
    ['Estado', 'Cantidad', 'Monto Total'],
    ...Object.entries(porEstado).map(([estado, d]) => [estado, d.cantidad, d.monto]),
  ])
  estadosSheet['!cols'] = [{ width: 20 }, { width: 15 }, { width: 20 }]
  XLSX.utils.book_append_sheet(workbook, estadosSheet, 'Por Estado')

  const porCombustible = agrupar(
    ordenes,
    (o) => o.combustible || 'Sin Especificar',
    (acc = { cantidad: 0, litros: 0, monto: 0 }, o) => ({
      cantidad: acc.cantidad + 1,
      litros: acc.litros + (o.cantidad || 0),
      monto: acc.monto + (o.total || 0),
    }),
  )
  const combustibleSheet = XLSX.utils.aoa_to_sheet([
    ['ÓRDENES POR COMBUSTIBLE'],
    [''],
    ['Combustible', 'Cantidad', 'Litros Total', 'Monto Total'],
    ...Object.entries(porCombustible).map(([c, d]) => [c, d.cantidad, d.litros, d.monto]),
  ])
  combustibleSheet['!cols'] = [{ width: 20 }, { width: 15 }, { width: 15 }, { width: 20 }]
  XLSX.utils.book_append_sheet(workbook, combustibleSheet, 'Por Combustible')

  if (mostrarEstacion) {
    const porEstacion = agrupar(
      ordenes,
      (o) => o.estacionNombre || 'N/A',
      (acc = { cantidad: 0, monto: 0 }, o) => ({
        cantidad: acc.cantidad + 1,
        monto: acc.monto + (o.total || 0),
      }),
    )
    const estacionSheet = XLSX.utils.aoa_to_sheet([
      ['ÓRDENES POR ESTACIÓN'],
      [''],
      ['Estación', 'Cantidad', 'Monto Total'],
      ...Object.entries(porEstacion).map(([e, d]) => [e, d.cantidad, d.monto]),
    ])
    estacionSheet['!cols'] = [{ width: 25 }, { width: 15 }, { width: 20 }]
    XLSX.utils.book_append_sheet(workbook, estacionSheet, 'Por Estación')
  }

  XLSX.writeFile(workbook, nombreArchivo(opciones, 'xlsx'))
}

function facturaCampo(orden, campo) {
  const facturaInfo = formatIdFacturaElectronica(orden.idFacturaElectronica)
  return typeof facturaInfo === 'object' ? facturaInfo[campo] || 'N/A' : 'N/A'
}
