// ===============================================================================================================
// CanastillaItem Service - JavaScript implementation for basket item management
// ===============================================================================================================

import HttpService from './HttpService.js'

class CanastillaItemService {
  constructor() {
    this.httpService = new HttpService()
    this.url = `${window.SERVER_URL}/CanastillaItem`
  }

  async getCanastillaItems() {
    try {
      return await this.httpService.get(this.url)
    } catch (error) {
      console.error('Error getting canastilla items:', error)
      throw error
    }
  }

  async getCanastillaItem(id) {
    try {
      return await this.httpService.get(`${this.url}/${id}`)
    } catch (error) {
      console.error(`Error getting canastilla item ${id}:`, error)
      throw error
    }
  }

  async getCanastillaItemsByCanastillaId(canastillaId) {
    try {
      return await this.httpService.get(`${this.url}/canastilla/${canastillaId}`)
    } catch (error) {
      console.error(`Error getting items for canastilla ${canastillaId}:`, error)
      throw error
    }
  }

  async addOrUpdateCanastillaItem(item) {
    try {
      return await this.httpService.post(this.url, item)
    } catch (error) {
      console.error('Error adding/updating canastilla item:', error)
      throw error
    }
  }

  async updateCanastillaItem(id, item) {
    try {
      return await this.httpService.put(`${this.url}/${id}`, item)
    } catch (error) {
      console.error(`Error updating canastilla item ${id}:`, error)
      throw error
    }
  }

  async deleteCanastillaItem(id) {
    try {
      return await this.httpService.delete(`${this.url}/${id}`)
    } catch (error) {
      console.error(`Error deleting canastilla item ${id}:`, error)
      throw error
    }
  }

  async softDeleteCanastillaItem(id) {
    try {
      return await this.httpService.patch(`${this.url}/${id}/soft-delete`, { deleted: true })
    } catch (error) {
      console.error(`Error soft deleting canastilla item ${id}:`, error)
      throw error
    }
  }

  async restoreCanastillaItem(id) {
    try {
      return await this.httpService.patch(`${this.url}/${id}/restore`, { deleted: false })
    } catch (error) {
      console.error(`Error restoring canastilla item ${id}:`, error)
      throw error
    }
  }

  async getActiveCanastillaItems(canastillaId) {
    try {
      return await this.httpService.get(`${this.url}/canastilla/${canastillaId}/active`)
    } catch (error) {
      console.error(`Error getting active items for canastilla ${canastillaId}:`, error)
      throw error
    }
  }

  async bulkUpdateCanastillaItems(items) {
    try {
      return await this.httpService.post(`${this.url}/bulk`, items)
    } catch (error) {
      console.error('Error bulk updating canastilla items:', error)
      throw error
    }
  }

  // Utility method to calculate item total with IVA
  calculateItemTotal(precio, iva, cantidad = 1) {
    const subtotal = precio * cantidad
    const ivaAmount = subtotal * (iva / 100)
    return subtotal + ivaAmount
  }

  // Utility method to format price
  formatPrice(price) {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
    }).format(price)
  }
}

export default CanastillaItemService
