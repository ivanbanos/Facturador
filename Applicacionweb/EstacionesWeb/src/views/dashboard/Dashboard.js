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
  CFormCheck,
  CBadge,
  CSpinner,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilPlus,
  cilLocationPin,
  cilPhone,
  cilContact,
  cilCalculator,
  cilCog,
  cilSpeedometer,
} from '@coreui/icons'
import { useNavigate } from 'react-router-dom'
import Toast from '../toast/Toast'

const Dashboard = () => {
  const navigate = useNavigate()
  const toastRef = useRef()
  const [estaciones, setEstaciones] = useState([])
  const [estacionSeleccionada, setEstacionSeleccionada] = useState('')
  const [loading, setLoading] = useState(true)
  const [showModal, setShowModal] = useState(false)
  const [showConfirmCombustibleModal, setShowConfirmCombustibleModal] = useState(false)
  const [saving, setSaving] = useState(false)
  const [combustibles, setCombustibles] = useState([])
  const [combustiblePendiente, setCombustiblePendiente] = useState(null)
  const [savingCombustible, setSavingCombustible] = useState(false)
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
    esGas: false,
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
    fetchCombustibles(estacion.guid)
    toastRef.current?.addMessage(`Estación "${estacion.nombre}" seleccionada`, 'success')
  }

  const handleInputChange = (e) => {
    const { name, value, type, checked } = e.target
    setNuevaEstacion((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }))
  }

  const fetchCombustibles = async (estacionGuid = estacionSeleccionada) => {
    if (!estacionGuid) {
      setCombustibles([])
      return
    }

    try {
      const response = await estacionService.getCombustibles(estacionGuid)
      if (response === 'fail') {
        navigate('/Login', { replace: true })
        return
      }
      setCombustibles(Array.isArray(response) ? response : [])
    } catch (error) {
      console.error('Error loading combustibles:', error)
      toastRef.current?.addMessage('Error al cargar combustibles de la estación', 'error')
    }
  }

  const handleCombustiblePrecioChange = (index, value) => {
    setCombustibles((prev) => {
      const updated = [...prev]
      const parsed = Number(value)
      updated[index] = {
        ...updated[index],
        precio: Number.isFinite(parsed) ? parsed : 0,
      }
      return updated
    })
  }

  const abrirConfirmacionCombustible = (combustible) => {
    if (!estacionSeleccionada) {
      toastRef.current?.addMessage('Debe seleccionar una estación', 'warning')
      return
    }

    if (!combustible?.combustible || !(combustible.precio > 0)) {
      toastRef.current?.addMessage('El combustible y el precio son obligatorios', 'warning')
      return
    }

    setCombustiblePendiente(combustible)
    setShowConfirmCombustibleModal(true)
  }

  const confirmarActualizacionCombustible = async () => {
    if (!combustiblePendiente) {
      setShowConfirmCombustibleModal(false)
      return
    }

    setSavingCombustible(true)
    try {
      const response = await estacionService.actualizarCombustible(
        estacionSeleccionada,
        combustiblePendiente,
      )
      if (response === 'fail') {
        navigate('/Login', { replace: true })
        return
      }

      toastRef.current?.addMessage('Precio de combustible actualizado correctamente', 'success')
      setShowConfirmCombustibleModal(false)
      setCombustiblePendiente(null)
      await fetchCombustibles(estacionSeleccionada)
    } catch (error) {
      console.error('Error updating combustible:', error)
      toastRef.current?.addMessage('No fue posible actualizar el combustible', 'error')
    } finally {
      setSavingCombustible(false)
    }
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
          esGas: false,
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
      esGas: false,
    })
  }

  useEffect(() => {
    fetchEstaciones()
  }, [])

  useEffect(() => {
    if (estacionSeleccionada) {
      fetchCombustibles(estacionSeleccionada)
    }
  }, [estacionSeleccionada])

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

      {estacionSeleccionada && (
        <CCard className="mb-4">
          <CCardHeader>
            <strong>Precios de combustibles por estación</strong>
          </CCardHeader>
          <CCardBody>
            {combustibles.length === 0 ? (
              <div className="text-center text-medium-emphasis py-4">
                No hay combustibles configurados para esta estación.
              </div>
            ) : (
              <CRow>
                {combustibles.map((combustible, index) => (
                  <CCol key={`${combustible.combustible}-${index}`} md={6} xl={4} className="mb-3">
                    <CCard className="h-100 border-light">
                      <CCardBody>
                        <div className="d-flex justify-content-between align-items-start mb-3">
                          <div>
                            <div className="fw-semibold">{combustible.combustible}</div>
                            <CBadge
                              color={combustible.esGas ? 'warning' : 'primary'}
                              className="mt-1"
                            >
                              {combustible.esGas ? 'Gas' : 'Combustible'}
                            </CBadge>
                          </div>
                          <div className="text-primary opacity-75">
                            <CIcon icon={cilSpeedometer} size="xl" />
                          </div>
                        </div>

                        <div className="mb-3">
                          <CFormLabel className="small text-medium-emphasis">Precio</CFormLabel>
                          <CFormInput
                            type="number"
                            min="0"
                            step="0.001"
                            value={combustible.precio ?? 0}
                            onChange={(e) => handleCombustiblePrecioChange(index, e.target.value)}
                          />
                        </div>

                        <div className="d-grid">
                          <CButton
                            color="primary"
                            onClick={() => abrirConfirmacionCombustible(combustible)}
                          >
                            Guardar
                          </CButton>
                        </div>
                      </CCardBody>
                    </CCard>
                  </CCol>
                ))}
              </CRow>
            )}
          </CCardBody>
        </CCard>
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

            <div className="mb-3">
              <CFormCheck
                id="esGas"
                name="esGas"
                checked={nuevaEstacion.esGas}
                onChange={handleInputChange}
                label="La estación opera con gas (GNVC/G.N.V.C/Gas)"
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

      <CModal
        visible={showConfirmCombustibleModal}
        onClose={() => setShowConfirmCombustibleModal(false)}
      >
        <CModalHeader>
          <CModalTitle>Confirmar actualización</CModalTitle>
        </CModalHeader>
        <CModalBody>
          {combustiblePendiente
            ? `¿Está seguro de actualizar ${combustiblePendiente.combustible} a ${Number(
                combustiblePendiente.precio || 0,
              ).toLocaleString('es-CO')}?`
            : '¿Está seguro de actualizar el precio del combustible?'}
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowConfirmCombustibleModal(false)}>
            Cancelar
          </CButton>
          <CButton
            color="primary"
            onClick={confirmarActualizacionCombustible}
            disabled={savingCombustible}
          >
            {savingCombustible ? (
              <>
                <CSpinner size="sm" className="me-2" />
                Guardando...
              </>
            ) : (
              'Sí, guardar'
            )}
          </CButton>
        </CModalFooter>
      </CModal>

      <Toast ref={toastRef} />
    </>
  )
}

export default Dashboard
