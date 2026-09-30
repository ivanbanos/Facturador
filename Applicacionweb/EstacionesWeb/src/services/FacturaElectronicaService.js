import HttpService from './HttpService'

const httpService = new HttpService()

const FacturaElectronicaService = {
  async obtenerInformacionFacturaElectronica(idVentaLocal) {
    try {
      const estacionGuid = localStorage.getItem('estacionGuid')

      if (!estacionGuid || !idVentaLocal) {
        throw new Error('Estación o ID de venta local no encontrados')
      }

      const endpoint = `${window.SERVER_URL}/ManejadorInformacionLocal/GetInfoFacturaElectronica/${idVentaLocal}/estacion/${estacionGuid}`

      const response = await httpService.get(endpoint)

      if (response === 'fail') {
        throw new Error('Error de autenticación')
      }

      return response
    } catch (error) {
      console.error('Error al obtener información de factura electrónica:', error)
      throw error
    }
  },

  /**
   * Parsea la información de factura electrónica
   * @param {string} data - String con la información de factura electrónica
   * @returns {object} - Objeto con la información parseada
   */
  parseFacturaElectronicaData(data) {
    try {
      if (!data || typeof data !== 'string') {
        return null
      }

      console.log('Datos recibidos:', data) // Para debug

      // Dividir por saltos de línea (pueden ser \n, \r\n, o \r)
      const lines = data.split(/\r?\n/).filter((line) => line.trim() !== '')

      console.log('Líneas procesadas:', lines) // Para debug

      if (lines.length < 3) {
        return null
      }

      // Buscar el consecutivo (parece estar en la segunda línea no vacía)
      let consecutivo = ''
      let fecha = ''
      let cufe = ''

      // Buscar el consecutivo (formato FE seguido de números)
      const consecutivoMatch = data.match(/FE\d+/)
      if (consecutivoMatch) {
        consecutivo = consecutivoMatch[0]
      }

      // Buscar el CUFE (viene después de "CUFE:")
      const cufeMatch = data.match(/CUFE:\s*\n\s*([a-f0-9]+)/i)
      if (cufeMatch) {
        cufe = cufeMatch[1]
      }

      // Para la fecha, buscaremos un patrón de fecha o usaremos una línea específica
      // Por ahora, si no encontramos fecha, la dejamos vacía
      const fechaMatch = data.match(/(\d{1,2}\/\d{1,2}\/\d{4}|\d{4}-\d{2}-\d{2})/)
      if (fechaMatch) {
        fecha = fechaMatch[1]
      }

      return {
        consecutivo: consecutivo || '',
        fecha: fecha || '',
        cufe: cufe || '',
        qrUrl: cufe
          ? `https://catalogo-vpfe.dian.gov.co/User/SearchDocument?DocumentKey=${cufe}`
          : '',
      }
    } catch (error) {
      console.error('Error al parsear información de factura electrónica:', error)
      return null
    }
  },
}

export default FacturaElectronicaService
