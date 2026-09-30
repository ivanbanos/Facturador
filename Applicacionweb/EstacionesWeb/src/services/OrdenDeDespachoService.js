// ===============================================================================================================
// OrdenDeDespacho Service - JavaScript implementation for dispatch order management
// Matches the OpenAPI specification exactly
// ===============================================================================================================

import HttpService from './HttpService.js'

class OrdenDeDespachoService {
  constructor() {
    this.httpService = new HttpService()
    this.baseUrl = `${window.SERVER_URL}`
  }

  /**
   * Obtiene las órdenes de despacho usando el filtro de búsqueda (POST /api/OrdenesDeDespacho)
   * @param {Object} filtroBusqueda - Objeto con fechaInicial, fechaFinal, identificacion, nombreTercero, estacion
   * @returns {Promise<Array>} Array de OrdenDeDespacho
   */
  async getOrdenesDeDespacho(filtroBusqueda = {}) {
    try {
      return await this.httpService.post(`${this.baseUrl}/OrdenesDeDespacho`, filtroBusqueda)
    } catch (error) {
      console.error('Error getting ordenes de despacho:', error)
      throw error
    }
  }

  /**
   * Agrega órdenes para imprimir (POST /api/OrdenesDeDespacho/AddOrdenesImprimir)
   * @param {Array} ordenes - Array de FacturasEntity (con guid)
   * @returns {Promise<number>} Número de órdenes agregadas
   */
  async addOrdenesImprimir(ordenes) {
    try {
      return await this.httpService.post(
        `${this.baseUrl}/OrdenesDeDespacho/AddOrdenesImprimir`,
        ordenes,
      )
    } catch (error) {
      console.error('Error adding ordenes to print queue:', error)
      throw error
    }
  }

  /**
   * Anula órdenes (POST /api/OrdenesDeDespacho/AnularOrdenes)
   * @param {Array} ordenes - Array de FacturasEntity (con guid)
   * @returns {Promise<number>} Número de órdenes anuladas
   */
  async anularOrdenes(ordenes) {
    try {
      return await this.httpService.post(`${this.baseUrl}/OrdenesDeDespacho/AnularOrdenes`, ordenes)
    } catch (error) {
      console.error('Error cancelling ordenes:', error)
      throw error
    }
  }

  /**
   * Envía facturación por GUID de orden (GET /api/OrdenesDeDespacho/EnviarFacturacion/{ordenGuid})
   * @param {string} ordenGuid - GUID de la orden
   * @returns {Promise<string>} Resultado del envío
   */
  async enviarFacturacionPorGuid(ordenGuid) {
    try {
      return await this.httpService.get(
        `${this.baseUrl}/OrdenesDeDespacho/EnviarFacturacion/${ordenGuid}`,
      )
    } catch (error) {
      console.error(`Error sending facturation for orden ${ordenGuid}:`, error)
      throw error
    }
  }

  /**
   * Envía facturación por ID de venta local (GET /api/OrdenesDeDespacho/EnviarFacturacion/{idVentaLocal}/{estacion})
   * @param {number} idVentaLocal - ID de la venta local
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<string>} Resultado del envío
   */
  async enviarFacturacionPorIdVenta(idVentaLocal, estacion) {
    try {
      return await this.httpService.get(
        `${this.baseUrl}/OrdenesDeDespacho/EnviarFacturacion/${idVentaLocal}/${estacion}`,
      )
    } catch (error) {
      console.error(`Error sending facturation for venta ${idVentaLocal}:`, error)
      throw error
    }
  }

  /**
   * Crea factura para órdenes de despacho (POST /api/OrdenesDeDespacho/CrearFacturaOrdenesDeDespacho)
   * @param {Array} ordenesGuids - Array de OrdenesDeDespachoGuids
   * @returns {Promise<string>} GUID de la factura creada
   */
  async crearFacturaOrdenesDeDespacho(ordenesGuids) {
    try {
      return await this.httpService.post(
        `${this.baseUrl}/OrdenesDeDespacho/CrearFacturaOrdenesDeDespacho`,
        ordenesGuids,
      )
    } catch (error) {
      console.error('Error creating factura for ordenes de despacho:', error)
      throw error
    }
  }

  /**
   * Obtiene orden de despacho por ID de venta local (GET /api/OrdenesDeDespacho/ObtenerOrdenDespachoPorIdVentaLocal/{idVentaLocal}/{estacion})
   * @param {number} idVentaLocal - ID de la venta local
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<string>} GUID de la orden
   */
  async obtenerOrdenPorIdVentaLocal(idVentaLocal, estacion) {
    try {
      return await this.httpService.get(
        `${this.baseUrl}/OrdenesDeDespacho/ObtenerOrdenDespachoPorIdVentaLocal/${idVentaLocal}/${estacion}`,
      )
    } catch (error) {
      console.error(`Error getting orden for venta ${idVentaLocal}:`, error)
      throw error
    }
  }

  /**
   * Obtiene órdenes por turno (GET /api/OrdenesDeDespacho/ObtenerOrdenesPorTurno/{turno})
   * @param {string} turno - GUID del turno
   * @returns {Promise<Array>} Array de OrdenDeDespacho
   */
  async obtenerOrdenesPorTurno(turno) {
    try {
      return await this.httpService.get(
        `${this.baseUrl}/OrdenesDeDespacho/ObtenerOrdenesPorTurno/${turno}`,
      )
    } catch (error) {
      console.error(`Error getting ordenes for turno ${turno}:`, error)
      throw error
    }
  }

  /**
   * Obtiene órdenes para imprimir por estación (desde ManejadorInformacionLocal)
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<Array>} Array de OrdenDeDespacho
   */
  async getOrdenesDeDespachoImprimir(estacion) {
    try {
      return await this.httpService.get(
        `${this.baseUrl}/ManejadorInformacionLocal/GetOrdenesDeDespachoImprimir/${estacion}`,
      )
    } catch (error) {
      console.error(`Error getting ordenes to print for station ${estacion}:`, error)
      throw error
    }
  }

  // =================== MÉTODOS DE CONVENIENCIA ===================

  /**
   * Método de conveniencia para obtener órdenes por rango de fechas
   * @param {Date|string} fechaInicial - Fecha inicial
   * @param {Date|string} fechaFinal - Fecha final
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<Array>} Array de OrdenDeDespacho
   */
  async getOrdenesPorFecha(fechaInicial, fechaFinal, estacion) {
    const filtroBusqueda = {
      fechaInicial: fechaInicial instanceof Date ? fechaInicial.toISOString() : fechaInicial,
      fechaFinal: fechaFinal instanceof Date ? fechaFinal.toISOString() : fechaFinal,
      estacion,
    }
    return this.getOrdenesDeDespacho(filtroBusqueda)
  }

  /**
   * Método de conveniencia para obtener órdenes por identificación
   * @param {string} identificacion - Identificación del tercero
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<Array>} Array de OrdenDeDespacho
   */
  async getOrdenesPorIdentificacion(identificacion, estacion) {
    const filtroBusqueda = {
      identificacion,
      estacion,
    }
    return this.getOrdenesDeDespacho(filtroBusqueda)
  }

  /**
   * Método de conveniencia para obtener órdenes por nombre de tercero
   * @param {string} nombreTercero - Nombre del tercero
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<Array>} Array de OrdenDeDespacho
   */
  async getOrdenesPorNombreTercero(nombreTercero, estacion) {
    const filtroBusqueda = {
      nombreTercero,
      estacion,
    }
    return this.getOrdenesDeDespacho(filtroBusqueda)
  }
}

export default OrdenDeDespachoService
