// ===============================================================================================================
// Resolucion Service - JavaScript implementation for resolution management
// ===============================================================================================================

import HttpService from './HttpService.js'

class ResolucionService {
  constructor() {
    this.httpService = new HttpService()
    this.url = `${window.SERVER_URL}/Resoluciones`
  }

  async getResoluciones() {
    try {
      return await this.httpService.get(this.url)
    } catch (error) {
      console.error('Error getting resoluciones:', error)
      throw error
    }
  }

  async getResolucion(idResolucion) {
    try {
      return await this.httpService.get(`${this.url}/${idResolucion}`)
    } catch (error) {
      console.error(`Error getting resolucion ${idResolucion}:`, error)
      throw error
    }
  }

  async addOrUpdate(resolucion) {
    try {
      return await this.httpService.post(this.url, resolucion)
    } catch (error) {
      console.error('Error adding/updating resolucion:', error)
      throw error
    }
  }

  async updateResolucion(idResolucion, resolucion) {
    try {
      return await this.httpService.put(`${this.url}/${idResolucion}`, resolucion)
    } catch (error) {
      console.error(`Error updating resolucion ${idResolucion}:`, error)
      throw error
    }
  }

  async deleteResolucion(idResolucion) {
    try {
      return await this.httpService.delete(`${this.url}/${idResolucion}`)
    } catch (error) {
      console.error(`Error deleting resolucion ${idResolucion}:`, error)
      throw error
    }
  }

  async getResolucionesPorTipo(tipo) {
    try {
      return await this.httpService.get(`${this.url}/tipo/${tipo}`)
    } catch (error) {
      console.error(`Error getting resoluciones by type ${tipo}:`, error)
      throw error
    }
  }

  async getResolucionesActivas() {
    try {
      return await this.httpService.get(`${this.url}/activas`)
    } catch (error) {
      console.error('Error getting active resoluciones:', error)
      throw error
    }
  }

  async getResolucionesPorFecha(fechaInicio, fechaFin) {
    try {
      const params = { fechaInicio, fechaFin }
      return await this.httpService.get(`${this.url}/fecha`, params)
    } catch (error) {
      console.error('Error getting resoluciones by date:', error)
      throw error
    }
  }

  async cambiarEstadoResolucion(idResolucion, estado) {
    try {
      return await this.httpService.patch(`${this.url}/${idResolucion}/estado`, { estado })
    } catch (error) {
      console.error(`Error changing state of resolucion ${idResolucion}:`, error)
      throw error
    }
  }

  async validarResolucion(idResolucion) {
    try {
      return await this.httpService.post(`${this.url}/${idResolucion}/validar`)
    } catch (error) {
      console.error(`Error validating resolucion ${idResolucion}:`, error)
      throw error
    }
  }
}

export default ResolucionService
