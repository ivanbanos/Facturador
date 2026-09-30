// ===============================================================================================================
// ObtenerOrdenesSinFacturaCreditoDirecto Service - JavaScript implementation for getting dispatch orders without direct credit invoice
// Matches OpenAPI spec: POST /ObtenerOrdenesSinFacturaCreditoDirecto
// ===============================================================================================================

import HttpService from './HttpService.js'

/**
 * Service to fetch dispatch orders without direct credit invoice
 * @param {Object} filtroBusqueda - Object with filter fields (fechaInicial, fechaFinal, identificacion, nombreTercero, estacion)
 * @returns {Promise<Array<Object>|'fail'>} - Array of OrdenDeDespacho objects or 'fail' on error
 */
const ObtenerOrdenesSinFacturaCreditoDirecto = async (filtroBusqueda) => {
  const httpService = new HttpService()

  try {
    const token = localStorage.getItem('token')
    if (!token) {
      console.error('No authentication token found')
      return 'fail'
    }

    // Ensure estacion is present in filtroBusqueda
    if (!filtroBusqueda.estacion) {
      filtroBusqueda.estacion = localStorage.getItem('estacionGuid')
    }
    if (!filtroBusqueda.estacion) {
      console.error('No station GUID provided or found in localStorage')
      return 'fail'
    }

    const url = `${window.SERVER_URL}/OrdenesDeDespacho/ObtenerOrdenesSinFacturaCreditoDirecto`
    console.log(`POST to: ${url}`)
    const response = await httpService.post(url, filtroBusqueda)

    if (response === 'fail' || response === null || response === undefined) {
      console.warn('API returned fail, null, or undefined response')
      return 'fail'
    }

    return response
  } catch (error) {
    console.error('Error in ObtenerOrdenesSinFacturaCreditoDirecto:', error)
    return 'fail'
  }
}

export default ObtenerOrdenesSinFacturaCreditoDirecto
