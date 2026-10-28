import React, { useState, useRef, useEffect, useMemo } from 'react'
import OrdenesDespachoService from '../../services/OrdenesDespachoService'
import ReenviarOrdenesDespachoPorIdVentaLocal from '../../services/ReenviarOrdenesDespachoPorIdVentaLocal'
import { useNavigate } from 'react-router-dom'
import {
  CButton,
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CFormInput,
  CFormLabel,
  CSpinner,
  CButtonGroup,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilCalendar, cilSearch, cilTruck, cilCloudDownload, cilSpreadsheet } from '@coreui/icons'
import Toast from '../toast/Toast'
import { isCliente } from '../../services/AuthService'
import FiltrosOrdenes from './ordenes/FiltrosOrdenes'
import ResumenOrdenes from './ordenes/ResumenOrdenes'
import TablaOrdenes from './ordenes/TablaOrdenes'
import { descargarOrdenesExcel, descargarOrdenesPDF } from './ordenes/exportarOrdenes'
import {
  FILTROS_VACIOS,
  calcularEstadisticas,
  filtrarOrdenes,
  opcionesFiltroOrdenes,
} from './ordenes/ordenesUtils'

const getTodayDate = () => new Date().toISOString().split('T')[0]

const OrdenesDespacho = () => {
  const navigate = useNavigate()
  const toastRef = useRef()
  const ordenesService = new OrdenesDespachoService()

  const [fechaInicial, setFechaInicial] = useState('')
  const [fechaFinal, setFechaFinal] = useState('')
  const [ordenes, setOrdenes] = useState([])
  const [loading, setLoading] = useState(false)
  const [showResults, setShowResults] = useState(false)
  const [filtros, setFiltros] = useState(FILTROS_VACIOS)

  // Inicializar fechas y cargar las órdenes del día actual
  useEffect(() => {
    const today = getTodayDate()
    setFechaInicial(today)
    setFechaFinal(today)
    handleSearch(today, today)
  }, [])

  const handleSearch = async (fechaIni = fechaInicial, fechaFin = fechaFinal) => {
    if (!fechaIni || !fechaFin) {
      toastRef.current?.addMessage('Debe seleccionar las fechas inicial y final', 'error')
      return
    }

    if (fechaIni > fechaFin) {
      toastRef.current?.addMessage(
        'La fecha inicial no puede ser mayor que la fecha final',
        'error',
      )
      return
    }

    setLoading(true)
    try {
      const response = await ordenesService.getOrdenesByDateRange(fechaIni, fechaFin)
      if (response === 'fail') {
        navigate('/Login', { replace: true })
        return
      }

      setOrdenes(response || [])
      setShowResults(true)
      setFiltros(FILTROS_VACIOS)

      toastRef.current?.addMessage(`Se encontraron ${(response || []).length} órdenes`, 'success')
    } catch (error) {
      console.error('Error:', error)
      toastRef.current?.addMessage('Error al buscar las órdenes', 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleFechaInicialChange = (value) => {
    setFechaInicial(value)
    if (fechaFinal && value > fechaFinal) {
      setFechaFinal(value)
    }
  }

  const handleFechaFinalChange = (value) => {
    setFechaFinal(value)
    if (fechaInicial && value < fechaInicial) {
      setFechaInicial(value)
    }
  }

  // Memorizados: la paginación vuelve a la página 1 solo cuando cambia el resultado filtrado
  const opciones = useMemo(() => opcionesFiltroOrdenes(ordenes), [ordenes])
  const filteredOrdenes = useMemo(() => filtrarOrdenes(ordenes, filtros), [ordenes, filtros])
  const estadisticas = useMemo(() => calcularEstadisticas(filteredOrdenes), [filteredOrdenes])

  const goToOrdenDetalle = (orden) => {
    navigate(`/OrdenDespachoDetalle/${orden.guid || orden.idVentaLocal}`, {
      state: { orden: orden },
    })
  }

  const opcionesExportacion = () => ({
    fechaInicial,
    fechaFinal,
    estadisticas,
    encabezado: localStorage.getItem('estacionNombre') || 'Estación',
    nit: localStorage.getItem('estacionNit') || 'N/A',
  })

  const descargar = (formato) => {
    if (!filteredOrdenes.length) {
      toastRef.current?.addMessage('No hay órdenes para descargar', 'error')
      return
    }
    try {
      if (formato === 'pdf') {
        descargarOrdenesPDF(filteredOrdenes, opcionesExportacion())
        toastRef.current?.addMessage('Reporte descargado exitosamente', 'success')
      } else {
        descargarOrdenesExcel(filteredOrdenes, opcionesExportacion())
        toastRef.current?.addMessage('Reporte Excel descargado exitosamente', 'success')
      }
    } catch (error) {
      console.error('Error downloading report:', error)
      toastRef.current?.addMessage('Error al descargar el reporte', 'error')
    }
  }

  const reenviarAFacturacion = async (ordenesSinFacturar) => {
    const idVentaLocalList = ordenesSinFacturar
      .map((orden) => orden.idVentaLocal)
      .filter((id) => id !== undefined && id !== null)

    if (!idVentaLocalList.length) {
      toastRef.current?.addMessage('No hay órdenes válidas para reenviar.', 'warning')
      return
    }

    toastRef.current?.addMessage('Enviando órdenes a facturación...', 'info')
    try {
      const result = await ReenviarOrdenesDespachoPorIdVentaLocal(idVentaLocalList)
      if (result === 'fail') {
        toastRef.current?.addMessage('Error al reenviar las órdenes.', 'error')
      } else {
        toastRef.current?.addMessage(
          `Órdenes reenviadas correctamente (${idVentaLocalList.length}).`,
          'success',
        )
      }
    } catch (error) {
      toastRef.current?.addMessage('Error inesperado al reenviar.', 'error')
    }
  }

  const puedeReenviar = !isCliente() && filtros.factura === 'sin' && filteredOrdenes.length > 0

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>
          <h4 className="mb-0">
            <CIcon icon={cilTruck} className="me-2" />
            Órdenes de Despacho
          </h4>
        </CCardHeader>
        <CCardBody>
          <CRow className="g-2 align-items-end">
            <CCol xs={6} md={4} lg={3}>
              <CFormLabel htmlFor="fechaInicial">
                <CIcon icon={cilCalendar} className="me-1" />
                Fecha Inicial
              </CFormLabel>
              <CFormInput
                type="date"
                id="fechaInicial"
                value={fechaInicial}
                max={fechaFinal || undefined}
                onChange={(e) => handleFechaInicialChange(e.target.value)}
              />
            </CCol>
            <CCol xs={6} md={4} lg={3}>
              <CFormLabel htmlFor="fechaFinal">
                <CIcon icon={cilCalendar} className="me-1" />
                Fecha Final
              </CFormLabel>
              <CFormInput
                type="date"
                id="fechaFinal"
                value={fechaFinal}
                min={fechaInicial || undefined}
                onChange={(e) => handleFechaFinalChange(e.target.value)}
              />
            </CCol>
            <CCol xs={12} md={4} lg={2} className="d-grid">
              <CButton color="primary" onClick={() => handleSearch()} disabled={loading}>
                {loading ? (
                  <>
                    <CSpinner size="sm" className="me-2" />
                    Buscando...
                  </>
                ) : (
                  <>
                    <CIcon icon={cilSearch} className="me-2" />
                    Buscar
                  </>
                )}
              </CButton>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {showResults && (
        <>
          <ResumenOrdenes estadisticas={estadisticas} />

          <CCard>
            <CCardHeader className="d-flex flex-wrap justify-content-between align-items-center gap-2">
              <h5 className="mb-0">Resultados ({filteredOrdenes.length} órdenes)</h5>
              <div className="d-flex flex-wrap gap-2">
                {puedeReenviar && (
                  <CButton
                    color="warning"
                    size="sm"
                    onClick={() => reenviarAFacturacion(filteredOrdenes)}
                  >
                    <CIcon icon={cilCloudDownload} className="me-2" />
                    Reenviar a facturación ({filteredOrdenes.length})
                  </CButton>
                )}
                <CButtonGroup size="sm">
                  <CButton
                    color="danger"
                    variant="outline"
                    onClick={() => descargar('pdf')}
                    disabled={!filteredOrdenes.length}
                  >
                    <CIcon icon={cilCloudDownload} className="me-2" />
                    PDF
                  </CButton>
                  <CButton
                    color="success"
                    variant="outline"
                    onClick={() => descargar('excel')}
                    disabled={!filteredOrdenes.length}
                  >
                    <CIcon icon={cilSpreadsheet} className="me-2" />
                    Excel
                  </CButton>
                </CButtonGroup>
              </div>
            </CCardHeader>
            <CCardBody>
              <FiltrosOrdenes filtros={filtros} opciones={opciones} onChange={setFiltros} />
              <TablaOrdenes ordenes={filteredOrdenes} onVer={goToOrdenDetalle} />
            </CCardBody>
          </CCard>
        </>
      )}

      <Toast ref={toastRef} />
    </>
  )
}

export default OrdenesDespacho
