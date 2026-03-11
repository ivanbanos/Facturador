import React, { useState, useEffect, useRef } from 'react'
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
  CBadge,
  CButtonGroup,
  CAlert,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CAccordion,
  CAccordionItem,
  CAccordionHeader,
  CAccordionBody,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilSearch,
  cilReload,
  cilContact,
  cilCloudDownload,
  cilCalendar,
  cilChart,
} from '@coreui/icons'
import FacturasCanastillasService from '../../services/FacturasCanastillasService'
import Toast from '../toast/Toast'

// PDFMake para generar reportes
var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

// Formato de moneda colombiana
const formatCurrency = (amount) => {
  return new Intl.NumberFormat('es-CO', {
    style: 'currency',
    currency: 'COP',
    minimumFractionDigits: 0,
  }).format(amount || 0)
}

// Formato de fecha
const formatDate = (dateString) => {
  if (!dateString) return '-'
  const date = new Date(dateString)
  return date.toLocaleDateString('es-CO')
}

// Función para transformar el estado mostrado al usuario
const getEstadoDisplay = (estado) => {
  if (!estado) return 'Activa'
  const estadoLower = estado.toLowerCase()
  if (estadoLower === 'anulada' || estadoLower === 'anulado') {
    return 'Facturada'
  }
  return estado
}

const FacturasCanastilla = () => {
  const navigate = useNavigate()
  const toastRef = useRef()
  const facturasService = new FacturasCanastillasService()

  // State management
  const [reporteData, setReporteData] = useState(null)
  const [facturas, setFacturas] = useState([])
  const [loading, setLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [fechaInicial, setFechaInicial] = useState('')
  const [fechaFinal, setFechaFinal] = useState('')
  const [showDetailModal, setShowDetailModal] = useState(false)
  const [selectedFactura, setSelectedFactura] = useState(null)
  const [loadingDetail, setLoadingDetail] = useState(false)

  // Estados calculados
  const [totalesCalculados, setTotalesCalculados] = useState({
    cantidad: 0,
    subtotal: 0,
    descuento: 0,
    iva: 0,
    total: 0,
  })

  // Cargar facturas con filtro inicial
  useEffect(() => {
    // Establecer fechas por defecto (último mes)
    const today = new Date()
    const lastMonth = new Date(today.getFullYear(), today.getMonth() - 1, today.getDate())

    setFechaInicial(lastMonth.toISOString().split('T')[0])
    setFechaFinal(today.toISOString().split('T')[0])
  }, [])

  // Filtrar facturas cuando se realice una búsqueda
  const filtrarFacturas = async () => {
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
        calcularTotales(response.facturas || [])
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

  const calcularTotales = (facturasArray) => {
    const totals = facturasArray.reduce(
      (acc, factura) => ({
        cantidad: acc.cantidad + 1,
        subtotal: acc.subtotal + (factura.subtotal || 0),
        descuento: acc.descuento + (factura.descuento || 0),
        iva: acc.iva + (factura.iva || 0),
        total: acc.total + (factura.total || 0),
      }),
      { cantidad: 0, subtotal: 0, descuento: 0, iva: 0, total: 0 },
    )
    setTotalesCalculados(totals)
  }

  const getFilteredFacturas = () => {
    if (!searchTerm) return facturas

    return facturas.filter(
      (factura) =>
        factura.consecutivo?.toString().toLowerCase().includes(searchTerm.toLowerCase()) ||
        factura.terceroId?.nombre?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        factura.terceroId?.identificacion?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        formatDate(factura.fecha).toLowerCase().includes(searchTerm.toLowerCase()),
    )
  }

  const openDetailModal = async (factura) => {
    setSelectedFactura(factura)
    setShowDetailModal(true)
    setLoadingDetail(true)

    try {
      const detalle = await facturasService.getFacturaDetalle(
        factura.id || factura.facturasCanastillaId,
      )
      setSelectedFactura({ ...factura, detalle })
    } catch (error) {
      console.error('Error loading factura detail:', error)
      toastRef.current?.addMessage('Error al cargar el detalle de la factura', 'error')
    } finally {
      setLoadingDetail(false)
    }
  }

  const verDetalle = (factura) => {
    const facturaId = factura.id || factura.facturasCanastillaId
    if (facturaId) {
      navigate(`/FacturaCanastillaDetalle/${facturaId}`)
    } else {
      toastRef.current?.addMessage('Error: ID de factura no encontrado', 'error')
    }
  }

  const generatePDFReport = () => {
    const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
    const estacionNit = localStorage.getItem('estacionNit') || ''
    const today = new Date().toLocaleDateString('es-CO')

    const filteredData = getFilteredFacturas()

    // Construir tabla de facturas
    const facturasTable = [
      ['Consecutivo', 'Fecha', 'Tercero', 'Subtotal', 'Descuento', 'IVA', 'Total'],
      ...filteredData.map((factura) => [
        factura.consecutivo || '-',
        formatDate(factura.fecha),
        factura.terceroId?.nombre || '-',
        { text: formatCurrency(factura.subtotal), style: 'tableRight' },
        { text: formatCurrency(factura.descuento), style: 'tableRight' },
        { text: formatCurrency(factura.iva), style: 'tableRight' },
        { text: formatCurrency(factura.total), style: 'tableRight' },
      ]),
    ]

    // Construir tabla de formas de pago si existe
    let formasPagoTable = []
    if (reporteData?.detalleFormaPago?.length > 0) {
      formasPagoTable = [
        ['Forma de Pago', 'Cantidad', 'Total'],
        ...reporteData.detalleFormaPago.map((forma) => [
          forma.formaDePago || '-',
          forma.cantidad || 0,
          { text: formatCurrency(forma.precio), style: 'tableRight' },
        ]),
      ]
    }

    // Construir tabla de artículos si existe
    let articulosTable = []
    if (reporteData?.detalleArticulo?.length > 0) {
      articulosTable = [
        ['Artículo', 'Cantidad', 'Subtotal', 'IVA', 'Total'],
        ...reporteData.detalleArticulo.map((articulo) => [
          articulo.descripcion || '-',
          articulo.cantidad || 0,
          { text: formatCurrency(articulo.subtotal), style: 'tableRight' },
          { text: formatCurrency(articulo.iva), style: 'tableRight' },
          { text: formatCurrency(articulo.total), style: 'tableRight' },
        ]),
      ]
    }

    const docDefinition = {
      watermark: {
        text: 'SIGES Soluciones',
        color: 'blue',
        opacity: 0.1,
        bold: true,
      },
      footer: function (currentPage, pageCount) {
        return `SIGES Soluciones - ${estacionNombre} - Página ${currentPage} de ${pageCount}`
      },
      content: [
        {
          stack: [estacionNombre, { text: `NIT ${estacionNit}`, style: 'subheader' }],
          style: 'header',
        },
        {
          text: 'Reporte de Facturas de Canastilla',
          style: 'header',
        },
        {
          text: `Período: ${formatDate(fechaInicial)} - ${formatDate(fechaFinal)}`,
          style: 'subheader',
        },
        { text: '\n' },
        {
          text: 'Resumen General',
          style: 'header',
        },
        {
          columns: [
            { text: `Cantidad de Facturas: ${totalesCalculados.cantidad}` },
            { text: `Total General: ${formatCurrency(totalesCalculados.total)}` },
          ],
        },
        {
          columns: [
            { text: `Subtotal: ${formatCurrency(totalesCalculados.subtotal)}` },
            { text: `Descuento: ${formatCurrency(totalesCalculados.descuento)}` },
          ],
        },
        {
          text: `IVA Total: ${formatCurrency(totalesCalculados.iva)}`,
        },
        { text: '\n' },
        {
          text: 'Detalle de Facturas',
          style: 'header',
        },
        {
          style: 'tableExample',
          table: {
            headerRows: 1,
            widths: ['auto', 'auto', '*', 'auto', 'auto', 'auto', 'auto'],
            body: facturasTable,
          },
        },

        // Agregar secciones adicionales si existen datos
        ...(formasPagoTable.length > 0
          ? [
              { text: '\n' },
              {
                text: 'Detalle por Forma de Pago',
                style: 'header',
              },
              {
                style: 'tableExample',
                table: {
                  headerRows: 1,
                  body: formasPagoTable,
                },
              },
            ]
          : []),

        ...(articulosTable.length > 0
          ? [
              { text: '\n' },
              {
                text: 'Detalle por Artículo',
                style: 'header',
              },
              {
                style: 'tableExample',
                table: {
                  headerRows: 1,
                  body: articulosTable,
                },
              },
            ]
          : []),
      ],

      styles: {
        header: {
          fontSize: 18,
          bold: true,
          alignment: 'center',
        },
        subheader: {
          fontSize: 14,
          alignment: 'center',
        },
        tableExample: {
          margin: [0, 5, 0, 15],
        },
        tableRight: {
          alignment: 'right',
        },
        tableHeader: {
          bold: true,
          fontSize: 13,
          color: 'black',
        },
      },
    }

    pdfMake.createPdf(docDefinition).download(`FacturasCanastilla-${estacionNombre}-${today}.pdf`)
    toastRef.current?.addMessage('Reporte PDF generado exitosamente', 'success')
  }

  const filteredFacturas = getFilteredFacturas()

  return (
    <>
      <CCard>
        <CCardHeader>
          <CRow>
            <CCol>
              <h4>
                <CIcon icon={cilContact} className="me-2" />
                Facturas de Canastilla
              </h4>
            </CCol>
            <CCol xs="auto">
              <CButtonGroup>
                <CButton
                  color="success"
                  variant="outline"
                  onClick={generatePDFReport}
                  disabled={loading || facturas.length === 0}
                >
                  <CIcon icon={cilCloudDownload} className="me-2" />
                  Exportar PDF
                </CButton>
                <CButton color="primary" onClick={filtrarFacturas} disabled={loading}>
                  <CIcon icon={cilSearch} className="me-2" />
                  Buscar
                </CButton>
              </CButtonGroup>
            </CCol>
          </CRow>
        </CCardHeader>

        <CCardBody>
          {/* Filtros */}
          <CRow className="mb-3">
            <CCol md={3}>
              <div className="mb-2">
                <label className="form-label">
                  <CIcon icon={cilCalendar} className="me-1" />
                  Fecha Inicial *
                </label>
                <CFormInput
                  type="date"
                  value={fechaInicial}
                  onChange={(e) => setFechaInicial(e.target.value)}
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="mb-2">
                <label className="form-label">
                  <CIcon icon={cilCalendar} className="me-1" />
                  Fecha Final *
                </label>
                <CFormInput
                  type="date"
                  value={fechaFinal}
                  onChange={(e) => setFechaFinal(e.target.value)}
                />
              </div>
            </CCol>
            <CCol md={4}>
              <div className="mb-2">
                <label className="form-label">
                  <CIcon icon={cilSearch} className="me-1" />
                  Buscar en resultados
                </label>
                <CFormInput
                  placeholder="Consecutivo, tercero, identificación..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                />
              </div>
            </CCol>
            <CCol md={2}>
              <div className="mb-2">
                <label className="form-label">&nbsp;</label>
                <div className="d-grid">
                  <CButton color="primary" onClick={filtrarFacturas} disabled={loading}>
                    <CIcon icon={cilSearch} className="me-2" />
                    {loading ? <CSpinner size="sm" /> : 'Filtrar'}
                  </CButton>
                </div>
              </div>
            </CCol>
          </CRow>

          {/* Resumen */}
          {facturas.length > 0 && (
            <CAlert color="info" className="mb-3">
              <CRow>
                <CCol md={2}>
                  <strong>Cantidad:</strong> {totalesCalculados.cantidad}
                </CCol>
                <CCol md={2}>
                  <strong>Subtotal:</strong> {formatCurrency(totalesCalculados.subtotal)}
                </CCol>
                <CCol md={2}>
                  <strong>Descuento:</strong> {formatCurrency(totalesCalculados.descuento)}
                </CCol>
                <CCol md={2}>
                  <strong>IVA:</strong> {formatCurrency(totalesCalculados.iva)}
                </CCol>
                <CCol md={4}>
                  <strong>Total:</strong> {formatCurrency(totalesCalculados.total)}
                </CCol>
              </CRow>
            </CAlert>
          )}

          {/* Detalles adicionales del reporte (si existen) */}
          {reporteData &&
            (reporteData.detalleFormaPago?.length > 0 ||
              reporteData.detalleArticulo?.length > 0) && (
              <CAccordion className="mb-3">
                {reporteData.detalleFormaPago?.length > 0 && (
                  <CAccordionItem itemKey="formasPago">
                    <CAccordionHeader>
                      <CIcon icon={cilChart} className="me-2" />
                      Detalle por Forma de Pago ({reporteData.detalleFormaPago.length})
                    </CAccordionHeader>
                    <CAccordionBody>
                      <CTable size="sm">
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>Forma de Pago</CTableHeaderCell>
                            <CTableHeaderCell className="text-end">Cantidad</CTableHeaderCell>
                            <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {reporteData.detalleFormaPago.map((forma, index) => (
                            <CTableRow key={index}>
                              <CTableDataCell>{forma.formaDePago || '-'}</CTableDataCell>
                              <CTableDataCell className="text-end">
                                {forma.cantidad || 0}
                              </CTableDataCell>
                              <CTableDataCell className="text-end">
                                <strong>{formatCurrency(forma.precio)}</strong>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    </CAccordionBody>
                  </CAccordionItem>
                )}

                {reporteData.detalleArticulo?.length > 0 && (
                  <CAccordionItem itemKey="articulos">
                    <CAccordionHeader>
                      <CIcon icon={cilChart} className="me-2" />
                      Detalle por Artículo ({reporteData.detalleArticulo.length})
                    </CAccordionHeader>
                    <CAccordionBody>
                      <CTable size="sm">
                        <CTableHead>
                          <CTableRow>
                            <CTableHeaderCell>Artículo</CTableHeaderCell>
                            <CTableHeaderCell className="text-end">Cantidad</CTableHeaderCell>
                            <CTableHeaderCell className="text-end">Subtotal</CTableHeaderCell>
                            <CTableHeaderCell className="text-end">IVA</CTableHeaderCell>
                            <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                          </CTableRow>
                        </CTableHead>
                        <CTableBody>
                          {reporteData.detalleArticulo.map((articulo, index) => (
                            <CTableRow key={index}>
                              <CTableDataCell>{articulo.descripcion || '-'}</CTableDataCell>
                              <CTableDataCell className="text-end">
                                {articulo.cantidad || 0}
                              </CTableDataCell>
                              <CTableDataCell className="text-end">
                                {formatCurrency(articulo.subtotal)}
                              </CTableDataCell>
                              <CTableDataCell className="text-end">
                                {formatCurrency(articulo.iva)}
                              </CTableDataCell>
                              <CTableDataCell className="text-end">
                                <strong>{formatCurrency(articulo.total)}</strong>
                              </CTableDataCell>
                            </CTableRow>
                          ))}
                        </CTableBody>
                      </CTable>
                    </CAccordionBody>
                  </CAccordionItem>
                )}
              </CAccordion>
            )}

          {/* Loading */}
          {loading && (
            <div className="text-center p-3">
              <CSpinner color="primary" />
              <div className="mt-2">Cargando facturas...</div>
            </div>
          )}

          {/* Tabla de facturas */}
          {!loading && (
            <CTable hover responsive striped>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell>Consecutivo</CTableHeaderCell>
                  <CTableHeaderCell>Fecha</CTableHeaderCell>
                  <CTableHeaderCell>Tercero</CTableHeaderCell>
                  <CTableHeaderCell>Identificación</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">Subtotal</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">Descuento</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">IVA</CTableHeaderCell>
                  <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                  <CTableHeaderCell>Estado</CTableHeaderCell>
                  <CTableHeaderCell>Acciones</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {filteredFacturas.length === 0 ? (
                  <CTableRow>
                    <CTableDataCell colSpan={10} className="text-center">
                      {facturas.length === 0
                        ? 'No hay facturas. Use el botón "Filtrar" para cargar datos.'
                        : 'No se encontraron facturas con el criterio de búsqueda'}
                    </CTableDataCell>
                  </CTableRow>
                ) : (
                  filteredFacturas.map((factura) => (
                    <CTableRow key={factura.id || factura.facturasCanastillaId}>
                      <CTableDataCell>
                        <strong>{factura.consecutivo || '-'}</strong>
                      </CTableDataCell>
                      <CTableDataCell>{formatDate(factura.fecha)}</CTableDataCell>
                      <CTableDataCell>{factura.terceroId?.nombre || '-'}</CTableDataCell>
                      <CTableDataCell>{factura.terceroId?.identificacion || '-'}</CTableDataCell>
                      <CTableDataCell className="text-end">
                        {formatCurrency(factura.subtotal)}
                      </CTableDataCell>
                      <CTableDataCell className="text-end">
                        {formatCurrency(factura.descuento)}
                      </CTableDataCell>
                      <CTableDataCell className="text-end">
                        {formatCurrency(factura.iva)}
                      </CTableDataCell>
                      <CTableDataCell className="text-end">
                        <strong>{formatCurrency(factura.total)}</strong>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color={factura.estado === 'Anulada' ? 'danger' : 'success'}>
                          {getEstadoDisplay(factura.estado)}
                        </CBadge>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CButtonGroup>
                          <CButton
                            color="info"
                            variant="outline"
                            size="sm"
                            onClick={() => openDetailModal(factura)}
                            title="Ver resumen"
                          >
                            <CIcon icon={cilSearch} />
                          </CButton>
                          <CButton
                            color="primary"
                            variant="outline"
                            size="sm"
                            onClick={() => verDetalle(factura)}
                            title="Ver detalle completo"
                          >
                            <CIcon icon={cilContact} />
                          </CButton>
                        </CButtonGroup>
                      </CTableDataCell>
                    </CTableRow>
                  ))
                )}
              </CTableBody>
            </CTable>
          )}
        </CCardBody>
      </CCard>

      {/* Modal de Detalle */}
      <CModal visible={showDetailModal} onClose={() => setShowDetailModal(false)} size="lg">
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

                {/* Mostrar canastillas si existen */}
                {selectedFactura.canastillas && selectedFactura.canastillas.length > 0 && (
                  <>
                    <hr />
                    <h5>Items de la Factura</h5>
                    <CTable size="sm">
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
