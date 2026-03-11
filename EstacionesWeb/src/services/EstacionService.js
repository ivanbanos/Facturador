// ===============================================================================================================
// Estacion Service - JavaScript implementation for station management
// ===============================================================================================================

import HttpService from './HttpService.js'

class EstacionService {
  constructor() {
    this.httpService = new HttpService()
    this.url = `${window.SERVER_URL}/Estaciones`
    this.ESTACION_KEY = 'estacion'
    this.ESTACION_NAME = 'estacion_nombre'
    this.observers = new Map()
  }

  async getEstaciones() {
    try {
      return await this.httpService.get(this.url)
    } catch (error) {
      console.error('Error getting estaciones:', error)
      throw error
    }
  }

  async getEstacion(idEstacion) {
    try {
      return await this.httpService.get(`${this.url}/${idEstacion}`)
    } catch (error) {
      console.error(`Error getting estacion ${idEstacion}:`, error)
      throw error
    }
  }

  async borrarEstaciones(estaciones) {
    try {
      return await this.httpService.post(`${this.url}/BorrarEstaciones`, estaciones)
    } catch (error) {
      console.error('Error deleting estaciones:', error)
      throw error
    }
  }

  async addOrUpdate(estacion) {
    try {
      // The API endpoint expects an array of estaciones
      return await this.httpService.post(this.url, [estacion])
    } catch (error) {
      console.error('Error adding/updating estacion:', error)
      throw error
    }
  }

  guardarSeleccion(estacion) {
    localStorage.setItem(this.ESTACION_KEY, estacion.guid)
    this.setItem(this.ESTACION_NAME, estacion.nombre)
  }

  obtenerEstacion() {
    return localStorage.getItem(this.ESTACION_KEY)
  }

  obtenerNombreEstacion() {
    return this.getItem(this.ESTACION_NAME)
  }

  borrarEstacionSeleccionada() {
    localStorage.removeItem(this.ESTACION_KEY)
    localStorage.removeItem(this.ESTACION_NAME)
  }

  // Local storage observer pattern implementation
  getItem(identifier) {
    const item = localStorage.getItem(identifier)
    this.notifyObservers(identifier, item)
    return item
  }

  setItem(identifier, value) {
    localStorage.setItem(identifier, value)
    this.notifyObservers(identifier, value)
  }

  // Simple observer pattern for localStorage changes
  subscribe(identifier, callback) {
    if (!this.observers.has(identifier)) {
      this.observers.set(identifier, [])
    }
    this.observers.get(identifier).push(callback)
  }

  unsubscribe(identifier, callback) {
    if (this.observers.has(identifier)) {
      const callbacks = this.observers.get(identifier)
      const index = callbacks.indexOf(callback)
      if (index > -1) {
        callbacks.splice(index, 1)
      }
    }
  }

  notifyObservers(identifier, value) {
    if (this.observers.has(identifier)) {
      this.observers.get(identifier).forEach((callback) => {
        try {
          callback(value)
        } catch (error) {
          console.error('Error in observer callback:', error)
        }
      })
    }
  }
}

export default EstacionService
