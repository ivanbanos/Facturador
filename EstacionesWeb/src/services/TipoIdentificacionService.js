// ===============================================================================================================
// TipoIdentificacion Service - JavaScript implementation for identification type management
// ===============================================================================================================

import HttpService from './HttpService.js'

class TipoIdentificacionService {
  constructor() {
    this.httpService = new HttpService()
    this.url = `${window.SERVER_URL}/TipoIdentificacion`
  }

  async getTiposIdentificacion() {
    try {
      return await this.httpService.get(this.url)
    } catch (error) {
      console.error('Error getting tipos identificacion:', error)
      throw error
    }
  }

  async getTipoIdentificacion(idTipo) {
    try {
      return await this.httpService.get(`${this.url}/${idTipo}`)
    } catch (error) {
      console.error(`Error getting tipo identificacion ${idTipo}:`, error)
      throw error
    }
  }

  async addOrUpdate(tipoIdentificacion) {
    try {
      return await this.httpService.post(this.url, tipoIdentificacion)
    } catch (error) {
      console.error('Error adding/updating tipo identificacion:', error)
      throw error
    }
  }

  async updateTipoIdentificacion(idTipo, tipoIdentificacion) {
    try {
      return await this.httpService.put(`${this.url}/${idTipo}`, tipoIdentificacion)
    } catch (error) {
      console.error(`Error updating tipo identificacion ${idTipo}:`, error)
      throw error
    }
  }

  async deleteTipoIdentificacion(idTipo) {
    try {
      return await this.httpService.delete(`${this.url}/${idTipo}`)
    } catch (error) {
      console.error(`Error deleting tipo identificacion ${idTipo}:`, error)
      throw error
    }
  }

  async getTiposIdentificacionActivos() {
    try {
      return await this.httpService.get(`${this.url}/activos`)
    } catch (error) {
      console.error('Error getting active tipos identificacion:', error)
      throw error
    }
  }

  async getTiposPorCategoria(categoria) {
    try {
      return await this.httpService.get(`${this.url}/categoria/${categoria}`)
    } catch (error) {
      console.error(`Error getting tipos by category ${categoria}:`, error)
      throw error
    }
  }

  async validarTipoIdentificacion(codigo) {
    try {
      return await this.httpService.get(`${this.url}/validar/${codigo}`)
    } catch (error) {
      console.error(`Error validating tipo identificacion ${codigo}:`, error)
      throw error
    }
  }

  async cambiarEstadoTipo(idTipo, estado) {
    try {
      return await this.httpService.patch(`${this.url}/${idTipo}/estado`, { estado })
    } catch (error) {
      console.error(`Error changing state of tipo identificacion ${idTipo}:`, error)
      throw error
    }
  }

  // Utility methods for common identification types
  getTiposComunes() {
    return [
      { codigo: 'CC', nombre: 'Cédula de Ciudadanía' },
      { codigo: 'CE', nombre: 'Cédula de Extranjería' },
      { codigo: 'TI', nombre: 'Tarjeta de Identidad' },
      { codigo: 'NIT', nombre: 'Número de Identificación Tributaria' },
      { codigo: 'PP', nombre: 'Pasaporte' },
      { codigo: 'RC', nombre: 'Registro Civil' },
    ]
  }

  validarFormatoDocumento(tipoDocumento, numeroDocumento) {
    // Basic validation rules - can be extended
    const validationRules = {
      CC: /^\d{6,10}$/,
      CE: /^\d{6,10}$/,
      TI: /^\d{6,10}$/,
      NIT: /^\d{9,10}$/,
      PP: /^[A-Z0-9]{6,12}$/,
      RC: /^\d{10,11}$/,
    }

    const rule = validationRules[tipoDocumento]
    if (!rule) {
      return { valido: false, mensaje: 'Tipo de documento no reconocido' }
    }

    const esValido = rule.test(numeroDocumento)
    return {
      valido: esValido,
      mensaje: esValido ? 'Documento válido' : 'Formato de documento inválido',
    }
  }
}

export default TipoIdentificacionService
