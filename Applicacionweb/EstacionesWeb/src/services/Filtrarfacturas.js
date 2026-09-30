import HttpService from './HttpService.js'

const FiltrarFacturas = async (fechaInicial, fechaFinal, identificacion) => {
  try {
    const httpService = new HttpService()
    const estacionGuid = localStorage.getItem('estacionGuid')

    const token = localStorage.getItem('token')
    if (!token) {
      console.error('No token found in localStorage')
      return 'fail'
    }

    const body = {
      fechaInicial,
      fechaFinal,
      identificacion,
      estacion: estacionGuid,
    }

    // Use the correct API endpoint
    const url = `${window.SERVER_URL}/Factura/GetFactura`
    const response = await httpService.post(url, body)

    // HttpService returns 'fail' for 401/403 errors
    if (response === 'fail') {
      console.error('Authentication failed for FiltrarFacturas')
      return 'fail'
    }

    return response
  } catch (error) {
    console.error('FiltrarFacturas error:', error)
    return 'fail'
  }
}

export default FiltrarFacturas
