// ===============================================================================================================
// GetCuposPorAutomotores Service - JavaScript implementation for vehicle credit limit reports
// Updated to match OpenAPI specification
// ===============================================================================================================

import HttpService from './HttpService.js'

/**
 * Service to fetch vehicle credit limits (cupos) for the current station
 * Returns an integer representing the number of vehicle credit records
 *
 * Expected API Response format according to OpenAPI spec:
 * Returns: integer (int32)
 */
const GetCuposPorAutomotores = async (estacionGuid = null) => {
  const httpService = new HttpService()

  try {
    // Check for authentication token first
    const token = localStorage.getItem('token')
    if (!token) {
      console.error('No authentication token found')
      return 'fail'
    }

    // Get the station GUID from parameter or localStorage
    const estacion = estacionGuid || localStorage.getItem('estacionGuid')

    if (!estacion) {
      console.error('No station GUID provided or found in localStorage')
      return 'fail'
    }

    // Construct the API endpoint URL according to OpenAPI spec
    const url = `${window.SERVER_URL}/CuposInfo/Automotores/${estacion}`

    console.log(`Fetching cupos por automotores from: ${url}`)

    // Make the API call
    const response = await httpService.get(url)

    // Handle authentication failures
    if (response === 'fail' || response === null || response === undefined) {
      console.warn('API returned fail, null, or undefined response')
      return 'fail'
    }

    // According to OpenAPI spec, this should return an integer
    return response

    // Map the response to ensure consistent field names and data types
    const mappedResponse = response.map((cupo, index) => {
      const mappedCupo = {
        // Primary fields used by the component
        cliente: cupo.cliente || cupo.nombreCliente || cupo.Cliente || `Cliente ${index + 1}`,
        nit: cupo.nit || cupo.identificacion || cupo.Nit || cupo.CC || '',
        placa: cupo.placa || cupo.Placa || cupo.numeroPlaca || cupo.licencePlate || '',
        cupoAsignado: parseFloat(cupo.cupoAsignado || cupo.cupoTotal || cupo.CupoAsignado || 0),
        cupoDisponible: parseFloat(
          cupo.cupoDisponible || cupo.cupoRestante || cupo.CupoDisponible || 0,
        ),

        // Additional fields that might be useful for future enhancements
        cupoUtilizado: parseFloat(cupo.cupoUtilizado || cupo.cupoUsado || cupo.CupoUtilizado || 0),
        telefono: cupo.telefono || cupo.Telefono || '',
        direccion: cupo.direccion || cupo.Direccion || '',
        email: cupo.email || cupo.correo || cupo.Email || '',
        activo: cupo.activo !== undefined ? cupo.activo : true,

        // Vehicle-specific fields
        marca: cupo.marca || cupo.Marca || '',
        modelo: cupo.modelo || cupo.Modelo || '',
        año: cupo.año || cupo.anio || cupo.year || cupo.Año || '',
        tipoVehiculo: cupo.tipoVehiculo || cupo.tipo || cupo.TipoVehiculo || '',

        // Calculated field: debt amount
        debe: function () {
          return this.cupoAsignado - this.cupoDisponible
        },
      }

      return mappedCupo
    })

    console.log(`Successfully fetched ${mappedResponse.length} cupos por automotores`)
    return mappedResponse
  } catch (error) {
    console.error('Error fetching cupos por automotores:', error)

    // Check if it's an authentication error
    if (
      error.message &&
      (error.message.includes('401') || error.message.includes('Unauthorized'))
    ) {
      console.warn('Authentication error detected, redirecting to login')
      return 'fail'
    }

    // Check if it's a network error
    if (error.message && error.message.includes('fetch')) {
      console.error('Network error while fetching cupos por automotores')
    }

    // For development/testing: return mock data if API is not available
    if (process.env.NODE_ENV === 'development' && window.SERVER_URL?.includes('localhost')) {
      console.warn('Development mode: API error detected, returning mock data')
      return getMockCuposAutomotoresData()
    }

    // For production errors, return 'fail' to trigger navigation to login
    return 'fail'
  }
}

/**
 * Mock data for development/testing purposes
 * This helps developers work on the UI even when the backend API is not available
 */
const getMockCuposAutomotoresData = () => {
  return [
    {
      cliente: 'TRANSPORTES DEL HUILA S.A.S',
      nit: '900123456-1',
      placa: 'HUL123',
      cupoAsignado: 2500000,
      cupoDisponible: 1800000,
      cupoUtilizado: 700000,
      telefono: '318-555-0001',
      direccion: 'Carrera 5 # 12-34, Neiva',
      email: 'transportes@huila.com',
      activo: true,
      marca: 'CHEVROLET',
      modelo: 'NPR',
      año: '2020',
      tipoVehiculo: 'CAMION',
    },
    {
      cliente: 'LOGISTICA MAGDALENA LTDA',
      nit: '800654321-2',
      placa: 'MAG456',
      cupoAsignado: 1800000,
      cupoDisponible: 450000,
      cupoUtilizado: 1350000,
      telefono: '315-555-0002',
      direccion: 'Calle 8 # 25-67, Neiva',
      email: 'logistica@magdalena.com',
      activo: true,
      marca: 'FORD',
      modelo: 'CARGO',
      año: '2019',
      tipoVehiculo: 'CAMION',
    },
    {
      cliente: 'COOPERATIVA DE TRANSPORTADORES',
      nit: '700987654-3',
      placa: 'COO789',
      cupoAsignado: 3200000,
      cupoDisponible: 2400000,
      cupoUtilizado: 800000,
      telefono: '312-555-0003',
      direccion: 'Avenida 26 # 15-89, Neiva',
      email: 'cooperativa@transportadores.com',
      activo: true,
      marca: 'KENWORTH',
      modelo: 'T800',
      año: '2021',
      tipoVehiculo: 'TRACTOCAMION',
    },
    {
      cliente: 'TRANSPORTES DEL HUILA S.A.S',
      nit: '900123456-1',
      placa: 'HUL124',
      cupoAsignado: 2200000,
      cupoDisponible: 1100000,
      cupoUtilizado: 1100000,
      telefono: '318-555-0001',
      direccion: 'Carrera 5 # 12-34, Neiva',
      email: 'transportes@huila.com',
      activo: true,
      marca: 'FREIGHTLINER',
      modelo: 'CASCADIA',
      año: '2018',
      tipoVehiculo: 'TRACTOCAMION',
    },
    {
      cliente: 'SERVICIO ESPECIAL DE CARGA',
      nit: '600111222-4',
      placa: 'SEC001',
      cupoAsignado: 1500000,
      cupoDisponible: 1200000,
      cupoUtilizado: 300000,
      telefono: '311-555-0004',
      direccion: 'Calle 15 # 10-45, Neiva',
      email: 'especial@carga.com',
      activo: true,
      marca: 'VOLVO',
      modelo: 'FH',
      año: '2022',
      tipoVehiculo: 'TRACTOCAMION',
    },
  ]
}

export default GetCuposPorAutomotores
