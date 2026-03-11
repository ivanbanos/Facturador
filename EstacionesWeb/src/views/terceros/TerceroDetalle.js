import React, { useState, useEffect } from 'react'
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CButton,
  CAlert,
  CSpinner,
  CBadge,
  CListGroup,
  CListGroupItem,
  CAvatar,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilUser,
  cilArrowLeft,
  cilPencil,
  cilPhone,
  cilContact,
  cilLocationPin,
  cilInfo,
  cilPeople,
  cilCreditCard,
  cilEnvelopeClosed,
} from '@coreui/icons'
import { useNavigate, useParams } from 'react-router-dom'
import TercerosService from '../../services/TercerosService'

const TerceroDetalle = () => {
  const navigate = useNavigate()
  const { guid } = useParams()
  const [tercero, setTercero] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const tercerosService = new TercerosService()

  // Opciones para mostrar labels
  const tiposPersona = {
    1: 'Persona Natural',
    2: 'Persona Jurídica',
  }

  const tiposIdentificacion = {
    1: 'Cédula de Ciudadanía',
    2: 'Cédula de Extranjería',
    3: 'NIT',
    4: 'Pasaporte',
    5: 'Tarjeta de Identidad',
  }

  const responsabilidadesTributarias = {
    1: 'Responsable de IVA',
    2: 'No responsable de IVA',
    3: 'Régimen Simplificado',
    4: 'Gran Contribuyente',
  }

  useEffect(() => {
    loadTercero()
  }, [guid])

  const loadTercero = async () => {
    setLoading(true)
    setError('')
    try {
      const data = await tercerosService.getTercero(guid)
      setTercero(data)
    } catch (error) {
      console.error('Error loading tercero:', error)
      setError('Error al cargar los datos del tercero.')
    } finally {
      setLoading(false)
    }
  }

  const handleEdit = () => {
    navigate(`/terceros/editar/${guid}`)
  }

  const handleBack = () => {
    navigate('/terceros')
  }

  const getNombreCompleto = () => {
    if (!tercero) return ''
    return `${tercero.nombre || ''} ${tercero.segundo || ''} ${tercero.apellidos || ''}`.trim()
  }

  const getInitials = () => {
    const nombre = getNombreCompleto()
    return nombre
      .split(' ')
      .map((word) => word.charAt(0))
      .join('')
      .substring(0, 2)
      .toUpperCase()
  }

  if (loading) {
    return (
      <div className="text-center">
        <CSpinner color="primary" />
        <p className="mt-2">Cargando datos del tercero...</p>
      </div>
    )
  }

  if (error) {
    return (
      <CAlert color="danger">
        {error}
        <div className="mt-2">
          <CButton color="secondary" onClick={handleBack}>
            Volver
          </CButton>
        </div>
      </CAlert>
    )
  }

  if (!tercero) {
    return (
      <CAlert color="warning">
        No se encontraron datos del tercero.
        <div className="mt-2">
          <CButton color="secondary" onClick={handleBack}>
            Volver
          </CButton>
        </div>
      </CAlert>
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
                <strong>Detalles del Tercero</strong>
              </div>
              <div className="d-flex gap-2">
                <CButton color="warning" onClick={handleEdit}>
                  <CIcon icon={cilPencil} className="me-1" />
                  Editar
                </CButton>
                <CButton color="secondary" variant="outline" onClick={handleBack}>
                  <CIcon icon={cilArrowLeft} className="me-1" />
                  Volver
                </CButton>
              </div>
            </div>
          </CCardHeader>
          <CCardBody>
            <CRow>
              {/* Información principal */}
              <CCol md={4}>
                <CCard className="mb-4">
                  <CCardBody className="text-center">
                    <CAvatar size="xl" color="primary" className="mb-3">
                      {getInitials()}
                    </CAvatar>
                    <h4>{getNombreCompleto()}</h4>
                    <CBadge color={tercero.tipoPersona === 1 ? 'info' : 'warning'} className="mb-2">
                      {tiposPersona[tercero.tipoPersona] || tercero.tipoPersona}
                    </CBadge>
                    <p className="text-muted mb-0">{tercero.identificacion}</p>
                    <small className="text-muted">
                      {tiposIdentificacion[tercero.tipoIdentificacion] ||
                        tercero.tipoIdentificacion}
                    </small>
                  </CCardBody>
                </CCard>
              </CCol>

              {/* Información detallada */}
              <CCol md={8}>
                <CRow>
                  {/* Información básica */}
                  <CCol md={6}>
                    <CCard className="mb-4">
                      <CCardHeader>
                        <CIcon icon={cilCreditCard} className="me-2" />
                        <strong>Información Básica</strong>
                      </CCardHeader>
                      <CCardBody>
                        <CListGroup flush>
                          <CListGroupItem className="d-flex justify-content-between align-items-start">
                            <div>
                              <strong>Identificación</strong>
                              <div className="text-muted small">
                                {tiposIdentificacion[tercero.tipoIdentificacion] ||
                                  tercero.tipoIdentificacion}
                              </div>
                            </div>
                            <span>{tercero.identificacion}</span>
                          </CListGroupItem>
                          <CListGroupItem className="d-flex justify-content-between">
                            <strong>Tipo de Persona</strong>
                            <CBadge color={tercero.tipoPersona === 1 ? 'info' : 'warning'}>
                              {tiposPersona[tercero.tipoPersona] || tercero.tipoPersona}
                            </CBadge>
                          </CListGroupItem>
                          <CListGroupItem className="d-flex justify-content-between">
                            <strong>Responsabilidad Tributaria</strong>
                            <span>
                              {responsabilidadesTributarias[tercero.responsabilidadTributaria] ||
                                tercero.responsabilidadTributaria}
                            </span>
                          </CListGroupItem>
                          {tercero.vendedor && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>Vendedor</strong>
                              <span>{tercero.vendedor}</span>
                            </CListGroupItem>
                          )}
                        </CListGroup>
                      </CCardBody>
                    </CCard>
                  </CCol>

                  {/* Información de contacto */}
                  <CCol md={6}>
                    <CCard className="mb-4">
                      <CCardHeader>
                        <CIcon icon={cilContact} className="me-2" />
                        <strong>Información de Contacto</strong>
                      </CCardHeader>
                      <CCardBody>
                        <CListGroup flush>
                          {tercero.correo && (
                            <CListGroupItem className="d-flex justify-content-between align-items-center">
                              <div className="d-flex align-items-center">
                                <CIcon icon={cilEnvelopeClosed} className="me-2 text-primary" />
                                <strong>Correo</strong>
                              </div>
                              <a href={`mailto:${tercero.correo}`} className="text-decoration-none">
                                {tercero.correo}
                              </a>
                            </CListGroupItem>
                          )}
                          {tercero.correo2 && (
                            <CListGroupItem className="d-flex justify-content-between align-items-center">
                              <div className="d-flex align-items-center">
                                <CIcon icon={cilEnvelopeClosed} className="me-2 text-primary" />
                                <strong>Correo 2</strong>
                              </div>
                              <a
                                href={`mailto:${tercero.correo2}`}
                                className="text-decoration-none"
                              >
                                {tercero.correo2}
                              </a>
                            </CListGroupItem>
                          )}
                          {tercero.telefono && (
                            <CListGroupItem className="d-flex justify-content-between align-items-center">
                              <div className="d-flex align-items-center">
                                <CIcon icon={cilPhone} className="me-2 text-success" />
                                <strong>Teléfono</strong>
                              </div>
                              <a href={`tel:${tercero.telefono}`} className="text-decoration-none">
                                {tercero.telefono}
                              </a>
                            </CListGroupItem>
                          )}
                          {tercero.telefono2 && (
                            <CListGroupItem className="d-flex justify-content-between align-items-center">
                              <div className="d-flex align-items-center">
                                <CIcon icon={cilPhone} className="me-2 text-success" />
                                <strong>Teléfono 2</strong>
                              </div>
                              <a href={`tel:${tercero.telefono2}`} className="text-decoration-none">
                                {tercero.telefono2}
                              </a>
                            </CListGroupItem>
                          )}
                          {tercero.celular && (
                            <CListGroupItem className="d-flex justify-content-between align-items-center">
                              <div className="d-flex align-items-center">
                                <CIcon icon={cilPhone} className="me-2 text-info" />
                                <strong>Celular</strong>
                              </div>
                              <a href={`tel:${tercero.celular}`} className="text-decoration-none">
                                {tercero.celular}
                              </a>
                            </CListGroupItem>
                          )}
                        </CListGroup>
                      </CCardBody>
                    </CCard>
                  </CCol>
                </CRow>

                {/* Información de ubicación */}
                <CRow>
                  <CCol md={12}>
                    <CCard className="mb-4">
                      <CCardHeader>
                        <CIcon icon={cilLocationPin} className="me-2" />
                        <strong>Información de Ubicación</strong>
                      </CCardHeader>
                      <CCardBody>
                        <CListGroup flush>
                          {tercero.direccion && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>Dirección</strong>
                              <span>{tercero.direccion}</span>
                            </CListGroupItem>
                          )}
                          {tercero.municipio && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>Municipio</strong>
                              <span>{tercero.municipio}</span>
                            </CListGroupItem>
                          )}
                          {tercero.departamento && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>Departamento</strong>
                              <span>{tercero.departamento}</span>
                            </CListGroupItem>
                          )}
                          {tercero.pais && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>País</strong>
                              <span>{tercero.pais}</span>
                            </CListGroupItem>
                          )}
                          {tercero.codigoPostal && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>Código Postal</strong>
                              <span>{tercero.codigoPostal}</span>
                            </CListGroupItem>
                          )}
                        </CListGroup>
                      </CCardBody>
                    </CCard>
                  </CCol>
                </CRow>

                {/* Información adicional */}
                {tercero.comentarios && (
                  <CRow>
                    <CCol md={12}>
                      <CCard className="mb-4">
                        <CCardHeader>
                          <CIcon icon={cilInfo} className="me-2" />
                          <strong>Comentarios</strong>
                        </CCardHeader>
                        <CCardBody>
                          <p className="mb-0">{tercero.comentarios}</p>
                        </CCardBody>
                      </CCard>
                    </CCol>
                  </CRow>
                )}

                {/* Información técnica */}
                <CRow>
                  <CCol md={12}>
                    <CCard>
                      <CCardHeader>
                        <CIcon icon={cilInfo} className="me-2" />
                        <strong>Información Técnica</strong>
                      </CCardHeader>
                      <CCardBody>
                        <CListGroup flush>
                          <CListGroupItem className="d-flex justify-content-between">
                            <strong>GUID</strong>
                            <code>{tercero.guid}</code>
                          </CListGroupItem>
                          {tercero.id && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>ID</strong>
                              <span>{tercero.id}</span>
                            </CListGroupItem>
                          )}
                          {tercero.idLocal && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>ID Local</strong>
                              <span>{tercero.idLocal}</span>
                            </CListGroupItem>
                          )}
                          {tercero.idContable && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>ID Contable</strong>
                              <span>{tercero.idContable}</span>
                            </CListGroupItem>
                          )}
                          {tercero.idFacturacion && (
                            <CListGroupItem className="d-flex justify-content-between">
                              <strong>ID Facturación</strong>
                              <span>{tercero.idFacturacion}</span>
                            </CListGroupItem>
                          )}
                        </CListGroup>
                      </CCardBody>
                    </CCard>
                  </CCol>
                </CRow>
              </CCol>
            </CRow>
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  )
}

export default TerceroDetalle
