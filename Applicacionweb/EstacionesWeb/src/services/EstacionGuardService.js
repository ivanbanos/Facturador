// ===============================================================================================================
// Estacion Guard Service - JavaScript implementation for route protection
// ===============================================================================================================

import EstacionService from './EstacionService.js'

class EstacionGuardService {
  constructor() {
    this.estacionService = new EstacionService()
  }

  canActivate() {
    const estacionId = this.estacionService.obtenerEstacion()

    if (!estacionId) {
      console.warn('No station selected. Access denied.')
      return false
    }

    return true
  }

  async canActivateAsync() {
    const estacionId = this.estacionService.obtenerEstacion()

    if (!estacionId) {
      console.warn('No station selected. Access denied.')
      return false
    }

    try {
      // Optionally verify the station still exists
      const estacion = await this.estacionService.getEstacion(estacionId)
      return estacion !== null && estacion !== undefined
    } catch (error) {
      console.error('Error verifying station:', error)
      return false
    }
  }

  redirectToStationSelection() {
    // This would typically redirect to a station selection page
    // Implementation depends on your routing system
    console.log('Redirecting to station selection...')
    // window.location.href = '/select-station';
  }

  checkStationAccess(callback) {
    if (this.canActivate()) {
      callback()
    } else {
      this.redirectToStationSelection()
    }
  }

  async checkStationAccessAsync(callback) {
    const hasAccess = await this.canActivateAsync()

    if (hasAccess) {
      callback()
    } else {
      this.redirectToStationSelection()
    }
  }
}

export default EstacionGuardService
