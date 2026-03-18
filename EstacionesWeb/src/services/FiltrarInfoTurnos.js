import HttpService from './HttpService.js'

const FiltrarInfoTurnos = async (fechaInicial, fechaFinal) => {
  try {
    const httpService = new HttpService()
    const estacionGuid = localStorage.getItem('estacionGuid')

    const token = localStorage.getItem('token')
    if (!token) {
      console.error('No token found in localStorage')
      return 'fail'
    }

    const body = {
      fechaInicial: fechaInicial,
      fechaFinal: fechaFinal,
      estacion: estacionGuid,
    }

    // Use the correct API endpoint
    const url = `${window.SERVER_URL}/Turnos/GetTurnoReporte`
    const response = await httpService.post(url, body)

    // HttpService returns 'fail' for 401/403 errors
    if (response === 'fail') {
      console.error('Authentication failed for FiltrarInfoTurnos')
      return 'fail'
    }

    return response
  } catch (error) {
    console.error('FiltrarInfoTurnos error:', error)
    return 'fail'
  }
}

export const FiltrarInfoTurnosDia = async ({
  fecha,
  empleado,
  isla,
  numeroTurno,
  surtidor,
  manguera,
  combustible,
}) => {
  try {
    const httpService = new HttpService()
    const estacionGuid = localStorage.getItem('estacionGuid')

    const token = localStorage.getItem('token')
    if (!token) {
      console.error('No token found in localStorage')
      return 'fail'
    }

    const body = {
      fecha,
      estacion: estacionGuid,
      empleado: empleado || null,
      isla: isla || null,
      numeroTurno: numeroTurno || null,
      surtidor: surtidor || null,
      manguera: manguera || null,
      combustible: combustible || null,
    }

    const url = `${window.SERVER_URL}/Turnos/GetTurnoReporteDia`
    const response = await httpService.post(url, body)

    if (response === 'fail') {
      console.error('Authentication failed for FiltrarInfoTurnosDia')
      return 'fail'
    }

    return response
  } catch (error) {
    console.error('FiltrarInfoTurnosDia error:', error)
    return 'fail'
  }
}

export default FiltrarInfoTurnos
