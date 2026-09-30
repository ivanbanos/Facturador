// ===============================================================================================================
// Clientes Service - JavaScript implementation for client management
// ===============================================================================================================

import HttpService from './HttpService.js'

class ClientesService {
  constructor() {
    this.httpService = new HttpService()
    this.url = `${window.SERVER_URL}/Clientes`
  }

  async getClientes() {
    try {
      return await this.httpService.get(this.url)
    } catch (error) {
      console.error('Error getting clientes:', error)
      throw error
    }
  }

  async getCliente(idCliente) {
    try {
      return await this.httpService.get(`${this.url}/${idCliente}`)
    } catch (error) {
      console.error(`Error getting cliente ${idCliente}:`, error)
      throw error
    }
  }

  async addOrUpdate(cliente) {
    try {
      return await this.httpService.post(this.url, cliente)
    } catch (error) {
      console.error('Error adding/updating cliente:', error)
      throw error
    }
  }

  async updateCliente(idCliente, cliente) {
    try {
      return await this.httpService.put(`${this.url}/${idCliente}`, cliente)
    } catch (error) {
      console.error(`Error updating cliente ${idCliente}:`, error)
      throw error
    }
  }

  async deleteCliente(idCliente) {
    try {
      return await this.httpService.delete(`${this.url}/${idCliente}`)
    } catch (error) {
      console.error(`Error deleting cliente ${idCliente}:`, error)
      throw error
    }
  }

  async buscarClientes(criterio) {
    try {
      return await this.httpService.get(`${this.url}/buscar`, { criterio })
    } catch (error) {
      console.error('Error searching clientes:', error)
      throw error
    }
  }

  async getClientesPorTipo(tipo) {
    try {
      return await this.httpService.get(`${this.url}/tipo/${tipo}`)
    } catch (error) {
      console.error(`Error getting clientes by type ${tipo}:`, error)
      throw error
    }
  }
}

export default ClientesService
