// ===============================================================================================================
// HTTP Service - JavaScript wrapper for API calls
// ===============================================================================================================

class HttpService {
  static httpFailureRetryCount = 1

  constructor() {
    this.baseHeaders = {
      'Content-Type': 'application/json',
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Headers': 'Content-Type',
      'Access-Control-Allow-Methods': 'OPTIONS,POST,GET,PUT,DELETE,PATCH',
    }
  }

  // Get headers with Authorization token if available
  getHeaders() {
    const token = localStorage.getItem('token')
    const headers = { ...this.baseHeaders }

    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }

    return headers
  }

  async get(url, params = null) {
    return this.requestByUrl('GET', url, undefined, params)
  }

  async getText(url) {
    try {
      const response = await fetch(url, {
        method: 'GET',
        mode: 'cors',
        headers: this.getHeaders(),
      })

      if (response.ok) {
        return await response.text()
      }
      throw new Error(`HTTP Error: ${response.status}`)
    } catch (error) {
      console.error('Error in getText:', error)
      throw error
    }
  }

  async postText(url, body) {
    const preparedBody = typeof body === 'string' ? body : JSON.stringify(body)

    try {
      const response = await fetch(url, {
        method: 'POST',
        mode: 'cors',
        headers: this.getHeaders(),
        body: preparedBody,
      })

      if (response.ok) {
        return await response.text()
      }
      throw new Error(`HTTP Error: ${response.status}`)
    } catch (error) {
      console.error('Error in postText:', error)
      throw error
    }
  }

  async post(url, body, params = null) {
    return this.requestByUrl('POST', url, body, params)
  }

  async patch(url, body, params = null) {
    return this.requestByUrl('PATCH', url, body, params)
  }

  async put(url, body, params = null) {
    return this.requestByUrl('PUT', url, body, params)
  }

  async delete(url, body = null, params = null) {
    return this.requestByUrl('DELETE', url, body, params)
  }

  async head(url, params = null) {
    return this.requestByUrl('HEAD', url, undefined, params)
  }

  async requestByUrl(method, url, body = undefined, params = null) {
    let finalUrl = url

    // Add query parameters if provided
    if (params && Object.keys(params).length > 0) {
      const urlParams = new URLSearchParams(params)
      finalUrl = `${url}?${urlParams.toString()}`
    }

    const requestOptions = {
      method: method,
      mode: 'cors',
      headers: this.getHeaders(),
    }

    if (body !== undefined && method !== 'GET' && method !== 'HEAD') {
      requestOptions.body = typeof body === 'string' ? body : JSON.stringify(body)
    }

    try {
      const response = await fetch(finalUrl, requestOptions)

      // Check for authentication failure
      if (response.status === 401 || response.status === 403) {
        return 'fail'
      }

      if (response.ok) {
        const contentType = response.headers.get('content-type')
        if (contentType && contentType.includes('application/json')) {
          return await response.json()
        } else {
          return await response.text()
        }
      }
      throw new Error(`HTTP Error: ${response.status}`)
    } catch (error) {
      console.error(`Error in ${method} request:`, error)
      throw error
    }
  }

  async getResource(url, ...params) {
    const resourceUrl = `${url}/${params.join('/')}`
    return this.get(resourceUrl)
  }
}

export default HttpService
