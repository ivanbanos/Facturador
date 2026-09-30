// ===============================================================================================================
// Canastillas Service - JavaScript implementation for basket management
// Updated to match OpenAPI specification
// ===============================================================================================================

import HttpService from './HttpService.js'

class CanastillasService {
  constructor() {
    this.httpService = new HttpService()
    this.baseUrl = `${window.SERVER_URL}/Canastilla`
  }

  /**
   * Obtiene todas las canastillas para una estación específica
   * @param {string} estacion - UUID de la estación
   * @returns {Promise<Array>} Lista de canastillas
   */
  async getCanastillas(estacion = null) {
    try {
      const url = estacion ? `${this.baseUrl}?estacion=${estacion}` : this.baseUrl
      return await this.httpService.get(url)
    } catch (error) {
      console.error('Error getting canastillas:', error)
      throw error
    }
  }

  /**
   * Obtiene una canastilla específica por su GUID
   * @param {string} guid - GUID de la canastilla
   * @returns {Promise<Object>} Canastilla
   */
  async getCanastilla(guid) {
    try {
      return await this.httpService.get(`${this.baseUrl}/${guid}`)
    } catch (error) {
      console.error(`Error getting canastilla ${guid}:`, error)
      throw error
    }
  }

  /**
   * Crea o actualiza múltiples canastillas
   * @param {Array} canastillas - Array de canastillas
   * @returns {Promise<number>} Número de canastillas procesadas
   */
  async createOrUpdateCanastillas(canastillas) {
    try {
      if (!Array.isArray(canastillas)) {
        throw new Error('El parámetro debe ser un array de canastillas')
      }
      return await this.httpService.post(this.baseUrl, canastillas)
    } catch (error) {
      console.error('Error creating/updating canastillas:', error)
      throw error
    }
  }

  /**
   * Obtiene canastillas por estación (método de conveniencia)
   * @param {string} estacionGuid - GUID de la estación
   * @returns {Promise<Array>} Lista de canastillas
   */
  async getCanastillasByEstacion(estacionGuid) {
    try {
      return await this.getCanastillas(estacionGuid)
    } catch (error) {
      console.error('Error getting canastillas by estacion:', error)
      throw error
    }
  }

  /**
   * Valida una canastilla antes de enviarla
   * @param {Object} canastilla - Canastilla a validar
   * @returns {boolean} True si es válida
   */
  validateCanastilla(canastilla) {
    if (!canastilla) return false

    const required = ['descripcion', 'precio', 'estacion']
    return required.every((field) => canastilla.hasOwnProperty(field) && canastilla[field] !== null)
  }

  /**
   * Prepara una canastilla para envío a la API
   * @param {Object} canastilla - Canastilla a preparar
   * @returns {Object} Canastilla preparada
   */
  prepareCanastillaForAPI(canastilla) {
    return {
      canastillaId: canastilla.canastillaId || 0,
      descripcion: canastilla.descripcion || '',
      unidad: canastilla.unidad || '',
      precio: parseFloat(canastilla.precio) || 0,
      deleted: Boolean(canastilla.deleted),
      guid: canastilla.guid || null,
      iva: parseInt(canastilla.iva) || 0,
      campoextra: canastilla.campoextra || null,
      estacion: canastilla.estacion || null,
    }
  }

  // Métodos de compatibilidad con la implementación anterior
  async addOrUpdate(canastillas) {
    return await this.createOrUpdateCanastillas(canastillas)
  }
}

export default CanastillasService
