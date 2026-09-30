import { React, useState, useRef } from 'react'
import FiltrarInfoCanastilla from '../../services/FiltrarInfoCanastilla'
import { useNavigate } from 'react-router-dom'
import {
  CButton,
  CRow,
  CCol,
  CCard,
  CCardBody,
  CCardHeader,
  CTable,
  CTableBody,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CTableDataCell,
  CFormInput,
  CFormLabel,
  CSpinner,
  CBadge,
  CAlert,
  CAccordion,
  CAccordionItem,
  CAccordionHeader,
  CAccordionBody,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilCalendar,
  cilCloudDownload,
  cilSearch,
  cilBasket,
  cilCalculator,
  cilCreditCard,
} from '@coreui/icons'
import Toast from '../toast/Toast'

var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

const cop = Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
})

const Canastilla = () => {
  const navigate = useNavigate()
  const toastRef = useRef()

  // Estados
  const [fechaInicial, setFechaInicial] = useState('')
  const [fechaFinal, setFechaFinal] = useState('')
  const [facturas, setFacturas] = useState([])
  const [formas, setFormas] = useState([])
  const [articulos, setArticulos] = useState([])
  const [loading, setLoading] = useState(false)
  const [showResults, setShowResults] = useState(false)

  // Función para validar fecha
  const isValidDate = (dateString) => {
    return !isNaN(Date.parse(dateString))
  }

  // Manejador de búsqueda
  const handleSearch = async () => {
    if (!fechaInicial || !fechaFinal) {
      toastRef.current?.addMessage('Debe seleccionar las fechas inicial y final', 'error')
      return
    }

    setLoading(true)
    try {
      const response = await FiltrarInfoCanastilla(fechaInicial, fechaFinal)
      if (response === 'fail') {
        navigate('/Login', { replace: true })
        return
      }

      setFacturas(response.facturas || [])
      setFormas(response.detalleFormaPago || [])
      setArticulos(response.detalleArticulo || [])
      setShowResults(true)

      toastRef.current?.addMessage('Reporte de canastilla generado exitosamente', 'success')
    } catch (error) {
      console.error('Error:', error)
      toastRef.current?.addMessage('Error al generar el reporte', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Función para descargar reporte PDF
  const descargarReporte = () => {
    if (!facturas.length && !formas.length && !articulos.length) {
      toastRef.current?.addMessage('No hay datos para generar el reporte', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'

      const docDefinition = {
        content: [
          {
            text: estacionNombre,
            style: 'header',
            alignment: 'center',
          },
          {
            text: `NIT: ${estacionNit}`,
            style: 'subheader',
            alignment: 'center',
          },
          {
            text: 'Reporte de Canastilla',
            style: 'title',
            alignment: 'center',
            margin: [0, 20, 0, 10],
          },
          {
            text: `Período: ${fechaInicial} - ${fechaFinal}`,
            style: 'subheader',
            margin: [0, 0, 0, 20],
          },
        ],
        styles: {
          header: {
            fontSize: 18,
            bold: true,
          },
          subheader: {
            fontSize: 12,
            bold: true,
          },
          title: {
            fontSize: 16,
            bold: true,
          },
          tableHeader: {
            fontSize: 12,
            bold: true,
          },
          total: {
            fontSize: 12,
            bold: true,
            alignment: 'right',
          },
        },
      }

      // Agregar sección de facturas
      if (facturas.length > 0) {
        docDefinition.content.push(
          {
            text: 'Facturas',
            style: 'tableHeader',
            margin: [0, 10, 0, 5],
          },
          {
            table: {
              headerRows: 1,
              widths: ['auto', '*', 'auto', 'auto', 'auto'],
              body: [
                ['Consecutivo', 'Cliente', 'Fecha', 'Forma Pago', 'Total'],
                ...facturas.map((f) => [
                  f.consecutivo || 'N/A',
                  f.cliente || 'N/A',
                  f.fecha || 'N/A',
                  f.formaPago || 'N/A',
                  cop.format(f.total || 0),
                ]),
              ],
            },
            layout: 'lightHorizontalLines',
            margin: [0, 0, 0, 20],
          },
        )
      }

      // Agregar sección de formas de pago
      if (formas.length > 0) {
        docDefinition.content.push(
          {
            text: 'Resumen por Forma de Pago',
            style: 'tableHeader',
            margin: [0, 10, 0, 5],
          },
          {
            table: {
              headerRows: 1,
              widths: ['*', 'auto', 'auto'],
              body: [
                ['Forma de Pago', 'Cantidad', 'Total'],
                ...formas.map((f) => [
                  f.formaPago || 'N/A',
                  f.cantidad || '0',
                  cop.format(f.total || 0),
                ]),
              ],
            },
            layout: 'lightHorizontalLines',
            margin: [0, 0, 0, 20],
          },
        )
      }

      // Agregar sección de artículos
      if (articulos.length > 0) {
        docDefinition.content.push(
          {
            text: 'Resumen por Artículo',
            style: 'tableHeader',
            margin: [0, 10, 0, 5],
          },
          {
            table: {
              headerRows: 1,
              widths: ['*', 'auto', 'auto', 'auto'],
              body: [
                ['Artículo', 'Cantidad', 'Precio Unit.', 'Total'],
                ...articulos.map((a) => [
                  a.articulo || 'N/A',
                  a.cantidad || '0',
                  cop.format(a.precioUnitario || 0),
                  cop.format(a.total || 0),
                ]),
              ],
            },
            layout: 'lightHorizontalLines',
            margin: [0, 0, 0, 20],
          },
        )
      }

      pdfMake
        .createPdf(docDefinition)
        .download(
          `canastilla_${fechaInicial}_${fechaFinal}_${new Date().toISOString().split('T')[0]}.pdf`,
        )
      toastRef.current?.addMessage('Reporte descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error generating PDF:', error)
      toastRef.current?.addMessage('Error al generar el PDF', 'error')
    }
  }

  // Calcular totales
  const calcularTotales = () => {
    return {
      totalFacturas: facturas.reduce((sum, f) => sum + (f.total || 0), 0),
      totalFormas: formas.reduce((sum, f) => sum + (f.total || 0), 0),
      totalArticulos: articulos.reduce((sum, a) => sum + (a.total || 0), 0),
      cantidadFacturas: facturas.length,
    }
  }

  const totales = calcularTotales()

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>
          <h4 className="mb-0">
            <CIcon icon={cilBasket} className="me-2" />
            Reporte de Canastilla
          </h4>
        </CCardHeader>
        <CCardBody>
          <CRow>
            <CCol md={4}>
              <div className="mb-3">
                <CFormLabel htmlFor="fechaInicial">
                  <CIcon icon={cilCalendar} className="me-1" />
                  Fecha Inicial *
                </CFormLabel>
                <CFormInput
                  type="date"
                  id="fechaInicial"
                  value={fechaInicial}
                  onChange={(e) => setFechaInicial(e.target.value)}
                />
              </div>
            </CCol>
            <CCol md={4}>
              <div className="mb-3">
                <CFormLabel htmlFor="fechaFinal">
                  <CIcon icon={cilCalendar} className="me-1" />
                  Fecha Final *
                </CFormLabel>
                <CFormInput
                  type="date"
                  id="fechaFinal"
                  value={fechaFinal}
                  onChange={(e) => setFechaFinal(e.target.value)}
                />
              </div>
            </CCol>
            <CCol md={4}>
              <div className="mb-3">
                <CFormLabel>&nbsp;</CFormLabel>
                <div>
                  <CButton
                    color="primary"
                    onClick={handleSearch}
                    disabled={loading}
                    className="w-100"
                  >
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
                </div>
              </div>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {showResults && (
        <>
          {/* Tarjetas de resumen */}
          <CRow className="mb-4">
            <CCol md={3}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-primary">{totales.cantidadFacturas}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Facturas
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-success">
                    {cop.format(totales.totalFacturas)}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Ventas
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-info">{formas.length}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Formas de Pago
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={3}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-warning">{articulos.length}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Artículos Vendidos
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>

          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <h5 className="mb-0">Resultados del Reporte</h5>
              <CButton
                color="success"
                onClick={descargarReporte}
                disabled={!facturas.length && !formas.length && !articulos.length}
              >
                <CIcon icon={cilCloudDownload} className="me-2" />
                Descargar PDF
              </CButton>
            </CCardHeader>
            <CCardBody>
              {!facturas.length && !formas.length && !articulos.length ? (
                <CAlert color="warning">
                  No se encontraron datos para el período especificado.
                </CAlert>
              ) : (
                <CAccordion flush>
                  {/* Sección de Facturas */}
                  {facturas.length > 0 && (
                    <CAccordionItem itemKey="facturas">
                      <CAccordionHeader>
                        <div className="d-flex justify-content-between align-items-center w-100 me-3">
                          <div className="d-flex align-items-center">
                            <CIcon icon={cilBasket} className="me-2" />
                            <strong>Facturas ({facturas.length})</strong>
                          </div>
                          <CBadge color="success">{cop.format(totales.totalFacturas)}</CBadge>
                        </div>
                      </CAccordionHeader>
                      <CAccordionBody>
                        <CTable striped hover responsive>
                          <CTableHead>
                            <CTableRow>
                              <CTableHeaderCell>Consecutivo</CTableHeaderCell>
                              <CTableHeaderCell>Cliente</CTableHeaderCell>
                              <CTableHeaderCell>Fecha</CTableHeaderCell>
                              <CTableHeaderCell>Forma Pago</CTableHeaderCell>
                              <CTableHeaderCell>Total</CTableHeaderCell>
                            </CTableRow>
                          </CTableHead>
                          <CTableBody>
                            {facturas.map((factura, index) => (
                              <CTableRow key={index}>
                                <CTableDataCell>{factura.consecutivo || 'N/A'}</CTableDataCell>
                                <CTableDataCell>{factura.cliente || 'N/A'}</CTableDataCell>
                                <CTableDataCell>{factura.fecha || 'N/A'}</CTableDataCell>
                                <CTableDataCell>
                                  <CBadge color="info">{factura.formaPago || 'N/A'}</CBadge>
                                </CTableDataCell>
                                <CTableDataCell className="fw-bold text-success">
                                  {cop.format(factura.total || 0)}
                                </CTableDataCell>
                              </CTableRow>
                            ))}
                          </CTableBody>
                        </CTable>
                      </CAccordionBody>
                    </CAccordionItem>
                  )}

                  {/* Sección de Formas de Pago */}
                  {formas.length > 0 && (
                    <CAccordionItem itemKey="formas">
                      <CAccordionHeader>
                        <div className="d-flex justify-content-between align-items-center w-100 me-3">
                          <div className="d-flex align-items-center">
                            <CIcon icon={cilCreditCard} className="me-2" />
                            <strong>Resumen por Forma de Pago</strong>
                          </div>
                          <CBadge color="info">{formas.length} formas</CBadge>
                        </div>
                      </CAccordionHeader>
                      <CAccordionBody>
                        <CTable striped hover responsive>
                          <CTableHead>
                            <CTableRow>
                              <CTableHeaderCell>Forma de Pago</CTableHeaderCell>
                              <CTableHeaderCell>Cantidad</CTableHeaderCell>
                              <CTableHeaderCell>Total</CTableHeaderCell>
                            </CTableRow>
                          </CTableHead>
                          <CTableBody>
                            {formas.map((forma, index) => (
                              <CTableRow key={index}>
                                <CTableDataCell>
                                  <CBadge color="primary">{forma.formaPago || 'N/A'}</CBadge>
                                </CTableDataCell>
                                <CTableDataCell>{forma.cantidad || 0}</CTableDataCell>
                                <CTableDataCell className="fw-bold text-success">
                                  {cop.format(forma.total || 0)}
                                </CTableDataCell>
                              </CTableRow>
                            ))}
                          </CTableBody>
                        </CTable>
                      </CAccordionBody>
                    </CAccordionItem>
                  )}

                  {/* Sección de Artículos */}
                  {articulos.length > 0 && (
                    <CAccordionItem itemKey="articulos">
                      <CAccordionHeader>
                        <div className="d-flex justify-content-between align-items-center w-100 me-3">
                          <div className="d-flex align-items-center">
                            <CIcon icon={cilCalculator} className="me-2" />
                            <strong>Resumen por Artículo</strong>
                          </div>
                          <CBadge color="warning">{articulos.length} artículos</CBadge>
                        </div>
                      </CAccordionHeader>
                      <CAccordionBody>
                        <CTable striped hover responsive>
                          <CTableHead>
                            <CTableRow>
                              <CTableHeaderCell>Artículo</CTableHeaderCell>
                              <CTableHeaderCell>Cantidad</CTableHeaderCell>
                              <CTableHeaderCell>Precio Unitario</CTableHeaderCell>
                              <CTableHeaderCell>Total</CTableHeaderCell>
                            </CTableRow>
                          </CTableHead>
                          <CTableBody>
                            {articulos.map((articulo, index) => (
                              <CTableRow key={index}>
                                <CTableDataCell>{articulo.articulo || 'N/A'}</CTableDataCell>
                                <CTableDataCell>
                                  <CBadge color="primary">{articulo.cantidad || 0}</CBadge>
                                </CTableDataCell>
                                <CTableDataCell>
                                  {cop.format(articulo.precioUnitario || 0)}
                                </CTableDataCell>
                                <CTableDataCell className="fw-bold text-success">
                                  {cop.format(articulo.total || 0)}
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
            </CCardBody>
          </CCard>
        </>
      )}

      <Toast ref={toastRef} />
    </>
  )
}

export default Canastilla
