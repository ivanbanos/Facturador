// ===============================================================================================================
// OrdenesDespacho Service - JavaScript implementation for dispatch orders management
// Updated to match OpenAPI specification
// ===============================================================================================================

import HttpService from './HttpService'

class OrdenesDespachoService {
  constructor() {
    this.httpService = new HttpService()
    this.baseUrl = `${window.SERVER_URL}/OrdenesDeDespacho`
  }

  // Validar token antes de cualquier operación
  validateToken() {
    const token = localStorage.getItem('token')
    if (!token) {
      return false
    }
    return true
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
   * Obtener órdenes de despacho con filtro de búsqueda (POST /api/OrdenesDeDespacho)
   * @param {Object} filtroBusqueda - Filtro de búsqueda
   * @returns {Promise<Array>} Lista de órdenes de despacho
   */
  async getOrdenesDespacho(filtroBusqueda) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      const response = await this.httpService.post(this.baseUrl, filtroBusqueda)
      return response
    } catch (error) {
      console.error('Error fetching ordenes despacho:', error)
      return 'fail'
    }
  }

  /**
   * Obtener órdenes de despacho por rango de fechas (método de conveniencia)
   * @param {Date|string} fechaInicial - Fecha inicial
   * @param {Date|string} fechaFinal - Fecha final
   * @returns {Promise<Array>} Lista de órdenes de despacho
   */
  async getOrdenesByDateRange(fechaInicial, fechaFinal) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      const filtro = this.createFiltroBusqueda(fechaInicial, fechaFinal)
      return await this.getOrdenesDespacho(filtro)
    } catch (error) {
      console.error('Error fetching ordenes despacho by date range:', error)
      return 'fail'
    }
  }

  /**
   * Agregar órdenes para imprimir (POST /api/OrdenesDeDespacho/AddOrdenesImprimir)
   * @param {Array} ordenesGuids - Array de objetos con guid de órdenes
   * @returns {Promise<number>} Resultado de la operación
   */
  async addOrdenesImprimir(ordenesGuids) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      const url = `${this.baseUrl}/AddOrdenesImprimir`
      const response = await this.httpService.post(url, ordenesGuids)
      return response
    } catch (error) {
      console.error('Error adding ordenes to print:', error)
      return 'fail'
    }
  }

  /**
   * Anular órdenes (POST /api/OrdenesDeDespacho/AnularOrdenes)
   * @param {Array} ordenesGuids - Array de objetos con guid de órdenes
   * @returns {Promise<number>} Resultado de la operación
   */
  async anularOrdenes(ordenesGuids) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      const url = `${this.baseUrl}/AnularOrdenes`
      const response = await this.httpService.post(url, ordenesGuids)
      return response
    } catch (error) {
      console.error('Error canceling ordenes:', error)
      return 'fail'
    }
  }

  /**
   * Enviar orden a facturación por GUID (GET /api/OrdenesDeDespacho/EnviarFacturacion/{ordenGuid})
   * @param {string} ordenGuid - GUID de la orden
   * @returns {Promise<string>} Resultado de la operación
   */
  async enviarFacturacionPorGuid(ordenGuid) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      const url = `${this.baseUrl}/EnviarFacturacion/${ordenGuid}`
      const response = await this.httpService.get(url)
      return response
    } catch (error) {
      console.error('Error sending orden to billing:', error)
      return 'fail'
    }
  }

  /**
   * Enviar orden a facturación por ID de venta local (GET /api/OrdenesDeDespacho/EnviarFacturacion/{idVentaLocal}/{estacion})
   * @param {number} idVentaLocal - ID de venta local
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<string>} Resultado de la operación
   */
  async enviarFacturacionPorIdLocal(idVentaLocal, estacion = null) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      const estacionGuid = estacion || localStorage.getItem('estacionGuid')
      const url = `${this.baseUrl}/EnviarFacturacion/${idVentaLocal}/${estacionGuid}`
      const response = await this.httpService.get(url)
      return response
    } catch (error) {
      console.error('Error sending orden to billing by local ID:', error)
      return 'fail'
    }
  }

  /**
   * Crear factura para órdenes de despacho (POST /api/OrdenesDeDespacho/CrearFacturaOrdenesDeDespacho)
   * @param {Array} ordenesGuids - Array de objetos con guid de órdenes
   * @returns {Promise<string>} Resultado de la operación
   */
  async crearFacturaOrdenes(ordenesGuids) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      const url = `${this.baseUrl}/CrearFacturaOrdenesDeDespacho`
      const response = await this.httpService.post(url, ordenesGuids)
      return response
    } catch (error) {
      console.error('Error creating invoice for ordenes:', error)
      return 'fail'
    }
  }

  /**
   * Obtener orden de despacho por ID de venta local (GET /api/OrdenesDeDespacho/ObtenerOrdenDespachoPorIdVentaLocal/{idVentaLocal}/{estacion})
   * @param {number} idVentaLocal - ID de venta local
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<string>} GUID de la orden
   */
  async getOrdenPorIdVentaLocal(idVentaLocal, estacion = null) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      const estacionGuid = estacion || localStorage.getItem('estacionGuid')
      const url = `${this.baseUrl}/ObtenerOrdenDespachoPorIdVentaLocal/${idVentaLocal}/${estacionGuid}`
      const response = await this.httpService.get(url)
      return response
    } catch (error) {
      console.error('Error fetching orden by local sale ID:', error)
      return 'fail'
    }
  }

  /**
   * Obtener órdenes por turno (GET /api/OrdenesDeDespacho/ObtenerOrdenesPorTurno/{turno})
   * @param {string} turno - GUID del turno
   * @returns {Promise<Array>} Lista de órdenes de despacho
   */
  async getOrdenesPorTurno(turno) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      const url = `${this.baseUrl}/ObtenerOrdenesPorTurno/${turno}`
      const response = await this.httpService.get(url)
      return response
    } catch (error) {
      console.error('Error fetching ordenes by shift:', error)
      return 'fail'
    }
  }

  /**
   * Crear objeto para operaciones con GUIDs
   * @param {string} guid - GUID de la orden
   * @returns {Object} Objeto con GUID formateado
   */
  createOrdenGuidObject(guid) {
    return { guid: guid }
  }

  /**
   * Validar filtro de búsqueda
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

    return true
  }

  // ========================================================================
  // MÉTODOS ACTUALIZADOS PARA COMPATIBILIDAD CON OPENAPI
  // ========================================================================

  /**
   * Obtener orden por ID - Actualizado para usar filtro de búsqueda
   * @param {string} ordenId - ID de la orden (puede ser GUID o ID de venta)
   * @returns {Promise<Object>} Orden de despacho
   */
  async getOrdenById(ordenId) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      // Intentar obtener por filtro de búsqueda con estación
      const estacion = localStorage.getItem('estacionGuid')
      const filtro = this.createFiltroBusqueda(null, null, null, null, estacion)
      const response = await this.getOrdenesDespacho(filtro)

      if (response === 'fail' || !Array.isArray(response)) {
        console.warn('No se pudieron obtener órdenes para buscar por ID')
        return 'fail'
      }

      // Buscar la orden por ID en el array de resultados
      const orden = response.find(
        (orden) =>
          orden.guid === ordenId ||
          orden.id === ordenId ||
          orden.ordenId === ordenId ||
          orden.idVentaLocal === parseInt(ordenId),
      )

      return orden || 'fail'
    } catch (error) {
      console.error('Error fetching orden by ID:', error)
      return 'fail'
    }
  }

  /**
   * Obtener detalle de orden - Los detalles vienen incluidos en la respuesta principal
   * @param {string} ordenId - ID de la orden
   * @returns {Promise<Object>} Detalle de la orden
   */
  async getOrdenDetalle(ordenId) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      // Obtener la orden principal que ya incluye el detalle
      const orden = await this.getOrdenById(ordenId)

      if (orden === 'fail') {
        return 'fail'
      }

      // Los detalles están incluidos en la respuesta principal
      return {
        items: orden.items || orden.detalles || orden.productos || [],
        observaciones: orden.observaciones || '',
        ...orden,
      }
    } catch (error) {
      console.error('Error fetching orden details:', error)
      return 'fail'
    }
  }

  /**
   * Actualizar estado de orden - Usar anular para cancelar
   * @param {string} ordenId - ID de la orden
   * @param {string} nuevoEstado - Nuevo estado
   * @returns {Promise<string>} Resultado de la operación
   */
  async updateEstadoOrden(ordenId, nuevoEstado) {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      if (nuevoEstado === 'anulado' || nuevoEstado === 'cancelled') {
        // Usar el endpoint de anular órdenes
        const ordenGuid = await this.getOrdenGuidByIdOrden(ordenId)
        if (ordenGuid === 'fail') {
          return 'fail'
        }

        const ordenesGuids = [{ guid: ordenGuid }]
        return await this.anularOrdenes(ordenesGuids)
      } else {
        console.warn(
          'updateEstadoOrden: Only cancellation is supported. Use enviarFacturacionPorGuid for dispatch.',
        )
        return 'fail'
      }
    } catch (error) {
      console.error('Error updating orden status:', error)
      return 'fail'
    }
  }

  /**
   * Despachar orden - Usar envío a facturación
   * @param {string} ordenId - ID de la orden
   * @param {string} observaciones - Observaciones del despacho
   * @returns {Promise<string>} Resultado de la operación
   */
  async despacharOrden(ordenId, observaciones = '') {
    if (!this.validateToken()) {
      return 'fail'
    }

    try {
      // Obtener el GUID de la orden
      const ordenGuid = await this.getOrdenGuidByIdOrden(ordenId)
      if (ordenGuid === 'fail') {
        return 'fail'
      }

      // Enviar a facturación
      const result = await this.enviarFacturacionPorGuid(ordenGuid)

      if (result !== 'fail') {
        console.log(`Orden ${ordenId} despachada con observaciones: ${observaciones}`)
      }

      return result
    } catch (error) {
      console.error('Error dispatching orden:', error)
      return 'fail'
    }
  }

  /**
   * Obtener GUID de orden por ID de orden
   * @param {string} ordenId - ID de la orden
   * @returns {Promise<string>} GUID de la orden
   */
  async getOrdenGuidByIdOrden(ordenId) {
    try {
      const orden = await this.getOrdenById(ordenId)
      if (orden === 'fail') {
        return 'fail'
      }

      return orden.guid || orden.id || 'fail'
    } catch (error) {
      console.error('Error getting orden GUID:', error)
      return 'fail'
    }
  }
}

export default OrdenesDespachoService
