// ===============================================================================================================
// User Service - JavaScript implementation for user management
// ===============================================================================================================

import HttpService from './HttpService.js'
import GuidService from './GuidService.js'
import { clearSession, getToken, isAuthenticated } from './AuthService'

class UserService {
  constructor() {
    this.httpService = new HttpService()
    this.guidService = new GuidService()
    this.url = window.SERVER_URL + '/usuarios'
  }

  async login(username, password) {
    try {
      const response = await this.httpService.getResource(this.url, username, password)

      if (response && response.token) {
        localStorage.setItem('token', response.token)
        localStorage.setItem('username', username)
        return response.token
      }

      throw new Error('Invalid login response')
    } catch (error) {
      console.error('Login error:', error)
      throw error
    }
  }

  async register(username, name, password) {
    try {
      const user = {
        id: this.guidService.generateGuid(),
        username: username,
        name: name,
        password: password,
      }

      return await this.httpService.post(this.url, user)
    } catch (error) {
      console.error('Registration error:', error)
      throw error
    }
  }

  logout() {
    clearSession()
  }

  getCurrentUser() {
    return localStorage.getItem('username')
  }

  getToken() {
    return getToken()
  }

  isLoggedIn() {
    return isAuthenticated()
  }

  // Lista de usuarios (solo administradores)
  async listar() {
    return this.httpService.get(this.url)
  }

  // Crea un usuario; con identificacionTercero = cliente; sin ella, rol Administrador u Operador
  async crear({ nombre, usuario, contrasena, identificacionTercero, rol }) {
    return this.enviar(`${this.url}/Crear`, {
      nombre,
      usuario,
      contrasena,
      identificacionTercero: identificacionTercero || null,
      rol: identificacionTercero ? null : rol || null,
    })
  }

  // Actualiza un usuario; contrasena vacia = se conserva la actual
  async actualizar(guid, { nombre, usuario, contrasena, identificacionTercero, rol }) {
    return this.enviar(
      `${this.url}/${guid}`,
      {
        nombre,
        usuario,
        contrasena: contrasena || null,
        identificacionTercero: identificacionTercero || null,
        rol: identificacionTercero ? null : rol || null,
      },
      'PUT',
    )
  }

  // Habilita (activo = true) o inhabilita (activo = false) un usuario
  async cambiarEstado(guid, activo) {
    return this.enviar(`${this.url}/${guid}/Estado`, { activo })
  }

  // Eliminacion logica (soft delete)
  async eliminar(guid) {
    return this.enviar(`${this.url}/${guid}`, undefined, 'DELETE')
  }

  async cambiarContrasena(contrasenaActual, contrasenaNueva) {
    return this.enviar(`${this.url}/CambiarContrasena`, { contrasenaActual, contrasenaNueva })
  }

  // Peticion que devuelve { ok, data, message } para poder mostrar los errores de validacion del API
  async enviar(url, body, method = 'POST') {
    if (!this.httpService.ensureSession()) {
      return { ok: false, message: 'La sesión expiró' }
    }
    const response = await fetch(url, {
      method,
      mode: 'cors',
      headers: this.httpService.getHeaders(),
      body: body === undefined ? undefined : JSON.stringify(body),
    })
    if (this.httpService.handleUnauthorized(response)) {
      return { ok: false, message: 'La sesión expiró' }
    }
    const data = await response.json().catch(() => null)
    if (response.ok) {
      return { ok: true, data }
    }
    return { ok: false, message: this.extraerMensaje(data, response.status) }
  }

  extraerMensaje(data, status) {
    if (data?.message) {
      return data.message
    }
    if (data?.errors) {
      return Object.values(data.errors).flat().join(' ')
    }
    if (status === 403) {
      return 'No tiene permisos para esta acción'
    }
    return `Error ${status}`
  }
}

export default UserService
