import { React, useState, useEffect, useRef } from 'react'
import EstacionService from '../../services/EstacionService'
import {
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CForm,
  CFormInput,
  CFormLabel,
  CBadge,
  CSpinner,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilPlus, cilLocationPin, cilPhone, cilContact, cilCalculator, cilCog } from '@coreui/icons'
import { useNavigate } from 'react-router-dom'
import Toast from '../toast/Toast'

const Dashboard = () => {
  const navigate = useNavigate()
  const toastRef = useRef()
  const [estaciones, setEstaciones] = useState([])
  const [estacionSeleccionada, setEstacionSeleccionada] = useState('')
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [saving, setSaving] = useState(false)
  const [nuevaEstacion, setNuevaEstacion] = useState({
    nombre: '',
    nit: '',
    direccion: '',
    telefono: '',
    razon: '',
    linea1: '',
    linea2: '',
    linea3: '',
    linea4: '',
  })
  const estacionService = new EstacionService()

  const fetchEstaciones = async () => {
    setLoading(true)
    try {
      // Check if token exists before making the request
      const token = localStorage.getItem('token')
      if (!token) {
        navigate('/Login', { replace: true })
        return
      }

      let response = await estacionService.getEstaciones()
      if (response == 'fail') {
        navigate('/Login', { replace: true })
      } else {
        setEstaciones(response)
        if (response.length > 0) {
          // Check if there's already a selected station in localStorage
          const selectedFromStorage = localStorage.getItem('estacion')
          if (selectedFromStorage) {
            setEstacionSeleccionada(selectedFromStorage)
          } else {
            // Set first station as default
            const firstStation = response[0]
            setEstacionSeleccionada(firstStation.guid)
            updateLocalStorage(firstStation)
          }
        }
      }
    } catch (error) {
      console.error('Error fetching estaciones:', error)
      toastRef.current?.addMessage('Error al cargar las estaciones', 'error')
      navigate('/Login', { replace: true })
    } finally {
      setLoading(false)
    }
  }

  const updateLocalStorage = (estacion) => {
    localStorage.setItem('estacion', estacion.guid)
    localStorage.setItem('estacionGuid', estacion.guid)
    localStorage.setItem('estacionNombre', estacion.nombre)
    localStorage.setItem('estacionNit', estacion.nit)
    if (estacion.direccion) {
      localStorage.setItem('estacionDireccion', estacion.direccion)
    }
    if (estacion.telefono) {
      localStorage.setItem('estacionTelefono', estacion.telefono)
    }
  }

  const handleEstacionSelect = (estacion) => {
    setEstacionSeleccionada(estacion.guid)
    updateLocalStorage(estacion)
    toastRef.current?.addMessage(`Estación "${estacion.nombre}" seleccionada`, 'success')
  }

  const handleInputChange = (e) => {
    const { name, value } = e.target
    setNuevaEstacion((prev) => ({
      ...prev,
      [name]: value,
    }))
  }

  const handleSaveEstacion = async () => {
    if (!nuevaEstacion.nombre.trim() || !nuevaEstacion.nit.trim()) {
      toastRef.current?.addMessage('Nombre y NIT son campos obligatorios', 'error')
      return
    }

    // Check if token exists before making the request
    const token = localStorage.getItem('token')
    if (!token) {
      navigate('/Login', { replace: true })
      return
    }

    setSaving(true)
    try {
      const response = await estacionService.addOrUpdate(nuevaEstacion)
      if (response === 'fail') {
        toastRef.current?.addMessage('Error al guardar la estación', 'error')
        navigate('/Login', { replace: true })
      } else {
        toastRef.current?.addMessage('Estación creada exitosamente', 'success')
        setShowModal(false)
        setNuevaEstacion({
          nombre: '',
          nit: '',
          direccion: '',
          telefono: '',
          razon: '',
          linea1: '',
          linea2: '',
          linea3: '',
          linea4: '',
        })
        fetchEstaciones() // Reload stations
      }
    } catch (error) {
      console.error('Error saving estacion:', error)
      toastRef.current?.addMessage('Error al guardar la estación', 'error')
      navigate('/Login', { replace: true })
    } finally {
      setSaving(false)
    }
  }

  const handleCloseModal = () => {
    setShowModal(false)
    setNuevaEstacion({
      nombre: '',
      nit: '',
      direccion: '',
      telefono: '',
      razon: '',
      linea1: '',
      linea2: '',
      linea3: '',
      linea4: '',
    })
  }

  useEffect(() => {
    fetchEstaciones()
  }, [])

  if (loading) {
    return (
      <div className="text-center p-5">
        <CSpinner color="primary" size="lg" />
        <div className="mt-3">Cargando estaciones...</div>
      </div>
    )
  }

  return (
    <>
      <div className="mb-4">
        <div className="d-flex justify-content-between align-items-center">
          <h2>
            <CIcon icon={cilCalculator} className="me-2" />
            Gestión de Estaciones
          </h2>
          <CButton
            color="primary"
            onClick={() => setShowModal(true)}
            className="d-flex align-items-center"
          >
            <CIcon icon={cilPlus} className="me-2" />
            Nueva Estación
          </CButton>
        </div>
        <p className="text-medium-emphasis">
          Selecciona una estación para trabajar. La estación seleccionada se aplicará a todos los
          módulos del sistema.
        </p>
      </div>

      <CRow>
        {estaciones.map((estacion) => (
          <CCol key={estacion.guid} sm={6} lg={4} xl={3}>
            <CCard
              className={`mb-4 cursor-pointer h-100 ${
                estacionSeleccionada === estacion.guid
                  ? 'border-primary shadow-lg'
                  : 'border-light hover-shadow'
              }`}
              style={{
                cursor: 'pointer',
                transition: 'all 0.3s ease',
                transform: estacionSeleccionada === estacion.guid ? 'translateY(-2px)' : 'none',
              }}
              onClick={() => handleEstacionSelect(estacion)}
            >
              <CCardHeader
                className={`d-flex justify-content-between align-items-center ${
                  estacionSeleccionada === estacion.guid ? 'bg-primary text-white' : 'bg-light'
                }`}
              >
                <div className="d-flex align-items-center">
                  <CIcon icon={cilCalculator} className="me-2" />
                  <strong>{estacion.nombre}</strong>
                </div>
                {estacionSeleccionada === estacion.guid && (
                  <CBadge color="success" className="ms-2">
                    <CIcon icon={cilCog} className="me-1" size="sm" />
                    Activa
                  </CBadge>
                )}
              </CCardHeader>
              <CCardBody>
                <div className="mb-2">
                  <small className="text-medium-emphasis">NIT:</small>
                  <div className="fw-semibold">{estacion.nit || 'No especificado'}</div>
                </div>

                {estacion.direccion && (
                  <div className="mb-2">
                    <small className="text-medium-emphasis d-flex align-items-center">
                      <CIcon icon={cilLocationPin} size="sm" className="me-1" />
                      Dirección:
                    </small>
                    <div className="fw-semibold">{estacion.direccion}</div>
                  </div>
                )}

                {estacion.telefono && (
                  <div className="mb-2">
                    <small className="text-medium-emphasis d-flex align-items-center">
                      <CIcon icon={cilPhone} size="sm" className="me-1" />
                      Teléfono:
                    </small>
                    <div className="fw-semibold">{estacion.telefono}</div>
                  </div>
                )}

                <div className="mt-3">
                  <small className="text-medium-emphasis">ID:</small>
                  <div className="small text-muted font-monospace">{estacion.guid}</div>
                </div>
              </CCardBody>
            </CCard>
          </CCol>
        ))}
      </CRow>

      {estaciones.length === 0 && (
        <div className="text-center p-5">
          <CIcon icon={cilCalculator} size="3xl" className="text-medium-emphasis mb-3" />
          <h4 className="text-medium-emphasis">No hay estaciones configuradas</h4>
          <p className="text-medium-emphasis">
            Comienza creando tu primera estación usando el botón Nueva Estación
          </p>
          <CButton color="primary" onClick={() => setShowModal(true)} className="mt-3">
            <CIcon icon={cilPlus} className="me-2" />
            Crear Primera Estación
          </CButton>
        </div>
      )}

      {/* Modal para Nueva Estación */}
      <CModal visible={showModal} onClose={handleCloseModal} size="lg">
        <CModalHeader>
          <CModalTitle>
            <CIcon icon={cilPlus} className="me-2" />
            Nueva Estación
          </CModalTitle>
        </CModalHeader>
        <CModalBody>
          <CForm>
            <div className="mb-3">
              <CFormLabel htmlFor="nombre">Nombre de la Estación *</CFormLabel>
              <CFormInput
                type="text"
                id="nombre"
                name="nombre"
                value={nuevaEstacion.nombre}
                onChange={handleInputChange}
                placeholder="Ingrese el nombre de la estación"
                required
              />
            </div>

            <div className="mb-3">
              <CFormLabel htmlFor="nit">NIT *</CFormLabel>
              <CFormInput
                type="text"
                id="nit"
                name="nit"
                value={nuevaEstacion.nit}
                onChange={handleInputChange}
                placeholder="Ingrese el NIT"
                required
              />
            </div>

            <div className="mb-3">
              <CFormLabel htmlFor="direccion">
                <CIcon icon={cilLocationPin} className="me-1" />
                Dirección
              </CFormLabel>
              <CFormInput
                type="text"
                id="direccion"
                name="direccion"
                value={nuevaEstacion.direccion}
                onChange={handleInputChange}
                placeholder="Ingrese la dirección (opcional)"
              />
            </div>

            <div className="mb-3">
              <CFormLabel htmlFor="telefono">
                <CIcon icon={cilPhone} className="me-1" />
                Teléfono
              </CFormLabel>
              <CFormInput
                type="text"
                id="telefono"
                name="telefono"
                value={nuevaEstacion.telefono}
                onChange={handleInputChange}
                placeholder="Ingrese el teléfono (opcional)"
              />
            </div>

            <div className="mb-3">
              <CFormLabel htmlFor="razon">Razón Social</CFormLabel>
              <CFormInput
                type="text"
                id="razon"
                name="razon"
                value={nuevaEstacion.razon}
                onChange={handleInputChange}
                placeholder="Ingrese la razón social (opcional)"
              />
            </div>

            <div className="mb-3">
              <CFormLabel htmlFor="linea1">Línea de encabezado 1</CFormLabel>
              <CFormInput
                type="text"
                id="linea1"
                name="linea1"
                value={nuevaEstacion.linea1}
                onChange={handleInputChange}
                placeholder="Línea adicional 1 (opcional)"
              />
            </div>

            <div className="mb-3">
              <CFormLabel htmlFor="linea2">Línea de encabezado 2</CFormLabel>
              <CFormInput
                type="text"
                id="linea2"
                name="linea2"
                value={nuevaEstacion.linea2}
                onChange={handleInputChange}
                placeholder="Línea adicional 2 (opcional)"
              />
            </div>

            <div className="mb-3">
              <CFormLabel htmlFor="linea3">Línea de encabezado 3</CFormLabel>
              <CFormInput
                type="text"
                id="linea3"
                name="linea3"
                value={nuevaEstacion.linea3}
                onChange={handleInputChange}
                placeholder="Línea adicional 3 (opcional)"
              />
            </div>

            <div className="mb-3">
              <CFormLabel htmlFor="linea4">Línea de encabezado 4</CFormLabel>
              <CFormInput
                type="text"
                id="linea4"
                name="linea4"
                value={nuevaEstacion.linea4}
                onChange={handleInputChange}
                placeholder="Línea adicional 4 (opcional)"
              />
            </div>
          </CForm>
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={handleCloseModal}>
            Cancelar
          </CButton>
          <CButton
            color="primary"
            onClick={handleSaveEstacion}
            disabled={saving || !nuevaEstacion.nombre.trim() || !nuevaEstacion.nit.trim()}
          >
            {saving ? (
              <>
                <CSpinner size="sm" className="me-2" />
                Guardando...
              </>
            ) : (
              <>
                <CIcon icon={cilContact} className="me-2" />
                Guardar Estación
              </>
            )}
          </CButton>
        </CModalFooter>
      </CModal>

      <Toast ref={toastRef} />
    </>
  )
}

export default Dashboard
