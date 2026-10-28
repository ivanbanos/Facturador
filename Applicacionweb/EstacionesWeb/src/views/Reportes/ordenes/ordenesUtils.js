// Formato, filtros y estadísticas de las órdenes de despacho (compartido por la vista por estación
// y la vista de clientes en todas las estaciones)

export const FILTROS_VACIOS = {
  texto: '',
  estado: '',
  combustible: '',
  formaPago: '',
  factura: '',
  estacion: '',
}

export const OPCIONES_FACTURA = [
  { valor: '', etiqueta: 'Todas' },
  { valor: 'con', etiqueta: 'Con factura electrónica' },
  { valor: 'sin', etiqueta: 'Sin factura electrónica' },
]

export const formatDate = (dateString) => {
  if (!dateString) return 'N/A'
  const date = new Date(dateString)
  return date.toLocaleDateString('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export const formatCurrency = (amount) =>
  new Intl.NumberFormat('es-ES', {
    style: 'currency',
    currency: 'COP',
  }).format(amount || 0)

export const getEstadoBadgeColor = (estado) => {
  switch (estado?.toLowerCase()) {
    case 'pendiente':
      return 'warning'
    case 'anulada':
    case 'anulado':
      return 'danger'
    case 'completado':
    case 'completada':
    case 'facturado':
    case 'facturada':
      return 'success'
    case 'en_proceso':
    case 'en proceso':
    case 'procesando':
      return 'info'
    default:
      return 'secondary'
  }
}

// El estado "Anulada" significa que la orden ya fue facturada
export const getEstadoDisplay = (estado) => {
  if (!estado) return 'N/A'
  const estadoLower = estado.toLowerCase()
  if (estadoLower === 'anulada' || estadoLower === 'anulado') {
    return 'Facturada'
  }
  return estado
}

/**
 * El formato es: Ok:EADM401708:7431586...
 * @returns {{completo: string, consecutivo: string, cufe: string, estado: string}|string}
 */
export const formatIdFacturaElectronica = (idFacturaElectronica) => {
  if (!idFacturaElectronica) return 'N/A'

  try {
    const parts = idFacturaElectronica.split(':')

    if (parts.length >= 3) {
      const [estado, consecutivo, cufe] = parts
      const cufeCorto = cufe.length > 10 ? cufe.substring(0, 10) + '...' : cufe

      return {
        completo: `${consecutivo} (${cufeCorto})`,
        consecutivo,
        cufe,
        estado,
      }
    }

    return 'Formato inválido'
  } catch (error) {
    return 'Error al procesar'
  }
}

export const tieneFacturaElectronica = (orden) => {
  const id = orden.idFacturaElectronica
  return Boolean(id) && (id.startsWith('Ok') || id.startsWith('ok'))
}

export const getFormaPago1 = (orden) =>
  orden.formaDePago || orden.formaPago || orden.FormaDePago || 'N/A'

export const getFormaPago2 = (orden) =>
  orden.formaDePago2 || orden.formaPago2 || orden.FormaDePago2 || ''

export const getTotalPago1 = (orden) => orden.total1 ?? orden.Total1 ?? orden.total ?? 0

export const getTotalPago2 = (orden) => orden.total2 ?? orden.Total2 ?? 0

export const getFormasPagoDisplay = (orden) => {
  const formaPago1 = getFormaPago1(orden)
  const formaPago2 = getFormaPago2(orden)
  const totalPago1 = getTotalPago1(orden)
  const totalPago2 = getTotalPago2(orden)

  if (!formaPago2 || Number(totalPago2) === 0) {
    return `${formaPago1}: ${formatCurrency(totalPago1)}`
  }

  return `${formaPago1}: ${formatCurrency(totalPago1)} / ${formaPago2}: ${formatCurrency(
    totalPago2,
  )}`
}

const coincideTexto = (orden, texto) => {
  if (!texto) return true
  const buscado = texto.toLowerCase()
  return [
    orden.numeroTransaccion,
    orden.idVentaLocal,
    orden.nombreTercero,
    orden.identificacion,
    orden.placa,
    orden.combustible,
    orden.vendedor,
    orden.estacionNombre,
  ].some(
    (valor) => valor !== undefined && valor !== null && `${valor}`.toLowerCase().includes(buscado),
  )
}

const usaFormaPago = (orden, formaPago) =>
  getFormaPago1(orden) === formaPago || getFormaPago2(orden) === formaPago

/**
 * Aplica los filtros de la pantalla sobre las órdenes ya consultadas.
 * @param {Array} ordenes
 * @param {typeof FILTROS_VACIOS} filtros
 */
export const filtrarOrdenes = (ordenes, filtros) =>
  ordenes.filter(
    (orden) =>
      coincideTexto(orden, filtros.texto) &&
      (!filtros.estado || getEstadoDisplay(orden.estado) === filtros.estado) &&
      (!filtros.combustible || orden.combustible === filtros.combustible) &&
      (!filtros.formaPago || usaFormaPago(orden, filtros.formaPago)) &&
      (!filtros.estacion || orden.estacionNombre === filtros.estacion) &&
      (filtros.factura !== 'con' || tieneFacturaElectronica(orden)) &&
      (filtros.factura !== 'sin' || !tieneFacturaElectronica(orden)),
  )

const valoresUnicos = (valores) =>
  [...new Set(valores.filter((valor) => valor && valor !== 'N/A'))].sort((a, b) =>
    a.localeCompare(b, 'es'),
  )

// Opciones de los selectores de filtro según las órdenes consultadas
export const opcionesFiltroOrdenes = (ordenes) => ({
  estados: valoresUnicos(ordenes.map((o) => getEstadoDisplay(o.estado))),
  combustibles: valoresUnicos(ordenes.map((o) => o.combustible)),
  formasPago: valoresUnicos(ordenes.flatMap((o) => [getFormaPago1(o), getFormaPago2(o)])),
  estaciones: valoresUnicos(ordenes.map((o) => o.estacionNombre)),
})

export const calcularEstadisticas = (ordenes) => ({
  total: ordenes.length,
  pendientes: ordenes.filter((o) => o.estado?.toLowerCase() === 'pendiente').length,
  anuladas: ordenes.filter((o) => o.estado?.toLowerCase() === 'anulada').length,
  completadas: ordenes.filter(
    (o) =>
      o.estado?.toLowerCase() === 'completado' ||
      o.estado?.toLowerCase() === 'facturado' ||
      (!o.estado?.toLowerCase().includes('anulad') &&
        !o.estado?.toLowerCase().includes('pendiente')),
  ).length,
  totalMonto: ordenes.reduce((sum, o) => sum + (o.total || 0), 0),
})

export const hayFiltrosActivos = (filtros) =>
  Object.keys(FILTROS_VACIOS).some((clave) => Boolean(filtros[clave]))
