import HttpService from './HttpService.js'

const ReporteFiscal = async (fechaInicial, fechaFinal) => {
  try {
    const httpService = new HttpService()
    const estacionGuid = localStorage.getItem('estacionGuid')

    const body = {
      fechaInicial: fechaInicial,
      fechaFinal: fechaFinal,
      estacion: estacionGuid,
    }

    const token = localStorage.getItem('token')
    if (!token) {
      console.error('No token found in localStorage')
      return 'fail'
    }

    // Use HttpService to make the request with proper headers and error handling
    const url = `${window.SERVER_URL}/OrdenesDeDespacho/GetConsolidado`
    const response = await httpService.post(url, body)

    // HttpService returns 'fail' for 401/403 errors
    if (response === 'fail') {
      console.error('Authentication failed for ReporteFiscal')
      return 'fail'
    }
    return response
  } catch (error) {
    console.error('ReporteFiscal error:', error)
    return 'fail'
  }
}

export default ReporteFiscal
