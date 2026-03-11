// ===============================================================================================================
// ReenviarOrdenesDespachoPorIdVentaLocal Service - JavaScript implementation for re-sending dispatch orders
// Matches OpenAPI spec: POST /ReenviarOrdenesDespachoPorIdVentaLocal
// ===============================================================================================================

import HttpService from './HttpService.js'

/**
 * Service to re-send dispatch orders by idVentaLocal list for the current station
 * @param {Array<number>} idVentaLocalList - Array of idVentaLocal integers
 * @param {string|null} estacionGuid - UUID of the station (optional, defaults to localStorage)
 * @returns {Promise<Array<string>|'fail'>} - Array of results or 'fail' on error
 */
const ReenviarOrdenesDespachoPorIdVentaLocal = async (idVentaLocalList, estacionGuid = null) => {
  const httpService = new HttpService()

  try {
    const token = localStorage.getItem('token')
    if (!token) {
      console.error('No authentication token found')
      return 'fail'
    }

    const estacion = estacionGuid || localStorage.getItem('estacionGuid')
    if (!estacion) {
      console.error('No station GUID provided or found in localStorage')
      return 'fail'
    }

    // Prepare request body according to OpenAPI spec
    const body = {
      idVentaLocalList,
      estacion,
    }

    const url = `${window.SERVER_URL}/ReenviarOrdenesDespachoPorIdVentaLocal`
    console.log(`POST to: ${url}`)
    const response = await httpService.post(url, body)

    if (response === 'fail' || response === null || response === undefined) {
      console.warn('API returned fail, null, or undefined response')
      return 'fail'
    }

    return response
  } catch (error) {
    console.error('Error in ReenviarOrdenesDespachoPorIdVentaLocal:', error)
    return 'fail'
  }
}

export default ReenviarOrdenesDespachoPorIdVentaLocal
