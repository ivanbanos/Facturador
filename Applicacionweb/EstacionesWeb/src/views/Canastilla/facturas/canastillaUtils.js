// Formato, filtros y totales de las facturas de canastilla (compartido por la vista por estación
// y la vista de clientes en todas las estaciones)

export const FILTROS_VACIOS = {
  texto: '',
  estado: '',
  formaPago: '',
  estacion: '',
}

export const formatCurrency = (amount) =>
  new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    minimumFractionDigits: 0,
  }).format(amount || 0)

export const formatDate = (dateString) => {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleDateString('es-CO')
}

// El estado "Anulada" significa que la factura ya fue facturada electrónicamente
export const getEstadoDisplay = (estado) => {
  if (!estado) return 'Activa'
  const estadoLower = estado.toLowerCase()
  if (estadoLower === 'anulada' || estadoLower === 'anulado') {
    return 'Facturada'
  }
  return estado
}

export const getFormaPago = (factura) => factura.codigoFormaPago?.descripcion || ''

export const idFactura = (factura) => factura.id || factura.facturasCanastillaId

const coincideTexto = (factura, texto) => {
  if (!texto) return true
  const buscado = texto.toLowerCase()
  return [
    factura.consecutivo,
    factura.terceroId?.nombre,
    factura.terceroId?.identificacion,
    formatDate(factura.fecha),
    factura.estacionNombre,
    factura.placa,
  ].some(
    (valor) => valor !== undefined && valor !== null && `${valor}`.toLowerCase().includes(buscado),
  )
}

/**
 * Aplica los filtros de la pantalla sobre las facturas ya consultadas.
 * @param {Array} facturas
 * @param {typeof FILTROS_VACIOS} filtros
 */
export const filtrarFacturas = (facturas, filtros) =>
  facturas.filter(
    (factura) =>
      coincideTexto(factura, filtros.texto) &&
      (!filtros.estado || getEstadoDisplay(factura.estado) === filtros.estado) &&
      (!filtros.formaPago || getFormaPago(factura) === filtros.formaPago) &&
      (!filtros.estacion || factura.estacionNombre === filtros.estacion),
  )

const valoresUnicos = (valores) =>
  [...new Set(valores.filter(Boolean))].sort((a, b) => a.localeCompare(b, 'es'))

export const opcionesFiltroFacturas = (facturas) => ({
  estados: valoresUnicos(facturas.map((f) => getEstadoDisplay(f.estado))),
  formasPago: valoresUnicos(facturas.map(getFormaPago)),
  estaciones: valoresUnicos(facturas.map((f) => f.estacionNombre)),
})

export const calcularTotales = (facturas) =>
  facturas.reduce(
    (acc, factura) => ({
      cantidad: acc.cantidad + 1,
      subtotal: acc.subtotal + (factura.subtotal || 0),
      descuento: acc.descuento + (factura.descuento || 0),
      iva: acc.iva + (factura.iva || 0),
      total: acc.total + (factura.total || 0),
    }),
    { cantidad: 0, subtotal: 0, descuento: 0, iva: 0, total: 0 },
  )

export const hayFiltrosActivos = (filtros) =>
  Object.keys(FILTROS_VACIOS).some((clave) => Boolean(filtros[clave]))
