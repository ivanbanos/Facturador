// ===============================================================================================================
// FacturasCanastillas Service - JavaScript implementation for basket invoices management
// Updated to match OpenAPI specification
// ===============================================================================================================

import HttpService from './HttpService.js'

class FacturasCanastillasService {
  constructor() {
    this.httpService = new HttpService()
    this.baseUrl = `${window.SERVER_URL}/FacturasCanastilla`
  }

  /**
   * Obtiene facturas usando el filtro de búsqueda
   * @param {Object} filtroBusqueda - Filtro de búsqueda
   * @returns {Promise<Array>} Lista de facturas canastilla
   */
  async getFacturasCanastillas(filtroBusqueda) {
    try {
      // Check for authentication token
      const token = localStorage.getItem('token')
      if (!token) {
        console.error('No authentication token found')
        return 'fail'
      }

      const response = await this.httpService.post(`${this.baseUrl}/GetFactura`, filtroBusqueda)
      return response
    } catch (error) {
      console.error('Error getting facturas canastilla:', error)
      throw error
    }
  }

  /**
   * Obtiene reporte de facturas con totales y detalles
   * @param {Object} filtroBusqueda - Filtro de búsqueda
   * @returns {Promise<Object>} Reporte de facturas canastilla
   */
  async getFacturasReporte(filtroBusqueda) {
    try {
      // Check for authentication token
      const token = localStorage.getItem('token')
      if (!token) {
        console.error('No authentication token found')
        return 'fail'
      }

      const response = await this.httpService.post(
        `${this.baseUrl}/GetFacturasReporte`,
        filtroBusqueda,
      )
      return response
    } catch (error) {
      console.error('Error getting facturas reporte:', error)
      throw error
    }
  }

  /**
   * Obtiene factura específica por ID
   * @param {string} idFactura - ID de la factura
   * @returns {Promise<Object>} Factura canastilla
   */
  async getFacturaCanastilla(idFactura) {
    try {
      const response = await this.httpService.get(`${this.baseUrl}/${idFactura}`)
      return response
    } catch (error) {
      console.error(`Error getting factura ${idFactura}:`, error)
      throw error
    }
  }

  /**
   * Obtiene detalle de factura
   * @param {string} idFactura - ID de la factura
   * @returns {Promise<Array>} Detalle de la factura
   */
  async getFacturaDetalle(idFactura) {
    try {
      const response = await this.httpService.get(`${this.baseUrl}/detalle/${idFactura}`)
      return response
    } catch (error) {
      console.error(`Error getting factura detalle ${idFactura}:`, error)
      throw error
    }
  }

  /**
   * Coloca factura en espera
   * @param {string} idFactura - ID de la factura
   * @param {string} idEstacion - GUID de la estación
   * @returns {Promise<number>} Resultado de la operación
   */
  async colocarFacturaEnEspera(idFactura, idEstacion) {
    try {
      const response = await this.httpService.get(
        `${this.baseUrl}/ColocarEspera/${idFactura}/Estacion/${idEstacion}`,
      )
      return response
    } catch (error) {
      console.error(`Error colocando factura en espera ${idFactura}:`, error)
      throw error
    }
  }

  /**
   * Obtiene facturas para imprimir
   * @param {string} idEstacion - GUID de la estación
   * @returns {Promise<number>} Número de facturas para imprimir
   */
  async getFacturasParaImprimir(idEstacion) {
    try {
      const response = await this.httpService.get(
        `${this.baseUrl}/ObtenerParaImprimir/Estacion/${idEstacion}`,
      )
      return response
    } catch (error) {
      console.error(`Error getting facturas para imprimir ${idEstacion}:`, error)
      throw error
    }
  }

  /**
   * Crea el filtro de búsqueda con los parámetros necesarios
   * @param {Date|string} fechaInicial - Fecha inicial
   * @param {Date|string} fechaFinal - Fecha final
   * @param {string} identificacion - Identificación del tercero
   * @param {string} nombreTercero - Nombre del tercero
   * @param {string} estacion - GUID de la estación
   * @returns {Object} Filtro de búsqueda
   */
  createFiltroBusqueda(
    fechaInicial,
    fechaFinal,
    identificacion = null,
    nombreTercero = null,
    estacion = null,
  ) {
    const estacionGuid = estacion || localStorage.getItem('estacionGuid')
    return {
      fechaInicial: fechaInicial ? new Date(fechaInicial).toISOString() : null,
      fechaFinal: fechaFinal ? new Date(fechaFinal).toISOString() : null,
      identificacion,
      nombreTercero,
      estacion: estacionGuid,
    }
  }

  /**
   * Valida el filtro de búsqueda
   * @param {Object} filtroBusqueda - Filtro a validar
   * @returns {boolean} True si es válido
   */
  validateFiltroBusqueda(filtroBusqueda) {
    if (!filtroBusqueda || typeof filtroBusqueda !== 'object') {
      return false
    }

    // La estación es requerida
    if (!filtroBusqueda.estacion) {
      return false
    }

    // Si se proporcionan fechas, ambas deben ser válidas
    if (filtroBusqueda.fechaInicial && filtroBusqueda.fechaFinal) {
      const fechaInicial = new Date(filtroBusqueda.fechaInicial)
      const fechaFinal = new Date(filtroBusqueda.fechaFinal)

      if (isNaN(fechaInicial.getTime()) || isNaN(fechaFinal.getTime())) {
        return false
      }

      if (fechaInicial > fechaFinal) {
        return false
      }
    }

    return true
  }

  // Métodos de compatibilidad con la implementación anterior
  async getFacturasCanastillasPorFecha(fechaInicio, fechaFin) {
    try {
      const filtro = this.createFiltroBusqueda(fechaInicio, fechaFin)
      return await this.getFacturasCanastillas(filtro)
    } catch (error) {
      console.error('Error getting facturas canastillas by date:', error)
      throw error
    }
  }

  async generarReporteCanastillas(parametros) {
    try {
      const filtro = this.createFiltroBusqueda(
        parametros.fechaInicial,
        parametros.fechaFinal,
        parametros.identificacion,
        parametros.nombreTercero,
        parametros.estacion,
      )

      if (!this.validateFiltroBusqueda(filtro)) {
        throw new Error('Filtro de búsqueda inválido')
      }

      return await this.getFacturasReporte(filtro)
    } catch (error) {
      console.error('Error generating facturas canastillas report:', error)
      throw error
    }
  }
}

export default FacturasCanastillasService
