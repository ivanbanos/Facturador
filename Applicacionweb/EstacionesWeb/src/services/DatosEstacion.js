// Datos de la estación para los encabezados de impresión. Por defecto es la estación seleccionada
// (localStorage); si la factura es de otra estación (vista de clientes en todas las estaciones) se
// buscan los datos de esa estación.

import EstacionService from './EstacionService'

export const datosEstacionSeleccionada = () => ({
  nombre: localStorage.getItem('estacionNombre') || 'Estación',
  nit: localStorage.getItem('estacionNit') || '',
  direccion: localStorage.getItem('estacionDireccion') || '',
  telefono: localStorage.getItem('estacionTelefono') || '',
})

const mismoGuid = (a, b) => Boolean(a) && Boolean(b) && a.toLowerCase() === b.toLowerCase()

/**
 * @param {string} [estacionGuid] estación de la factura
 * @returns {Promise<{nombre: string, nit: string, direccion: string, telefono: string}>}
 */
export const obtenerDatosEstacion = async (estacionGuid) => {
  const seleccionada = datosEstacionSeleccionada()
  if (!estacionGuid || mismoGuid(estacionGuid, localStorage.getItem('estacionGuid'))) {
    return seleccionada
  }

  try {
    const estaciones = await new EstacionService().getEstaciones()
    const estacion = Array.isArray(estaciones)
      ? estaciones.find((e) => mismoGuid(e.guid, estacionGuid))
      : null
    if (!estacion) {
      return seleccionada
    }
    return {
      nombre: estacion.nombre || 'Estación',
      nit: estacion.nit || '',
      direccion: estacion.direccion || '',
      telefono: estacion.telefono || '',
    }
  } catch (error) {
    console.error('Error obteniendo datos de la estación:', error)
    return seleccionada
  }
}
