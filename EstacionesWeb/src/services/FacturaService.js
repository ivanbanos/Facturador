// ===============================================================================================================
// Factura Service - JavaScript implementation for invoice management
// ===============================================================================================================

import HttpService from './HttpService.js'

class FacturaService {
  constructor() {
    this.httpService = new HttpService()
    this.url = `${window.SERVER_URL}/Facturas`
  }

  async getFacturas() {
    try {
      return await this.httpService.get(this.url)
    } catch (error) {
      console.error('Error getting facturas:', error)
      throw error
    }
  }

  async getFactura(idFactura) {
    try {
      return await this.httpService.get(`${this.url}/${idFactura}`)
    } catch (error) {
      console.error(`Error getting factura ${idFactura}:`, error)
      throw error
    }
  }

  async addOrUpdate(factura) {
    try {
      return await this.httpService.post(this.url, factura)
    } catch (error) {
      console.error('Error adding/updating factura:', error)
      throw error
    }
  }

  async updateFactura(idFactura, factura) {
    try {
      return await this.httpService.put(`${this.url}/${idFactura}`, factura)
    } catch (error) {
      console.error(`Error updating factura ${idFactura}:`, error)
      throw error
    }
  }

  async deleteFactura(idFactura) {
    try {
      return await this.httpService.delete(`${this.url}/${idFactura}`)
    } catch (error) {
      console.error(`Error deleting factura ${idFactura}:`, error)
      throw error
    }
  }

  async getFacturasPorFecha(fechaInicio, fechaFin) {
    try {
      const params = { fechaInicio, fechaFin }
      return await this.httpService.get(`${this.url}/fecha`, params)
    } catch (error) {
      console.error('Error getting facturas by date:', error)
      throw error
    }
  }

  async getFacturasPorCliente(idCliente) {
    try {
      return await this.httpService.get(`${this.url}/cliente/${idCliente}`)
    } catch (error) {
      console.error(`Error getting facturas for client ${idCliente}:`, error)
      throw error
    }
  }

  async anularFactura(idFactura, motivo) {
    try {
      return await this.httpService.post(`${this.url}/${idFactura}/anular`, { motivo })
    } catch (error) {
      console.error(`Error cancelling factura ${idFactura}:`, error)
      throw error
    }
  }

  async generarReporte(parametros) {
    try {
      return await this.httpService.post(`${this.url}/reporte`, parametros)
    } catch (error) {
      console.error('Error generating factura report:', error)
      throw error
    }
  }
}

export default FacturaService
