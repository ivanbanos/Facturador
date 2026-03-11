// ===============================================================================================================
// User Service - JavaScript implementation for user management
// ===============================================================================================================

import HttpService from './HttpService.js'
import GuidService from './GuidService.js'

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
    localStorage.clear()
  }

  getCurrentUser() {
    return localStorage.getItem('username')
  }

  getToken() {
    return localStorage.getItem('token')
  }

  isLoggedIn() {
    const token = this.getToken()
    return token !== null && token !== undefined && token !== ''
  }
}

export default UserService
