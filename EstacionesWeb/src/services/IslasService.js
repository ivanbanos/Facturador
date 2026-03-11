// ===============================================================================================================
// Islas Service - JavaScript implementation for islands management
// ===============================================================================================================

import HttpService from './HttpService.js'

class IslasService {
  constructor() {
    this.httpService = new HttpService()
    this.url = `${window.SERVER_URL}/Islas`
  }

  async getIslas() {
    try {
      return await this.httpService.get(this.url)
    } catch (error) {
      console.error('Error getting islas:', error)
      throw error
    }
  }

  async getIsla(idIsla) {
    try {
      return await this.httpService.get(`${this.url}/${idIsla}`)
    } catch (error) {
      console.error(`Error getting isla ${idIsla}:`, error)
      throw error
    }
  }

  async addOrUpdate(isla) {
    try {
      return await this.httpService.post(this.url, isla)
    } catch (error) {
      console.error('Error adding/updating isla:', error)
      throw error
    }
  }

  async updateIsla(idIsla, isla) {
    try {
      return await this.httpService.put(`${this.url}/${idIsla}`, isla)
    } catch (error) {
      console.error(`Error updating isla ${idIsla}:`, error)
      throw error
    }
  }

  async deleteIsla(idIsla) {
    try {
      return await this.httpService.delete(`${this.url}/${idIsla}`)
    } catch (error) {
      console.error(`Error deleting isla ${idIsla}:`, error)
      throw error
    }
  }

  async getIslasPorEstacion(idEstacion) {
    try {
      return await this.httpService.get(`${this.url}/estacion/${idEstacion}`)
    } catch (error) {
      console.error(`Error getting islas for station ${idEstacion}:`, error)
      throw error
    }
  }

  async getIslasActivas() {
    try {
      return await this.httpService.get(`${this.url}/activas`)
    } catch (error) {
      console.error('Error getting active islas:', error)
      throw error
    }
  }

  async cambiarEstadoIsla(idIsla, estado) {
    try {
      return await this.httpService.patch(`${this.url}/${idIsla}/estado`, { estado })
    } catch (error) {
      console.error(`Error changing state of isla ${idIsla}:`, error)
      throw error
    }
  }

  async asignarIslaAEstacion(idIsla, idEstacion) {
    try {
      return await this.httpService.patch(`${this.url}/${idIsla}/asignar`, { idEstacion })
    } catch (error) {
      console.error(`Error assigning isla ${idIsla} to station ${idEstacion}:`, error)
      throw error
    }
  }
}

export default IslasService
