// Exportación a PDF de las facturas de canastilla filtradas
import { formatCurrency, formatDate } from './canastillaUtils'

var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

const derecha = (valor) => ({ text: formatCurrency(valor), style: 'tableRight' })

const seccion = (titulo, cuerpo) =>
  cuerpo.length > 0
    ? [
        { text: '\n' },
        { text: titulo, style: 'header' },
        { style: 'tableExample', table: { headerRows: 1, body: cuerpo } },
      ]
    : []

/**
 * @param {Array} facturas facturas filtradas
 * @param {Object} opciones
 * @param {Object} opciones.reporte respuesta del API (detalleFormaPago, detalleArticulo)
 * @param {Object} opciones.totales resultado de calcularTotales
 * @param {string} opciones.fechaInicial
 * @param {string} opciones.fechaFinal
 * @param {string} opciones.encabezado nombre que va en el encabezado (estación o cliente)
 * @param {string} [opciones.nit]
 * @param {boolean} [opciones.mostrarEstacion] agrega la columna Estación
 */
export const descargarCanastillaPDF = (facturas, opciones) => {
  const { reporte, totales, fechaInicial, fechaFinal, encabezado, nit, mostrarEstacion } = opciones
  const today = new Date().toLocaleDateString('es-CO')

  const facturasTable = [
    [
      'Consecutivo',
      ...(mostrarEstacion ? ['Estación'] : []),
      'Fecha',
      'Tercero',
      'Subtotal',
      'Descuento',
      'IVA',
      'Total',
    ],
    ...facturas.map((factura) => [
      factura.consecutivo || '-',
      ...(mostrarEstacion ? [factura.estacionNombre || '-'] : []),
      formatDate(factura.fecha),
      factura.terceroId?.nombre || '-',
      derecha(factura.subtotal),
      derecha(factura.descuento),
      derecha(factura.iva),
      derecha(factura.total),
    ]),
  ]

  const formasPagoTable = reporte?.detalleFormaPago?.length
    ? [
        ['Forma de Pago', 'Cantidad', 'Total'],
        ...reporte.detalleFormaPago.map((forma) => [
          forma.formaDePago || '-',
          forma.cantidad || 0,
          derecha(forma.precio),
        ]),
      ]
    : []

  const articulosTable = reporte?.detalleArticulo?.length
    ? [
        ['Artículo', 'Cantidad', 'Subtotal', 'IVA', 'Total'],
        ...reporte.detalleArticulo.map((articulo) => [
          articulo.descripcion || '-',
          articulo.cantidad || 0,
          derecha(articulo.subtotal),
          derecha(articulo.iva),
          derecha(articulo.total),
        ]),
      ]
    : []

  const anchos = facturasTable[0].map((titulo) => (titulo === 'Tercero' ? '*' : 'auto'))

  const docDefinition = {
    watermark: { text: 'SIGES Soluciones', color: 'blue', opacity: 0.1, bold: true },
    footer: (currentPage, pageCount) =>
      `SIGES Soluciones - ${encabezado} - Página ${currentPage} de ${pageCount}`,
    content: [
      {
        stack: [encabezado, ...(nit ? [{ text: `NIT ${nit}`, style: 'subheader' }] : [])],
        style: 'header',
      },
      { text: 'Reporte de Facturas de Canastilla', style: 'header' },
      {
        text: `Período: ${formatDate(fechaInicial)} - ${formatDate(fechaFinal)}`,
        style: 'subheader',
      },
      { text: '\n' },
      { text: 'Resumen General', style: 'header' },
      {
        columns: [
          { text: `Cantidad de Facturas: ${totales.cantidad}` },
          { text: `Total General: ${formatCurrency(totales.total)}` },
        ],
      },
      {
        columns: [
          { text: `Subtotal: ${formatCurrency(totales.subtotal)}` },
          { text: `Descuento: ${formatCurrency(totales.descuento)}` },
        ],
      },
      { text: `IVA Total: ${formatCurrency(totales.iva)}` },
      { text: '\n' },
      { text: 'Detalle de Facturas', style: 'header' },
      {
        style: 'tableExample',
        table: { headerRows: 1, widths: anchos, body: facturasTable },
      },
      ...seccion('Detalle por Forma de Pago', formasPagoTable),
      ...seccion('Detalle por Artículo', articulosTable),
    ],
    styles: {
      header: { fontSize: 18, bold: true, alignment: 'center' },
      subheader: { fontSize: 14, alignment: 'center' },
      tableExample: { margin: [0, 5, 0, 15] },
      tableRight: { alignment: 'right' },
      tableHeader: { bold: true, fontSize: 13, color: 'black' },
    },
  }

  pdfMake.createPdf(docDefinition).download(`FacturasCanastilla-${encabezado}-${today}.pdf`)
}
