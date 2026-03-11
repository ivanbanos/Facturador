// ===============================================================================================================
// CuposInfo Service - JavaScript implementation for credit limit information management
// Based on OpenAPI specification
// ===============================================================================================================

import HttpService from './HttpService.js'

class CuposInfoService {
  constructor() {
    this.httpService = new HttpService()
    this.baseUrl = `${window.SERVER_URL}/CuposInfo`
  }

  /**
   * Envía información de cupos (automotores y clientes) al servidor
   * @param {Object} cuposRequest - Objeto con cupos de automotores y clientes
   * @returns {Promise<number>} Resultado de la operación
   */
  async enviarCuposInfo(cuposRequest) {
    try {
      if (!this.validateCuposRequest(cuposRequest)) {
        throw new Error('Estructura de cupos inválida')
      }

      const response = await this.httpService.post(this.baseUrl, cuposRequest)
      return response
    } catch (error) {
      console.error('Error sending cupos info:', error)
      throw error
    }
  }

  /**
   * Obtiene cupos por automotores para una estación
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<number>} Número de cupos de automotores
   */
  async getCuposPorAutomotores(estacion) {
    try {
      const response = await this.httpService.get(`${this.baseUrl}/Automotores/${estacion}`)
      return response
    } catch (error) {
      console.error('Error getting cupos automotores:', error)
      throw error
    }
  }

  /**
   * Obtiene cupos por clientes para una estación
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<number>} Número de cupos de clientes
   */
  async getCuposPorClientes(estacion) {
    try {
      const response = await this.httpService.get(`${this.baseUrl}/Clientes/${estacion}`)
      return response
    } catch (error) {
      console.error('Error getting cupos clientes:', error)
      throw error
    }
  }

  /**
   * Valida la estructura del request de cupos
   * @param {Object} cuposRequest - Request a validar
   * @returns {boolean} True si es válido
   */
  validateCuposRequest(cuposRequest) {
    if (!cuposRequest || typeof cuposRequest !== 'object') {
      return false
    }

    // Validar que tenga estacionGuid
    if (!cuposRequest.estacionGuid) {
      return false
    }

    // Validar cuposAutomotores si existe
    if (cuposRequest.cuposAutomotores && !Array.isArray(cuposRequest.cuposAutomotores)) {
      return false
    }

    // Validar cuposClientes si existe
    if (cuposRequest.cuposClientes && !Array.isArray(cuposRequest.cuposClientes)) {
      return false
    }

    return true
  }

  /**
   * Crea un objeto CupoAutomotor válido
   * @param {Object} cupo - Datos del cupo
   * @returns {Object} Cupo automotor formateado
   */
  createCupoAutomotor(cupo) {
    return {
      cliente: cupo.cliente || '',
      coD_CLI: cupo.coD_CLI || '',
      nit: cupo.nit || '',
      placa: cupo.placa || '',
      cupoAsignado: parseFloat(cupo.cupoAsignado) || 0,
      cupoDisponible: parseFloat(cupo.cupoDisponible) || 0,
      estacionGuid: cupo.estacionGuid || '',
    }
  }

  /**
   * Crea un objeto CupoCliente válido
   * @param {Object} cupo - Datos del cupo
   * @returns {Object} Cupo cliente formateado
   */
  createCupoCliente(cupo) {
    return {
      cliente: cupo.cliente || '',
      coD_CLI: cupo.coD_CLI || '',
      nit: cupo.nit || '',
      cupoAsignado: parseFloat(cupo.cupoAsignado) || 0,
      cupoDisponible: parseFloat(cupo.cupoDisponible) || 0,
      estacionGuid: cupo.estacionGuid || '',
    }
  }

  /**
   * Crea un request de cupos completo
   * @param {Array} cuposAutomotores - Array de cupos de automotores
   * @param {Array} cuposClientes - Array de cupos de clientes
   * @param {string} estacionGuid - GUID de la estación
   * @returns {Object} Request de cupos formateado
   */
  createCuposRequest(cuposAutomotores = [], cuposClientes = [], estacionGuid) {
    return {
      cuposAutomotores: cuposAutomotores.map((cupo) => this.createCupoAutomotor(cupo)),
      cuposClientes: cuposClientes.map((cupo) => this.createCupoCliente(cupo)),
      estacionGuid: estacionGuid || localStorage.getItem('estacionGuid'),
    }
  }
}

export default CuposInfoService
