import React, { useState, useEffect } from 'react'
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CForm,
  CFormInput,
  CFormLabel,
  CFormSelect,
  CFormTextarea,
  CButton,
  CAlert,
  CSpinner,
  CInputGroup,
  CInputGroupText,
  CBadge,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilUser,
  cilSave,
  cilArrowLeft,
  cilCheck,
  cilX,
  cilPhone,
  cilContact,
  cilLocationPin,
} from '@coreui/icons'
import { useNavigate, useParams } from 'react-router-dom'
import TercerosService from '../../services/TercerosService'

const TerceroForm = () => {
  const navigate = useNavigate()
  const { guid } = useParams()
  const isEditing = !!guid
  const [loading, setLoading] = useState(false)
  const [loadingData, setLoadingData] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [validationErrors, setValidationErrors] = useState({})
  const [isValidIdentificacion, setIsValidIdentificacion] = useState(null)

  const [tercero, setTercero] = useState({
    guid: '',
    nombre: '',
    segundo: '',
    apellidos: '',
    identificacion: '',
    tipoIdentificacion: 1,
    tipoPersona: 1,
    municipio: '',
    departamento: '',
    direccion: '',
    pais: 'Colombia',
    codigoPostal: '',
    telefono: '',
    telefono2: '',
    celular: '',
    correo: '',
    correo2: '',
    vendedor: '',
    comentarios: '',
    responsabilidadTributaria: 1,
  })

  const tercerosService = new TercerosService()

  // Opciones para los selects
  const tiposPersona = [
    { value: 1, label: 'Persona Natural' },
    { value: 2, label: 'Persona Jurídica' },
  ]

  const tiposIdentificacion = [
    { value: 1, label: 'Cédula de Ciudadanía' },
    { value: 2, label: 'Cédula de Extranjería' },
    { value: 3, label: 'NIT' },
    { value: 4, label: 'Pasaporte' },
    { value: 5, label: 'Tarjeta de Identidad' },
  ]

  const responsabilidadesTributarias = [
    { value: 1, label: 'Responsable de IVA' },
    { value: 2, label: 'No responsable de IVA' },
    { value: 3, label: 'Régimen Simplificado' },
    { value: 4, label: 'Gran Contribuyente' },
  ]

  useEffect(() => {
    if (isEditing) {
      loadTercero()
    }
  }, [guid])

  const loadTercero = async () => {
    setLoadingData(true)
    setError('')
    try {
      const data = await tercerosService.getTercero(guid)
      setTercero(data || {})
    } catch (error) {
      console.error('Error loading tercero:', error)
      setError('Error al cargar los datos del tercero.')
    } finally {
      setLoadingData(false)
    }
  }

  const handleInputChange = (field, value) => {
    setTercero((prev) => ({
      ...prev,
      [field]: value,
    }))

    // Limpiar error de validación del campo
    if (validationErrors[field]) {
      setValidationErrors((prev) => {
        const newErrors = { ...prev }
        delete newErrors[field]
        return newErrors
      })
    }

    // Validar identificación en tiempo real
    if (field === 'identificacion' && value) {
      validateIdentificacion(value)
    }
  }

  const validateIdentificacion = async (identificacion) => {
    if (!identificacion) {
      setIsValidIdentificacion(null)
      return
    }

    try {
      const isValid = await tercerosService.isTerceroValidoPorIdentificacion(identificacion)
      setIsValidIdentificacion(isValid)
    } catch (error) {
      console.error('Error validating identificacion:', error)
      setIsValidIdentificacion(null)
    }
  }

  const validateForm = () => {
    const validation = tercerosService.validarTercero(tercero)
    if (!validation.isValid) {
      const errors = {}
      validation.errores.forEach((error) => {
        if (error.includes('identificación')) errors.identificacion = error
        if (error.includes('nombre')) errors.nombre = error
        if (error.includes('tipo de persona')) errors.tipoPersona = error
        if (error.includes('tipo de identificación')) errors.tipoIdentificacion = error
        if (error.includes('correo')) errors.correo = error
      })
      setValidationErrors(errors)
      return false
    }
    return true
  }

  const handleSubmit = async (e) => {
    e.preventDefault()

    if (!validateForm()) {
      setError('Por favor, corrija los errores en el formulario.')
      return
    }

    setLoading(true)
    setError('')
    setSuccess('')

    try {
      if (isEditing) {
        await tercerosService.actualizarTercero(tercero)
        setSuccess('Tercero actualizado exitosamente')
      } else {
        await tercerosService.crearTercero(tercero)
        setSuccess('Tercero creado exitosamente')
      }

      setTimeout(() => {
        navigate('/terceros')
      }, 2000)
    } catch (error) {
      console.error('Error saving tercero:', error)
      setError(
        `Error al ${isEditing ? 'actualizar' : 'crear'} el tercero. Por favor, intente nuevamente.`,
      )
    } finally {
      setLoading(false)
    }
  }

  const handleCancel = () => {
    navigate('/terceros')
  }

  if (loadingData) {
    return (
      <div className="text-center">
        <CSpinner color="primary" />
        <p className="mt-2">Cargando datos del tercero...</p>
      </div>
    )
  }

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="mb-4">
          <CCardHeader>
            <div className="d-flex justify-content-between align-items-center">
              <div className="d-flex align-items-center">
                <CIcon icon={cilUser} className="me-2" />
                <strong>{isEditing ? 'Editar Tercero' : 'Nuevo Tercero'}</strong>
              </div>
              <CButton color="secondary" variant="outline" onClick={handleCancel}>
                <CIcon icon={cilArrowLeft} className="me-1" />
                Volver
              </CButton>
            </div>
          </CCardHeader>
          <CCardBody>
            {error && (
              <CAlert color="danger" dismissible onClose={() => setError('')}>
                {error}
              </CAlert>
            )}
            {success && (
              <CAlert color="success" dismissible onClose={() => setSuccess('')}>
                {success}
              </CAlert>
            )}

            <CForm onSubmit={handleSubmit}>
              <CRow>
                {/* Información básica */}
                <CCol md={12}>
                  <CCard className="mb-3">
                    <CCardHeader>
                      <strong>Información Básica</strong>
                    </CCardHeader>
                    <CCardBody>
                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel>Tipo de Persona *</CFormLabel>
                            <CFormSelect
                              value={tercero.tipoPersona}
                              onChange={(e) =>
                                handleInputChange('tipoPersona', parseInt(e.target.value))
                              }
                              invalid={!!validationErrors.tipoPersona}
                            >
                              {tiposPersona.map((tipo) => (
                                <option key={tipo.value} value={tipo.value}>
                                  {tipo.label}
                                </option>
                              ))}
                            </CFormSelect>
                            {validationErrors.tipoPersona && (
                              <div className="invalid-feedback d-block">
                                {validationErrors.tipoPersona}
                              </div>
                            )}
                          </div>
                        </CCol>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel>Tipo de Identificación *</CFormLabel>
                            <CFormSelect
                              value={tercero.tipoIdentificacion}
                              onChange={(e) =>
                                handleInputChange('tipoIdentificacion', parseInt(e.target.value))
                              }
                              invalid={!!validationErrors.tipoIdentificacion}
                            >
                              {tiposIdentificacion.map((tipo) => (
                                <option key={tipo.value} value={tipo.value}>
                                  {tipo.label}
                                </option>
                              ))}
                            </CFormSelect>
                            {validationErrors.tipoIdentificacion && (
                              <div className="invalid-feedback d-block">
                                {validationErrors.tipoIdentificacion}
                              </div>
                            )}
                          </div>
                        </CCol>
                      </CRow>

                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel>Número de Identificación *</CFormLabel>
                            <CInputGroup>
                              <CFormInput
                                type="text"
                                value={tercero.identificacion}
                                onChange={(e) =>
                                  handleInputChange('identificacion', e.target.value)
                                }
                                placeholder="Ingrese la identificación"
                                invalid={!!validationErrors.identificacion}
                                required
                              />
                              {isValidIdentificacion !== null && (
                                <CInputGroupText>
                                  {isValidIdentificacion ? (
                                    <CIcon icon={cilCheck} className="text-success" />
                                  ) : (
                                    <CIcon icon={cilX} className="text-danger" />
                                  )}
                                </CInputGroupText>
                              )}
                            </CInputGroup>
                            {validationErrors.identificacion && (
                              <div className="invalid-feedback d-block">
                                {validationErrors.identificacion}
                              </div>
                            )}
                          </div>
                        </CCol>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel>Responsabilidad Tributaria</CFormLabel>
                            <CFormSelect
                              value={tercero.responsabilidadTributaria}
                              onChange={(e) =>
                                handleInputChange(
                                  'responsabilidadTributaria',
                                  parseInt(e.target.value),
                                )
                              }
                            >
                              {responsabilidadesTributarias.map((resp) => (
                                <option key={resp.value} value={resp.value}>
                                  {resp.label}
                                </option>
                              ))}
                            </CFormSelect>
                          </div>
                        </CCol>
                      </CRow>

                      <CRow>
                        <CCol md={4}>
                          <div className="mb-3">
                            <CFormLabel>Primer Nombre *</CFormLabel>
                            <CFormInput
                              type="text"
                              value={tercero.nombre}
                              onChange={(e) => handleInputChange('nombre', e.target.value)}
                              placeholder="Primer nombre"
                              invalid={!!validationErrors.nombre}
                              required
                            />
                            {validationErrors.nombre && (
                              <div className="invalid-feedback d-block">
                                {validationErrors.nombre}
                              </div>
                            )}
                          </div>
                        </CCol>
                        <CCol md={4}>
                          <div className="mb-3">
                            <CFormLabel>Segundo Nombre</CFormLabel>
                            <CFormInput
                              type="text"
                              value={tercero.segundo}
                              onChange={(e) => handleInputChange('segundo', e.target.value)}
                              placeholder="Segundo nombre"
                            />
                          </div>
                        </CCol>
                        <CCol md={4}>
                          <div className="mb-3">
                            <CFormLabel>Apellidos</CFormLabel>
                            <CFormInput
                              type="text"
                              value={tercero.apellidos}
                              onChange={(e) => handleInputChange('apellidos', e.target.value)}
                              placeholder="Apellidos"
                            />
                          </div>
                        </CCol>
                      </CRow>
                    </CCardBody>
                  </CCard>
                </CCol>

                {/* Información de contacto */}
                <CCol md={12}>
                  <CCard className="mb-3">
                    <CCardHeader>
                      <CIcon icon={cilContact} className="me-2" />
                      <strong>Información de Contacto</strong>
                    </CCardHeader>
                    <CCardBody>
                      <CRow>
                        <CCol md={4}>
                          <div className="mb-3">
                            <CFormLabel>Teléfono</CFormLabel>
                            <CInputGroup>
                              <CInputGroupText>
                                <CIcon icon={cilPhone} />
                              </CInputGroupText>
                              <CFormInput
                                type="tel"
                                value={tercero.telefono}
                                onChange={(e) => handleInputChange('telefono', e.target.value)}
                                placeholder="Número de teléfono"
                              />
                            </CInputGroup>
                          </div>
                        </CCol>
                        <CCol md={4}>
                          <div className="mb-3">
                            <CFormLabel>Teléfono 2</CFormLabel>
                            <CInputGroup>
                              <CInputGroupText>
                                <CIcon icon={cilPhone} />
                              </CInputGroupText>
                              <CFormInput
                                type="tel"
                                value={tercero.telefono2}
                                onChange={(e) => handleInputChange('telefono2', e.target.value)}
                                placeholder="Segundo teléfono"
                              />
                            </CInputGroup>
                          </div>
                        </CCol>
                        <CCol md={4}>
                          <div className="mb-3">
                            <CFormLabel>Celular</CFormLabel>
                            <CInputGroup>
                              <CInputGroupText>
                                <CIcon icon={cilPhone} />
                              </CInputGroupText>
                              <CFormInput
                                type="tel"
                                value={tercero.celular}
                                onChange={(e) => handleInputChange('celular', e.target.value)}
                                placeholder="Número de celular"
                              />
                            </CInputGroup>
                          </div>
                        </CCol>
                      </CRow>

                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel>Correo Electrónico</CFormLabel>
                            <CFormInput
                              type="email"
                              value={tercero.correo}
                              onChange={(e) => handleInputChange('correo', e.target.value)}
                              placeholder="correo@ejemplo.com"
                              invalid={!!validationErrors.correo}
                            />
                            {validationErrors.correo && (
                              <div className="invalid-feedback d-block">
                                {validationErrors.correo}
                              </div>
                            )}
                          </div>
                        </CCol>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel>Correo Electrónico 2</CFormLabel>
                            <CFormInput
                              type="email"
                              value={tercero.correo2}
                              onChange={(e) => handleInputChange('correo2', e.target.value)}
                              placeholder="segundo@ejemplo.com"
                            />
                          </div>
                        </CCol>
                      </CRow>
                    </CCardBody>
                  </CCard>
                </CCol>

                {/* Información de ubicación */}
                <CCol md={12}>
                  <CCard className="mb-3">
                    <CCardHeader>
                      <CIcon icon={cilLocationPin} className="me-2" />
                      <strong>Información de Ubicación</strong>
                    </CCardHeader>
                    <CCardBody>
                      <CRow>
                        <CCol md={12}>
                          <div className="mb-3">
                            <CFormLabel>Dirección</CFormLabel>
                            <CFormInput
                              type="text"
                              value={tercero.direccion}
                              onChange={(e) => handleInputChange('direccion', e.target.value)}
                              placeholder="Dirección completa"
                            />
                          </div>
                        </CCol>
                      </CRow>

                      <CRow>
                        <CCol md={3}>
                          <div className="mb-3">
                            <CFormLabel>País</CFormLabel>
                            <CFormInput
                              type="text"
                              value={tercero.pais}
                              onChange={(e) => handleInputChange('pais', e.target.value)}
                              placeholder="País"
                            />
                          </div>
                        </CCol>
                        <CCol md={3}>
                          <div className="mb-3">
                            <CFormLabel>Departamento</CFormLabel>
                            <CFormInput
                              type="text"
                              value={tercero.departamento}
                              onChange={(e) => handleInputChange('departamento', e.target.value)}
                              placeholder="Departamento"
                            />
                          </div>
                        </CCol>
                        <CCol md={3}>
                          <div className="mb-3">
                            <CFormLabel>Municipio</CFormLabel>
                            <CFormInput
                              type="text"
                              value={tercero.municipio}
                              onChange={(e) => handleInputChange('municipio', e.target.value)}
                              placeholder="Municipio"
                            />
                          </div>
                        </CCol>
                        <CCol md={3}>
                          <div className="mb-3">
                            <CFormLabel>Código Postal</CFormLabel>
                            <CFormInput
                              type="text"
                              value={tercero.codigoPostal}
                              onChange={(e) => handleInputChange('codigoPostal', e.target.value)}
                              placeholder="Código postal"
                            />
                          </div>
                        </CCol>
                      </CRow>
                    </CCardBody>
                  </CCard>
                </CCol>

                {/* Información adicional */}
                <CCol md={12}>
                  <CCard className="mb-3">
                    <CCardHeader>
                      <strong>Información Adicional</strong>
                    </CCardHeader>
                    <CCardBody>
                      <CRow>
                        <CCol md={6}>
                          <div className="mb-3">
                            <CFormLabel>Vendedor</CFormLabel>
                            <CFormInput
                              type="text"
                              value={tercero.vendedor}
                              onChange={(e) => handleInputChange('vendedor', e.target.value)}
                              placeholder="Vendedor asignado"
                            />
                          </div>
                        </CCol>
                      </CRow>

                      <CRow>
                        <CCol md={12}>
                          <div className="mb-3">
                            <CFormLabel>Comentarios</CFormLabel>
                            <CFormTextarea
                              rows={3}
                              value={tercero.comentarios}
                              onChange={(e) => handleInputChange('comentarios', e.target.value)}
                              placeholder="Comentarios adicionales"
                            />
                          </div>
                        </CCol>
                      </CRow>
                    </CCardBody>
                  </CCard>
                </CCol>
              </CRow>

              {/* Botones de acción */}
              <div className="d-flex justify-content-end gap-2">
                <CButton type="button" color="secondary" onClick={handleCancel} disabled={loading}>
                  Cancelar
                </CButton>
                <CButton type="submit" color="primary" disabled={loading}>
                  {loading ? (
                    <>
                      <CSpinner size="sm" className="me-2" />
                      {isEditing ? 'Actualizando...' : 'Creando...'}
                    </>
                  ) : (
                    <>
                      <CIcon icon={cilSave} className="me-1" />
                      {isEditing ? 'Actualizar' : 'Crear'} Tercero
                    </>
                  )}
                </CButton>
              </div>
            </CForm>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  )
}

export default TerceroForm
