// ===============================================================================================================
// AuthService - manejo del token JWT en el navegador (sesion, expiracion y rol)
// ===============================================================================================================

import { FEATURE_USUARIOS_CLIENTES, isFeatureEnabled } from './FeatureFlags'

const TOKEN_KEY = 'token'
const ROLE_CLAIMS = ['role', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
const NAME_CLAIMS = [
  'unique_name',
  'name',
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
]

export const ROL_CLIENTE = 'Cliente'
export const ROL_ADMINISTRADOR = 'Administrador'

// Rutas que puede ver un usuario cliente (el resto redirige a la primera de esta lista)
export const RUTAS_CLIENTE = [
  '/OrdenesDespacho',
  '/OrdenDespachoDetalle',
  '/FacturasCanastilla',
  '/FacturaCanastillaDetalle',
  '/ReporteFiscal',
  '/CambiarContrasena',
]

// Rutas que solo existen con la funcionalidad usuarios-clientes activa
const RUTAS_USUARIOS_CLIENTES = ['/Usuarios', '/CambiarContrasena']

const coincideRuta = (pathname, rutas) => {
  const ruta = pathname.toLowerCase()
  return rutas.some((permitida) => {
    const base = permitida.toLowerCase()
    return ruta === base || ruta.startsWith(`${base}/`)
  })
}

export const usuariosClientesActivo = () => isFeatureEnabled(FEATURE_USUARIOS_CLIENTES)

const decodeBase64Url = (value) => {
  const base64 = value.replace(/-/g, '+').replace(/_/g, '/')
  const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4)
  const binary = atob(padded)
  const bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0))
  return new TextDecoder().decode(bytes)
}

const firstClaim = (payload, names) => {
  const key = names.find((name) => payload[name] !== undefined)
  const value = key ? payload[key] : null
  return Array.isArray(value) ? value[0] : value
}

export const getToken = () => {
  try {
    const token = localStorage.getItem(TOKEN_KEY)
    // Versiones anteriores guardaban el texto 'null' al cerrar sesion
    if (!token || token === 'null' || token === 'undefined') {
      return null
    }
    return token
  } catch {
    return null
  }
}

export const getPayload = () => {
  const token = getToken()
  if (!token) {
    return null
  }
  try {
    return JSON.parse(decodeBase64Url(token.split('.')[1]))
  } catch {
    return null
  }
}

// Milisegundos restantes antes de que expire el token (0 si no hay token o ya expiro)
export const getMsHastaExpiracion = () => {
  const payload = getPayload()
  if (!payload || !payload.exp) {
    return 0
  }
  return Math.max(0, payload.exp * 1000 - Date.now())
}

export const isAuthenticated = () => getMsHastaExpiracion() > 0

export const getRol = () => {
  const payload = getPayload()
  return payload ? firstClaim(payload, ROLE_CLAIMS) : null
}

export const isCliente = () => getRol() === ROL_CLIENTE

export const getNombreUsuario = () => {
  const payload = getPayload()
  return payload ? firstClaim(payload, NAME_CLAIMS) : null
}

export const getIdentificacionTercero = () => {
  const payload = getPayload()
  return payload ? payload.identificacionTercero || null : null
}

export const setToken = (token) => {
  localStorage.setItem(TOKEN_KEY, token)
}

export const clearSession = () => {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem('role')
  localStorage.removeItem('idClient')
}

// Ruta inicial segun el rol del usuario
export const getRutaInicio = () => (isCliente() ? RUTAS_CLIENTE[0] : '/dashboard')

export const puedeVerRuta = (pathname) => {
  if (!usuariosClientesActivo() && coincideRuta(pathname, RUTAS_USUARIOS_CLIENTES)) {
    return false
  }
  if (!isCliente()) {
    return true
  }
  return coincideRuta(pathname, RUTAS_CLIENTE)
}

// Cierra la sesion y lleva al inicio de sesion (HashRouter)
export const redirectToLogin = () => {
  clearSession()
  if (!window.location.hash.toLowerCase().startsWith('#/login')) {
    window.location.hash = '#/login'
  }
}
