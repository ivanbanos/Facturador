import React, { useState, useEffect } from 'react'
import {
  CCard,
  CCardBody,
  CCardHeader,
  CCol,
  CRow,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CButton,
  CInputGroup,
  CFormInput,
  CSpinner,
  CAlert,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
  CBadge,
  CPagination,
  CPaginationItem,
  CButtonGroup,
  CFormSelect,
  CTooltip,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilPeople,
  cilPlus,
  cilPencil,
  cilTrash,
  cilSearch,
  cilReload,
  cilCloudDownload,
  cilSync,
  cilUserFollow,
  cilFilter,
  cilX,
} from '@coreui/icons'
import TercerosService from '../../services/TercerosService'
import { useNavigate } from 'react-router-dom'

const Terceros = () => {
  const navigate = useNavigate()
  const [terceros, setTerceros] = useState([])
  const [tercerosFiltrados, setTercerosFiltrados] = useState([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [showDeleteModal, setShowDeleteModal] = useState(false)
  const [terceroToDelete, setTerceroToDelete] = useState(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [itemsPerPage] = useState(10)
  const [showFilters, setShowFilters] = useState(false)
  const [filters, setFilters] = useState({
    tipoPersona: '',
    tipoIdentificacion: '',
    municipio: '',
    departamento: '',
  })

  const tercerosService = new TercerosService()

  // Tipos de persona y identificación (según la API)
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

  useEffect(() => {
    loadTerceros()
  }, [])

  useEffect(() => {
    filterTerceros()
  }, [terceros, searchTerm, filters])

  const loadTerceros = async () => {
    setLoading(true)
    setError('')
    try {
      const data = await tercerosService.getTerceros()
      setTerceros(data || [])
    } catch (error) {
      console.error('Error loading terceros:', error)
      setError('Error al cargar los terceros. Por favor, intente nuevamente.')
    } finally {
      setLoading(false)
    }
  }

  const filterTerceros = () => {
    let filtered = [...terceros]

    // Filtro de búsqueda general
    if (searchTerm) {
      filtered = filtered.filter((tercero) => {
        const nombreCompleto = `${tercero.nombre || ''} ${tercero.segundo || ''} ${
          tercero.apellidos || ''
        }`.trim()
        return (
          nombreCompleto.toLowerCase().includes(searchTerm.toLowerCase()) ||
          (tercero.identificacion &&
            tercero.identificacion.toLowerCase().includes(searchTerm.toLowerCase())) ||
          (tercero.correo && tercero.correo.toLowerCase().includes(searchTerm.toLowerCase()))
        )
      })
    }

    // Filtros específicos
    if (filters.tipoPersona) {
      filtered = filtered.filter((tercero) => tercero.tipoPersona === parseInt(filters.tipoPersona))
    }

    if (filters.tipoIdentificacion) {
      filtered = filtered.filter(
        (tercero) => tercero.tipoIdentificacion === parseInt(filters.tipoIdentificacion),
      )
    }

    if (filters.municipio) {
      filtered = filtered.filter(
        (tercero) =>
          tercero.municipio &&
          tercero.municipio.toLowerCase().includes(filters.municipio.toLowerCase()),
      )
    }

    if (filters.departamento) {
      filtered = filtered.filter(
        (tercero) =>
          tercero.departamento &&
          tercero.departamento.toLowerCase().includes(filters.departamento.toLowerCase()),
      )
    }

    setTercerosFiltrados(filtered)
    setCurrentPage(1)
  }

  const handleSearch = (value) => {
    setSearchTerm(value)
  }

  const handleFilterChange = (field, value) => {
    setFilters((prev) => ({
      ...prev,
      [field]: value,
    }))
  }

  const clearFilters = () => {
    setFilters({
      tipoPersona: '',
      tipoIdentificacion: '',
      municipio: '',
      departamento: '',
    })
    setSearchTerm('')
  }

  const handleCreateTercero = () => {
    navigate('/terceros/nuevo')
  }

  const handleEditTercero = (tercero) => {
    navigate(`/terceros/editar/${tercero.guid}`)
  }

  const handleViewTercero = (tercero) => {
    navigate(`/terceros/ver/${tercero.guid}`)
  }

  const confirmDelete = (tercero) => {
    setTerceroToDelete(tercero)
    setShowDeleteModal(true)
  }

  const handleSyncTerceros = async () => {
    setLoading(true)
    setError('')
    setSuccess('')
    try {
      await tercerosService.sincronizarTerceros()
      setSuccess('Terceros sincronizados exitosamente')
      await loadTerceros()
    } catch (error) {
      console.error('Error syncing terceros:', error)
      setError('Error al sincronizar terceros. Por favor, intente nuevamente.')
    } finally {
      setLoading(false)
    }
  }

  const getTipoPersonaLabel = (tipo) => {
    const tipoObj = tiposPersona.find((t) => t.value === tipo)
    return tipoObj ? tipoObj.label : tipo
  }

  const getTipoIdentificacionLabel = (tipo) => {
    const tipoObj = tiposIdentificacion.find((t) => t.value === tipo)
    return tipoObj ? tipoObj.label : tipo
  }

  // Paginación
  const indexOfLastItem = currentPage * itemsPerPage
  const indexOfFirstItem = indexOfLastItem - itemsPerPage
  const currentItems = tercerosFiltrados.slice(indexOfFirstItem, indexOfLastItem)
  const totalPages = Math.ceil(tercerosFiltrados.length / itemsPerPage)

  const paginate = (pageNumber) => setCurrentPage(pageNumber)

  return (
    <CRow>
      <CCol xs={12}>
        <CCard className="mb-4">
          <CCardHeader>
            <div className="d-flex justify-content-between align-items-center">
              <div className="d-flex align-items-center">
                <CIcon icon={cilPeople} className="me-2" />
                <strong>Gestión de Terceros</strong>
                <CBadge color="info" className="ms-2">
                  {tercerosFiltrados.length} terceros
                </CBadge>
              </div>
              <div className="d-flex gap-2">
                <CTooltip content="Sincronizar terceros">
                  <CButton
                    color="info"
                    variant="outline"
                    onClick={handleSyncTerceros}
                    disabled={loading}
                  >
                    <CIcon icon={cilSync} />
                  </CButton>
                </CTooltip>
                <CTooltip content="Recargar">
                  <CButton
                    color="secondary"
                    variant="outline"
                    onClick={loadTerceros}
                    disabled={loading}
                  >
                    <CIcon icon={cilReload} />
                  </CButton>
                </CTooltip>
                <CButton color="primary" onClick={handleCreateTercero}>
                  <CIcon icon={cilPlus} className="me-1" />
                  Nuevo Tercero
                </CButton>
              </div>
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

            {/* Barra de búsqueda y filtros */}
            <CRow className="mb-3">
              <CCol md={6}>
                <CInputGroup>
                  <CFormInput
                    placeholder="Buscar por nombre, identificación o correo..."
                    value={searchTerm}
                    onChange={(e) => handleSearch(e.target.value)}
                  />
                  <CButton
                    type="button"
                    color="secondary"
                    variant="outline"
                    onClick={() => setShowFilters(!showFilters)}
                  >
                    <CIcon icon={cilFilter} />
                  </CButton>
                  {(searchTerm || Object.values(filters).some((f) => f)) && (
                    <CButton
                      type="button"
                      color="secondary"
                      variant="outline"
                      onClick={clearFilters}
                    >
                      <CIcon icon={cilX} />
                    </CButton>
                  )}
                </CInputGroup>
              </CCol>
            </CRow>

            {/* Filtros avanzados */}
            {showFilters && (
              <CCard className="mb-3">
                <CCardBody>
                  <CRow>
                    <CCol md={3}>
                      <CFormSelect
                        value={filters.tipoPersona}
                        onChange={(e) => handleFilterChange('tipoPersona', e.target.value)}
                      >
                        <option value="">Tipo de Persona</option>
                        {tiposPersona.map((tipo) => (
                          <option key={tipo.value} value={tipo.value}>
                            {tipo.label}
                          </option>
                        ))}
                      </CFormSelect>
                    </CCol>
                    <CCol md={3}>
                      <CFormSelect
                        value={filters.tipoIdentificacion}
                        onChange={(e) => handleFilterChange('tipoIdentificacion', e.target.value)}
                      >
                        <option value="">Tipo de Identificación</option>
                        {tiposIdentificacion.map((tipo) => (
                          <option key={tipo.value} value={tipo.value}>
                            {tipo.label}
                          </option>
                        ))}
                      </CFormSelect>
                    </CCol>
                    <CCol md={3}>
                      <CFormInput
                        placeholder="Municipio"
                        value={filters.municipio}
                        onChange={(e) => handleFilterChange('municipio', e.target.value)}
                      />
                    </CCol>
                    <CCol md={3}>
                      <CFormInput
                        placeholder="Departamento"
                        value={filters.departamento}
                        onChange={(e) => handleFilterChange('departamento', e.target.value)}
                      />
                    </CCol>
                  </CRow>
                </CCardBody>
              </CCard>
            )}

            {loading ? (
              <div className="text-center">
                <CSpinner color="primary" />
                <p className="mt-2">Cargando terceros...</p>
              </div>
            ) : (
              <>
                <CTable hover responsive>
                  <CTableHead>
                    <CTableRow>
                      <CTableHeaderCell>Identificación</CTableHeaderCell>
                      <CTableHeaderCell>Nombre Completo</CTableHeaderCell>
                      <CTableHeaderCell>Tipo Persona</CTableHeaderCell>
                      <CTableHeaderCell>Correo</CTableHeaderCell>
                      <CTableHeaderCell>Municipio</CTableHeaderCell>
                      <CTableHeaderCell>Acciones</CTableHeaderCell>
                    </CTableRow>
                  </CTableHead>
                  <CTableBody>
                    {currentItems.length === 0 ? (
                      <CTableRow>
                        <CTableDataCell colSpan="6" className="text-center">
                          {terceros.length === 0
                            ? 'No hay terceros registrados'
                            : 'No se encontraron terceros con los criterios de búsqueda'}
                        </CTableDataCell>
                      </CTableRow>
                    ) : (
                      currentItems.map((tercero) => (
                        <CTableRow key={tercero.guid}>
                          <CTableDataCell>
                            <strong>{tercero.identificacion}</strong>
                            <br />
                            <small className="text-muted">
                              {getTipoIdentificacionLabel(tercero.tipoIdentificacion)}
                            </small>
                          </CTableDataCell>
                          <CTableDataCell>
                            <strong>
                              {`${tercero.nombre || ''} ${tercero.segundo || ''} ${
                                tercero.apellidos || ''
                              }`.trim()}
                            </strong>
                          </CTableDataCell>
                          <CTableDataCell>
                            <CBadge color={tercero.tipoPersona === 1 ? 'info' : 'warning'}>
                              {getTipoPersonaLabel(tercero.tipoPersona)}
                            </CBadge>
                          </CTableDataCell>
                          <CTableDataCell>{tercero.correo || '-'}</CTableDataCell>
                          <CTableDataCell>
                            {tercero.municipio || '-'}
                            {tercero.departamento && (
                              <>
                                <br />
                                <small className="text-muted">{tercero.departamento}</small>
                              </>
                            )}
                          </CTableDataCell>
                          <CTableDataCell>
                            <CButtonGroup size="sm">
                              <CTooltip content="Ver detalles">
                                <CButton
                                  color="info"
                                  variant="outline"
                                  onClick={() => handleViewTercero(tercero)}
                                >
                                  <CIcon icon={cilUserFollow} size="sm" />
                                </CButton>
                              </CTooltip>
                              <CTooltip content="Editar">
                                <CButton
                                  color="warning"
                                  variant="outline"
                                  onClick={() => handleEditTercero(tercero)}
                                >
                                  <CIcon icon={cilPencil} size="sm" />
                                </CButton>
                              </CTooltip>
                            </CButtonGroup>
                          </CTableDataCell>
                        </CTableRow>
                      ))
                    )}
                  </CTableBody>
                </CTable>

                {/* Paginación */}
                {totalPages > 1 && (
                  <div className="d-flex justify-content-between align-items-center mt-3">
                    <div>
                      Mostrando {indexOfFirstItem + 1} a{' '}
                      {Math.min(indexOfLastItem, tercerosFiltrados.length)} de{' '}
                      {tercerosFiltrados.length} terceros
                    </div>
                    <CPagination>
                      <CPaginationItem
                        disabled={currentPage === 1}
                        onClick={() => paginate(currentPage - 1)}
                      >
                        Anterior
                      </CPaginationItem>
                      {[...Array(totalPages)].map((_, index) => (
                        <CPaginationItem
                          key={index + 1}
                          active={currentPage === index + 1}
                          onClick={() => paginate(index + 1)}
                        >
                          {index + 1}
                        </CPaginationItem>
                      ))}
                      <CPaginationItem
                        disabled={currentPage === totalPages}
                        onClick={() => paginate(currentPage + 1)}
                      >
                        Siguiente
                      </CPaginationItem>
                    </CPagination>
                  </div>
                )}
              </>
            )}
          </CCardBody>
        </CCard>
      </CCol>
    </CRow>
  )
}

export default Terceros
