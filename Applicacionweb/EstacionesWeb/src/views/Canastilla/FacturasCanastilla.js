import React, { useState, useEffect, useRef, useMemo } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  CCard,
  CCardBody,
  CCardHeader,
  CButton,
  CRow,
  CCol,
  CTable,
  CTableBody,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CTableDataCell,
  CFormInput,
  CSpinner,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilSearch, cilContact, cilCloudDownload, cilCalendar } from '@coreui/icons'
import FacturasCanastillasService from '../../services/FacturasCanastillasService'
import Toast from '../toast/Toast'
import DetalleReporteCanastilla from './facturas/DetalleReporteCanastilla'
import FiltrosCanastilla from './facturas/FiltrosCanastilla'
import ResumenCanastilla from './facturas/ResumenCanastilla'
import TablaFacturasCanastilla from './facturas/TablaFacturasCanastilla'
import { descargarCanastillaPDF } from './facturas/exportarCanastilla'
import {
  FILTROS_VACIOS,
  calcularTotales,
  filtrarFacturas,
  formatCurrency,
  formatDate,
  hayFiltrosActivos,
  idFactura,
  opcionesFiltroFacturas,
} from './facturas/canastillaUtils'

const FacturasCanastilla = () => {
  const navigate = useNavigate()
  const toastRef = useRef()
  const facturasService = new FacturasCanastillasService()

  const [reporteData, setReporteData] = useState(null)
  const [facturas, setFacturas] = useState([])
  const [loading, setLoading] = useState(false)
  const [filtros, setFiltros] = useState(FILTROS_VACIOS)
  const [fechaInicial, setFechaInicial] = useState('')
  const [fechaFinal, setFechaFinal] = useState('')
  const [showDetailModal, setShowDetailModal] = useState(false)
  const [selectedFactura, setSelectedFactura] = useState(null)
  const [loadingDetail, setLoadingDetail] = useState(false)

  // Fechas por defecto: último mes
  useEffect(() => {
    const today = new Date()
    const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, today.getDate())

    setFechaInicial(lastMonth.toISOString().split('T')[0])
    setFechaFinal(today.toISOString().split('T')[0])
  }, [])

  const buscarFacturas = async () => {
    if (!fechaInicial || !fechaFinal) {
      toastRef.current?.addMessage('Debe seleccionar ambas fechas', 'warning')
      return
    }

    if (fechaInicial > fechaFinal) {
      toastRef.current?.addMessage(
        'La fecha inicial no puede ser mayor que la fecha final',
        'warning',
      )
      return
    }

    setLoading(true)
    try {
      const filtro = facturasService.createFiltroBusqueda(fechaInicial, fechaFinal)
      const response = await facturasService.getFacturasReporte(filtro)

      if (response === 'fail') {
        navigate('/Login', { replace: true })
        toastRef.current?.addMessage('Error de autenticación. Redirigiendo al login...', 'error')
      } else {
        setReporteData(response)
        setFacturas(response.facturas || [])
        setFiltros(FILTROS_VACIOS)
        toastRef.current?.addMessage(
          `Se encontraron ${response.facturas?.length || 0} facturas`,
          'success',
        )
      }
    } catch (error) {
      console.error('Error loading facturas:', error)
      toastRef.current?.addMessage('Error al cargar las facturas', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Memorizados: la paginación vuelve a la página 1 solo cuando cambia el resultado filtrado
  const opciones = useMemo(() => opcionesFiltroFacturas(facturas), [facturas])
  const filteredFacturas = useMemo(() => filtrarFacturas(facturas, filtros), [facturas, filtros])
  const totales = useMemo(() => calcularTotales(filteredFacturas), [filteredFacturas])

  const openDetailModal = async (factura) => {
    setSelectedFactura(factura)
    setShowDetailModal(true)
    setLoadingDetail(true)

    try {
      const detalle = await facturasService.getFacturaDetalle(idFactura(factura))
      setSelectedFactura({ ...factura, detalle })
    } catch (error) {
      console.error('Error loading factura detail:', error)
      toastRef.current?.addMessage('Error al cargar el detalle de la factura', 'error')
    } finally {
      setLoadingDetail(false)
    }
  }

  const verDetalle = (factura) => {
    const facturaId = idFactura(factura)
    if (facturaId) {
      navigate(`/FacturaCanastillaDetalle/${facturaId}`)
    } else {
      toastRef.current?.addMessage('Error: ID de factura no encontrado', 'error')
    }
  }

  const generatePDFReport = () => {
    descargarCanastillaPDF(filteredFacturas, {
      // Los desgloses del API son de todo el período: con filtros no coincidirían con las filas
      reporte: hayFiltrosActivos(filtros) ? null : reporteData,
      totales,
      fechaInicial,
      fechaFinal,
      encabezado: localStorage.getItem('estacionNombre') || 'Estación',
      nit: localStorage.getItem('estacionNit') || '',
    })
    toastRef.current?.addMessage('Reporte PDF generado exitosamente', 'success')
  }

  return (
    <>
      <CCard>
        <CCardHeader className="d-flex flex-wrap justify-content-between align-items-center gap-2">
          <h4 className="mb-0">
            <CIcon icon={cilContact} className="me-2" />
            Facturas de Canastilla
          </h4>
          <CButton
            color="success"
            variant="outline"
            size="sm"
            onClick={generatePDFReport}
            disabled={loading || filteredFacturas.length === 0}
          >
            <CIcon icon={cilCloudDownload} className="me-2" />
            Exportar PDF
          </CButton>
        </CCardHeader>

        <CCardBody>
          <CRow className="g-2 align-items-end mb-3">
            <CCol xs={6} md={4} lg={3}>
              <label className="form-label">
                <CIcon icon={cilCalendar} className="me-1" />
                Fecha Inicial *
              </label>
              <CFormInput
                type="date"
                value={fechaInicial}
                max={fechaFinal || undefined}
                onChange={(e) => setFechaInicial(e.target.value)}
              />
            </CCol>
            <CCol xs={6} md={4} lg={3}>
              <label className="form-label">
                <CIcon icon={cilCalendar} className="me-1" />
                Fecha Final *
              </label>
              <CFormInput
                type="date"
                value={fechaFinal}
                min={fechaInicial || undefined}
                onChange={(e) => setFechaFinal(e.target.value)}
              />
            </CCol>
            <CCol xs={12} md={4} lg={2} className="d-grid">
              <CButton color="primary" onClick={buscarFacturas} disabled={loading}>
                {loading ? (
                  <CSpinner size="sm" />
                ) : (
                  <>
                    <CIcon icon={cilSearch} className="me-2" />
                    Buscar
                  </>
                )}
              </CButton>
            </CCol>
          </CRow>

          {facturas.length > 0 && (
            <>
              <ResumenCanastilla totales={totales} />
              <DetalleReporteCanastilla reporte={reporteData} />
              <FiltrosCanastilla filtros={filtros} opciones={opciones} onChange={setFiltros} />
            </>
          )}

          {loading ? (
            <div className="text-center p-3">
              <CSpinner color="primary" />
              <div className="mt-2">Cargando facturas...</div>
            </div>
          ) : (
            <TablaFacturasCanastilla
              facturas={filteredFacturas}
              onResumen={openDetailModal}
              onDetalle={verDetalle}
              mensajeVacio={
                facturas.length === 0
                  ? 'No hay facturas. Use el botón "Buscar" para cargar datos.'
                  : 'No se encontraron facturas con el criterio de búsqueda'
              }
            />
          )}
        </CCardBody>
      </CCard>

      <CModal
        visible={showDetailModal}
        onClose={() => setShowDetailModal(false)}
        size="lg"
        fullscreen="md"
      >
        <CModalHeader>
          <CModalTitle>Detalle de Factura - {selectedFactura?.consecutivo}</CModalTitle>
        </CModalHeader>
        <CModalBody>
          {loadingDetail ? (
            <div className="text-center p-3">
              <CSpinner color="primary" />
              <div className="mt-2">Cargando detalle...</div>
            </div>
          ) : (
            selectedFactura && (
              <>
                <CRow>
                  <CCol md={6}>
                    <p>
                      <strong>Consecutivo:</strong> {selectedFactura.consecutivo}
                    </p>
                    <p>
                      <strong>Fecha:</strong> {formatDate(selectedFactura.fecha)}
                    </p>
                    <p>
                      <strong>Tercero:</strong> {selectedFactura.terceroId?.nombre}
                    </p>
                    <p>
                      <strong>Identificación:</strong> {selectedFactura.terceroId?.identificacion}
                    </p>
                  </CCol>
                  <CCol md={6}>
                    <p>
                      <strong>Subtotal:</strong> {formatCurrency(selectedFactura.subtotal)}
                    </p>
                    <p>
                      <strong>Descuento:</strong> {formatCurrency(selectedFactura.descuento)}
                    </p>
                    <p>
                      <strong>IVA:</strong> {formatCurrency(selectedFactura.iva)}
                    </p>
                    <p>
                      <strong>Total:</strong> {formatCurrency(selectedFactura.total)}
                    </p>
                  </CCol>
                </CRow>

                {selectedFactura.canastillas && selectedFactura.canastillas.length > 0 && (
                  <>
                    <hr />
                    <h5>Items de la Factura</h5>
                    <CTable size="sm" responsive>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>Descripción</CTableHeaderCell>
                          <CTableHeaderCell className="text-end">Cantidad</CTableHeaderCell>
                          <CTableHeaderCell className="text-end">Precio</CTableHeaderCell>
                          <CTableHeaderCell className="text-end">Subtotal</CTableHeaderCell>
                          <CTableHeaderCell className="text-end">IVA</CTableHeaderCell>
                          <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {selectedFactura.canastillas.map((item, index) => (
                          <CTableRow key={index}>
                            <CTableDataCell>{item.canastilla?.descripcion || '-'}</CTableDataCell>
                            <CTableDataCell className="text-end">{item.cantidad}</CTableDataCell>
                            <CTableDataCell className="text-end">
                              {formatCurrency(item.precio)}
                            </CTableDataCell>
                            <CTableDataCell className="text-end">
                              {formatCurrency(item.subtotal)}
                            </CTableDataCell>
                            <CTableDataCell className="text-end">
                              {formatCurrency(item.iva)}
                            </CTableDataCell>
                            <CTableDataCell className="text-end">
                              <strong>{formatCurrency(item.total)}</strong>
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>
                  </>
                )}
              </>
            )
          )}
        </CModalBody>
        <CModalFooter>
          <CButton color="secondary" onClick={() => setShowDetailModal(false)}>
            Cerrar
          </CButton>
        </CModalFooter>
      </CModal>

      <Toast ref={toastRef} />
    </>
  )
}

export default FacturasCanastilla
