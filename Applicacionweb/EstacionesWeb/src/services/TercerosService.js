// ===============================================================================================================
// Terceros Service - JavaScript implementation for third parties management
// Based on OpenAPI specification for /api/Terceros endpoints
// ===============================================================================================================

import HttpService from './HttpService.js'

class TercerosService {
  constructor() {
    this.httpService = new HttpService()
    this.baseUrl = `${window.SERVER_URL}`
  }

  /**
   * Obtiene todos los terceros (GET /api/Terceros)
   * @returns {Promise<Array>} Array de Tercero
   */
  async getTerceros() {
    try {
      return await this.httpService.get(`${this.baseUrl}/Terceros`)
    } catch (error) {
      console.error('Error getting terceros:', error)
      throw error
    }
  }

  /**
   * Obtiene un tercero por GUID (GET /api/Terceros/{guid})
   * @param {string} guid - GUID del tercero
   * @returns {Promise<Object>} Tercero
   */
  async getTercero(guid) {
    try {
      return await this.httpService.get(`${this.baseUrl}/Terceros/${guid}`)
    } catch (error) {
      console.error(`Error getting tercero ${guid}:`, error)
      throw error
    }
  }

  /**
   * Crea o actualiza terceros (POST /api/Terceros)
   * @param {Array} terceros - Array de terceros a crear/actualizar
   * @returns {Promise<number>} Número de terceros procesados
   */
  async addOrUpdateTerceros(terceros) {
    try {
      return await this.httpService.post(`${this.baseUrl}/Terceros`, terceros)
    } catch (error) {
      console.error('Error adding/updating terceros:', error)
      throw error
    }
  }

  /**
   * Valida si un tercero es válido por identificación (GET /api/Terceros/GetIsTerceroValidoPorIdentificacion/{identificacion})
   * @param {string} identificacion - Identificación del tercero
   * @returns {Promise<boolean>} True si es válido
   */
  async isTerceroValidoPorIdentificacion(identificacion) {
    try {
      return await this.httpService.get(
        `${this.baseUrl}/Terceros/GetIsTerceroValidoPorIdentificacion/${identificacion}`,
      )
    } catch (error) {
      console.error(`Error validating tercero with identificacion ${identificacion}:`, error)
      throw error
    }
  }

  /**
   * Sincroniza terceros (POST /api/Terceros/SincronizarTerceros)
   * @returns {Promise<void>}
   */
  async sincronizarTerceros() {
    try {
      return await this.httpService.post(`${this.baseUrl}/Terceros/SincronizarTerceros`)
    } catch (error) {
      console.error('Error synchronizing terceros:', error)
      throw error
    }
  }

  /**
   * Obtiene terceros actualizados por estación (GET /api/ManejadorInformacionLocal/GetTercerosActualizados/{estacion})
   * @param {string} estacion - GUID de la estación
   * @returns {Promise<Array>} Array de terceros actualizados
   */
  async getTercerosActualizadosPorEstacion(estacion) {
    try {
      return await this.httpService.get(
        `${this.baseUrl}/ManejadorInformacionLocal/GetTercerosActualizados/${estacion}`,
      )
    } catch (error) {
      console.error(`Error getting updated terceros for station ${estacion}:`, error)
      throw error
    }
  }

  /**
   * Obtiene todos los terceros actualizados (GET /api/ManejadorInformacionLocal/GetTercerosActualizados)
   * @returns {Promise<Array>} Array de terceros actualizados
   */
  async getTercerosActualizados() {
    try {
      return await this.httpService.get(
        `${this.baseUrl}/ManejadorInformacionLocal/GetTercerosActualizados`,
      )
    } catch (error) {
      console.error('Error getting all updated terceros:', error)
      throw error
    }
  }

  /**
   * Envía terceros al sistema (POST /api/ManejadorInformacionLocal/EnviarTerceros)
   * @param {Array} terceros - Array de terceros a enviar
   * @returns {Promise<void>}
   */
  async enviarTerceros(terceros) {
    try {
      return await this.httpService.post(
        `${this.baseUrl}/ManejadorInformacionLocal/EnviarTerceros`,
        terceros,
      )
    } catch (error) {
      console.error('Error sending terceros:', error)
      throw error
    }
  }

  // =================== MÉTODOS DE CONVENIENCIA PARA CRUD ===================

  /**
   * Crea un nuevo tercero
   * @param {Object} tercero - Datos del tercero
   * @returns {Promise<number>} Resultado de la operación
   */
  async crearTercero(tercero) {
    return this.addOrUpdateTerceros([tercero])
  }

  /**
   * Actualiza un tercero existente
   * @param {Object} tercero - Datos del tercero actualizado
   * @returns {Promise<number>} Resultado de la operación
   */
  async actualizarTercero(tercero) {
    return this.addOrUpdateTerceros([tercero])
  }

  /**
   * Busca terceros por identificación (filtro local)
   * @param {string} identificacion - Identificación a buscar
   * @returns {Promise<Array>} Terceros que coinciden
   */
  async buscarPorIdentificacion(identificacion) {
    try {
      const terceros = await this.getTerceros()
      return terceros.filter(
        (tercero) =>
          tercero.identificacion &&
          tercero.identificacion.toLowerCase().includes(identificacion.toLowerCase()),
      )
    } catch (error) {
      console.error('Error searching terceros by identificacion:', error)
      throw error
    }
  }

  /**
   * Busca terceros por nombre (filtro local)
   * @param {string} nombre - Nombre a buscar
   * @returns {Promise<Array>} Terceros que coinciden
   */
  async buscarPorNombre(nombre) {
    try {
      const terceros = await this.getTerceros()
      return terceros.filter((tercero) => {
        const nombreCompleto = `${tercero.nombre || ''} ${tercero.segundo || ''} ${
          tercero.apellidos || ''
        }`.trim()
        return nombreCompleto.toLowerCase().includes(nombre.toLowerCase())
      })
    } catch (error) {
      console.error('Error searching terceros by name:', error)
      throw error
    }
  }

  /**
   * Busca terceros por múltiples criterios
   * @param {Object} filtros - Objeto con criterios de búsqueda
   * @returns {Promise<Array>} Terceros que coinciden
   */
  async buscarTerceros(filtros = {}) {
    try {
      const terceros = await this.getTerceros()
      return terceros.filter((tercero) => {
        let cumple = true

        if (filtros.identificacion) {
          cumple =
            cumple &&
            tercero.identificacion &&
            tercero.identificacion.toLowerCase().includes(filtros.identificacion.toLowerCase())
        }

        if (filtros.nombre) {
          const nombreCompleto = `${tercero.nombre || ''} ${tercero.segundo || ''} ${
            tercero.apellidos || ''
          }`.trim()
          cumple = cumple && nombreCompleto.toLowerCase().includes(filtros.nombre.toLowerCase())
        }

        if (filtros.tipoPersona !== undefined) {
          cumple = cumple && tercero.tipoPersona === filtros.tipoPersona
        }

        if (filtros.municipio) {
          cumple =
            cumple &&
            tercero.municipio &&
            tercero.municipio.toLowerCase().includes(filtros.municipio.toLowerCase())
        }

        return cumple
      })
    } catch (error) {
      console.error('Error searching terceros:', error)
      throw error
    }
  }

  /**
   * Valida los datos de un tercero antes de enviar
   * @param {Object} tercero - Datos del tercero a validar
   * @returns {Object} Objeto con isValid y errores
   */
  validarTercero(tercero) {
    const errores = []

    if (!tercero.identificacion || tercero.identificacion.trim() === '') {
      errores.push('La identificación es requerida')
    }

    if (!tercero.nombre || tercero.nombre.trim() === '') {
      errores.push('El nombre es requerido')
    }

    if (tercero.tipoPersona === undefined || tercero.tipoPersona === null) {
      errores.push('El tipo de persona es requerido')
    }

    if (tercero.tipoIdentificacion === undefined || tercero.tipoIdentificacion === null) {
      errores.push('El tipo de identificación es requerido')
    }

    if (tercero.correo && !this.validarEmail(tercero.correo)) {
      errores.push('El formato del correo electrónico no es válido')
    }

    if (tercero.correo2 && !this.validarEmail(tercero.correo2)) {
      errores.push('El formato del segundo correo electrónico no es válido')
    }

    return {
      isValid: errores.length === 0,
      errores,
    }
  }

  /**
   * Valida formato de email
   * @param {string} email - Email a validar
   * @returns {boolean} True si es válido
   */
  validarEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return regex.test(email)
  }
}

export default TercerosService
