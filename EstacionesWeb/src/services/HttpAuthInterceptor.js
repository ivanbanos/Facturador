// ===============================================================================================================
// HTTP Auth Interceptor - JavaScript implementation for handling authentication
// ===============================================================================================================

class HttpAuthInterceptor {
  constructor() {
    this.tokenKey = 'token'
  }

  // Add authorization token to headers
  addAuthHeader(headers = {}) {
    const token = this.getToken()
    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }
    return headers
  }

  // Get token from localStorage
  getToken() {
    return localStorage.getItem(this.tokenKey)
  }

  // Set token in localStorage
  setToken(token) {
    localStorage.setItem(this.tokenKey, token)
  }

  // Remove token from localStorage
  removeToken() {
    localStorage.removeItem(this.tokenKey)
  }

  // Check if user is authenticated
  isAuthenticated() {
    const token = this.getToken()
    return token !== null && token !== undefined && token !== ''
  }

  // Intercept fetch requests to add auth headers
  async intercept(url, options = {}) {
    const headers = this.addAuthHeader(options.headers || {})

    const requestOptions = {
      ...options,
      headers: headers,
    }

    try {
      const response = await fetch(url, requestOptions)

      // Handle 401 Unauthorized responses
      if (response.status === 401) {
        this.removeToken()
        // Redirect to login or handle unauthorized access
        console.warn('Unauthorized access detected. Token removed.')
      }

      return response
    } catch (error) {
      console.error('Auth interceptor error:', error)
      throw error
    }
  }
}

export default HttpAuthInterceptor
