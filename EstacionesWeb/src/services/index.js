// ===============================================================================================================
// Services Index - JavaScript exports for all services
// ===============================================================================================================

// Base Services
export { default as HttpService } from './HttpService.js'
export { default as GuidService } from './GuidService.js'
export { default as HttpAuthInterceptor } from './HttpAuthInterceptor.js'

// Business Services
export { default as UserService } from './UserService.js'
export { default as CanastillasService } from './CanastillasService.js'
export { default as CanastillaItemService } from './CanastillaItemService.js'
export { default as EstacionService } from './EstacionService.js'
export { default as EstacionGuardService } from './EstacionGuardService.js'
export { default as ClientesService } from './ClientesService.js'
export { default as FacturaService } from './FacturaService.js'
export { default as FacturasCanastillasService } from './FacturasCanastillasService.js'
export { default as IslasService } from './IslasService.js'
export { default as OrdenDeDespachoService } from './OrdenDeDespachoService.js'
export { default as ResolucionService } from './ResolucionService.js'
export { default as TercerosService } from './TercerosService.js'
export { default as TipoIdentificacionService } from './TipoIdentificacionService.js'

// Legacy service exports (for backward compatibility)
export { default as GetToken } from './GetToken.js'
export { default as Filtrarfacturas } from './Filtrarfacturas.js'
export { default as FiltrarInfoCanastilla } from './FiltrarInfoCanastilla.js'
export { default as FiltrarInfoTurnos } from './FiltrarInfoTurnos.js'
export { default as FiltrarOrdenes } from './FiltrarOrdenes.js'
export { default as GetFacturasCanastilla } from './GetFacturasCanastilla.js'
export { default as ObtenerInfoCanastilla } from './ObtenerInfoCanastilla.js'
export { default as ReporteFiscal } from './ReporteFiscal.js'

// Service Factory for creating service instances
export class ServiceFactory {
  static createHttpService() {
    return new HttpService()
  }

  static createGuidService() {
    return new GuidService()
  }

  static createAuthInterceptor() {
    return new HttpAuthInterceptor()
  }

  static createUserService() {
    return new UserService()
  }

  static createCanastillasService() {
    return new CanastillasService()
  }

  static createCanastillaItemService() {
    return new CanastillaItemService()
  }

  static createEstacionService() {
    return new EstacionService()
  }

  static createEstacionGuardService() {
    return new EstacionGuardService()
  }

  static createClientesService() {
    return new ClientesService()
  }

  static createFacturaService() {
    return new FacturaService()
  }

  static createFacturasCanastillasService() {
    return new FacturasCanastillasService()
  }

  static createIslasService() {
    return new IslasService()
  }

  static createOrdenDeDespachoService() {
    return new OrdenDeDespachoService()
  }

  static createResolucionService() {
    return new ResolucionService()
  }

  static createTercerosService() {
    return new TercerosService()
  }

  static createTipoIdentificacionService() {
    return new TipoIdentificacionService()
  }
}

// Default export with all services
export default {
  // New Services
  HttpService,
  GuidService,
  HttpAuthInterceptor,
  UserService,
  CanastillasService,
  CanastillaItemService,
  EstacionService,
  EstacionGuardService,
  ClientesService,
  FacturaService,
  FacturasCanastillasService,
  IslasService,
  OrdenDeDespachoService,
  ResolucionService,
  TercerosService,
  TipoIdentificacionService,
  ServiceFactory,

  // Legacy Services
  GetToken,
  Filtrarfacturas,
  FiltrarInfoCanastilla,
  FiltrarInfoTurnos,
  FiltrarOrdenes,
  GetFacturasCanastilla,
  ObtenerInfoCanastilla,
  ReporteFiscal,
}
