import { React, useState, useEffect, useRef } from 'react'
import ReporteFiscalCall from '../../services/ReporteFiscal'
import { Link, useNavigate } from 'react-router-dom'
import {
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
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CFormSelect,
  CCard,
  CCardBody,
  CCardHeader,
  CButtonGroup,
  CSpinner,
  CBadge,
  CAlert,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilCalendar,
  cilCloudDownload,
  cilSearch,
  cilChartPie,
  cilCalculator,
  cilSpreadsheet,
  cilCreditCard,
} from '@coreui/icons'
import Toast from '../toast/Toast'
import * as XLSX from 'xlsx'
var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

let cop = Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  minimumFractionDigits: 0,
})
const ReporteFiscal = () => {
  let navigate = useNavigate()
  const [fechaInicial, setFechaInicial] = useState('')
  const [fechaFinal, setFechaFinal] = useState('')
  const [reporteData, setReporteData] = useState(null)
  const [loading, setLoading] = useState(false)
  const [showResults, setShowResults] = useState(false)

  const toastRef = useRef()

  // Función para calcular total de ventas de órdenes
  const calcularTotalOrdenes = (consolidadosOrdenes) => {
    return consolidadosOrdenes?.reduce((total, item) => total + (item.total || 0), 0) || 0
  }

  // Función para calcular total de órdenes anuladas
  const calcularTotalOrdenesAnuladas = (consolidadoOrdenesAnuladas) => {
    return consolidadoOrdenesAnuladas?.reduce((total, item) => total + (item.total || 0), 0) || 0
  }

  // Función para calcular cantidad total despachada
  const calcularCantidadTotal = (consolidados) => {
    return consolidados?.reduce((total, item) => total + (item.cantidad || 0), 0) || 0
  }

  const FiltrarReporteFiscalHandler = async () => {
    if (!fechaInicial || !fechaFinal) {
      toastRef.current?.addMessage('Debe seleccionar las fechas inicial y final', 'error')
      return
    }

    setLoading(true)
    try {
      let response = await ReporteFiscalCall(fechaInicial, fechaFinal)
      if (response == 'fail') {
        navigate('/Login', { replace: true })
      } else {
        setReporteData(response)
        setShowResults(true)
        toastRef.current?.addMessage('Reporte fiscal generado exitosamente', 'success')
      }
    } catch (error) {
      console.error('Error:', error)
      toastRef.current?.addMessage('Error al generar el reporte fiscal', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Función para descargar reporte Excel
  const descargarReporteExcel = () => {
    if (!reporteData) {
      toastRef.current?.addMessage('No hay datos para generar el reporte', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'

      const totalOrdenes = calcularTotalOrdenes(reporteData.consolidadosOrdenes)
      const totalOrdenesAnuladas = calcularTotalOrdenesAnuladas(
        reporteData.consolidadoOrdenesAnuladas,
      )
      const cantidadTotal = calcularCantidadTotal(reporteData.consolidadosOrdenes)
      const cantidadTotalFacturadas = calcularCantidadTotal(reporteData.consolidadoOrdenesAnuladas)

      // Crear workbook
      const wb = XLSX.utils.book_new()

      // Hoja 1: Resumen General
      const resumenData = [
        ['REPORTE FISCAL'],
        [`Estación: ${estacionNombre}`],
        [`NIT: ${estacionNit}`],
        [`Período: ${fechaInicial} - ${fechaFinal}`],
        [''],
        ['RESUMEN GENERAL'],
        ['Concepto', 'Valor'],
        ['Total de Órdenes', reporteData.totalDeOrdenes || 0],
        ['Órdenes de despacho Facturadas', reporteData.totalOrdenesAnuladas || 0],
        ['Cantidad Total Despachada (Gal)', cantidadTotalFacturadas.toFixed(2)],
        ['Valor Total Ventas no Facturadas', totalOrdenes],
        ['Valor Total Facturadas', totalOrdenesAnuladas],
      ]

      const wsResumen = XLSX.utils.aoa_to_sheet(resumenData)
      wsResumen['!cols'] = [{ width: 30 }, { width: 20 }]
      XLSX.utils.book_append_sheet(wb, wsResumen, 'Resumen General')

      // Hoja 2: Consolidado por Combustible
      if (reporteData.consolidadosOrdenes && reporteData.consolidadosOrdenes.length > 0) {
        const combustibleData = [
          ['CONSOLIDADO POR COMBUSTIBLE'],
          [''],
          ['Combustible', 'Cantidad (Gal)', 'Total ($)'],
          ...reporteData.consolidadosOrdenes.map((item) => [
            (item.combustible || '').trim(),
            item.cantidad?.toFixed(2) || '0.00',
            item.total || 0,
          ]),
          [''],
          ['TOTAL', cantidadTotal.toFixed(2), totalOrdenes],
        ]

        const wsCombustible = XLSX.utils.aoa_to_sheet(combustibleData)
        wsCombustible['!cols'] = [{ width: 20 }, { width: 15 }, { width: 15 }]
        XLSX.utils.book_append_sheet(wb, wsCombustible, 'Por Combustible')
      }

      // Hoja 3: Órdenes Anuladas (si existen)
      if (
        reporteData.consolidadoOrdenesAnuladas &&
        reporteData.consolidadoOrdenesAnuladas.length > 0
      ) {
        const anuladasData = [
          ['ÓRDENES DE DESPACHO FACTURADAS'],
          [''],
          ['Combustible', 'Cantidad (Gal)', 'Total ($)'],
          ...reporteData.consolidadoOrdenesAnuladas.map((item) => [
            (item.combustible || '').trim(),
            item.cantidad?.toFixed(2) || '0.00',
            item.total || 0,
          ]),
          [''],
          [
            'TOTAL',
            calcularCantidadTotal(reporteData.consolidadoOrdenesAnuladas).toFixed(2),
            totalOrdenesAnuladas,
          ],
        ]

        const wsAnuladas = XLSX.utils.aoa_to_sheet(anuladasData)
        wsAnuladas['!cols'] = [{ width: 20 }, { width: 15 }, { width: 15 }]
        XLSX.utils.book_append_sheet(wb, wsAnuladas, 'Facturadas')
      }

      // Hoja 4: Consolidado Formas de Pago
      if (
        reporteData.consolidadoFormaPagoOrdenes &&
        reporteData.consolidadoFormaPagoOrdenes.length > 0
      ) {
        const formasPagoData = [
          ['CONSOLIDADO FORMAS DE PAGO DE ÓRDENES'],
          [''],
          ['Forma de Pago', 'Cantidad de Facturas', 'Cantidad de Combustible (L)', 'Total ($)'],
          ...reporteData.consolidadoFormaPagoOrdenes.map((item) => [
            item.formaPago || '',
            item.cantidadFacturas || 0,
            item.cantidadCombustible || 0,
            item.total || 0,
          ]),
          [''],
          [
            'TOTAL',
            reporteData.consolidadoFormaPagoOrdenes.reduce(
              (sum, item) => sum + (item.cantidadFacturas || 0),
              0,
            ),
            reporteData.consolidadoFormaPagoOrdenes.reduce(
              (sum, item) => sum + (item.cantidadCombustible || 0),
              0,
            ),
            reporteData.consolidadoFormaPagoOrdenes.reduce(
              (sum, item) => sum + (item.total || 0),
              0,
            ),
          ],
        ]

        const wsFormasPago = XLSX.utils.aoa_to_sheet(formasPagoData)
        wsFormasPago['!cols'] = [{ width: 25 }, { width: 18 }, { width: 22 }, { width: 15 }]
        XLSX.utils.book_append_sheet(wb, wsFormasPago, 'Formas de Pago')
      }

      // Descargar archivo
      const fileName = `reporte_fiscal_${fechaInicial}_${fechaFinal}_${
        new Date().toISOString().split('T')[0]
      }.xlsx`

      XLSX.writeFile(wb, fileName)
      toastRef.current?.addMessage('Reporte Excel descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error generating Excel:', error)
      toastRef.current?.addMessage('Error al generar el archivo Excel', 'error')
    }
  }

  // Función para descargar reporte PDF
  const descargarReporte = () => {
    if (!reporteData) {
      toastRef.current?.addMessage('No hay datos para generar el reporte', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'

      const totalOrdenes = calcularTotalOrdenes(reporteData.consolidadosOrdenes)
      const totalOrdenesAnuladas = calcularTotalOrdenesAnuladas(
        reporteData.consolidadoOrdenesAnuladas,
      )
      const cantidadTotalFacturadas = calcularCantidadTotal(reporteData.consolidadoOrdenesAnuladas)

      // Crear tabla de consolidados por combustible
      const tablaCombustibles = [
        ['Combustible', 'Cantidad (Gal)', 'Total ($)'],
        ...(reporteData.consolidadosOrdenes || []).map((item) => [
          (item.combustible || '').trim(),
          item.cantidad?.toFixed(2) || '0.00',
          cop.format(item.total || 0),
        ]),
      ]

      // Crear tabla de órdenes anuladas si existen
      const tablaAnuladas =
        reporteData.consolidadoOrdenesAnuladas?.length > 0
          ? [
              ['Combustible', 'Cantidad (Gal)', 'Total ($)'],
              ...(reporteData.consolidadoOrdenesAnuladas || []).map((item) => [
                (item.combustible || '').trim(),
                item.cantidad?.toFixed(2) || '0.00',
                cop.format(item.total || 0),
              ]),
            ]
          : null

      const docDefinition = {
        pageSize: 'A4',
        pageMargins: [40, 60, 40, 60],
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
            text: 'Reporte Fiscal',
            style: 'title',
            alignment: 'center',
            margin: [0, 20, 0, 10],
          },
          {
            text: `Período: ${fechaInicial} - ${fechaFinal}`,
            style: 'subheader',
            alignment: 'center',
            margin: [0, 0, 0, 20],
          },
          {
            text: 'Resumen General',
            style: 'sectionHeader',
            margin: [0, 10, 0, 5],
          },
          {
            table: {
              headerRows: 1,
              widths: ['*', 'auto'],
              body: [
                ['Concepto', 'Valor'],
                ['Total de Órdenes', reporteData.totalDeOrdenes || 0],
                ['Órdenes de despacho Facturadas', reporteData.totalOrdenesAnuladas || 0],
                ['Cantidad Total Despachada (Gal)', cantidadTotalFacturadas.toFixed(2)],
                ['Valor Total Ventas no Facturadas', cop.format(totalOrdenes)],
                ['Valor Total Facturadas', cop.format(totalOrdenesAnuladas)],
              ],
            },
            layout: 'lightHorizontalLines',
            margin: [0, 0, 0, 20],
          },
          {
            text: 'Consolidado por Combustible',
            style: 'sectionHeader',
            margin: [0, 20, 0, 5],
          },
          {
            table: {
              headerRows: 1,
              widths: ['*', 'auto', 'auto'],
              body: tablaCombustibles,
            },
            layout: 'lightHorizontalLines',
            margin: [0, 0, 0, 20],
          },
          ...(tablaAnuladas
            ? [
                {
                  text: 'Órdenes de despacho Facturadas por Combustible',
                  style: 'sectionHeader',
                  margin: [0, 20, 0, 5],
                },
                {
                  table: {
                    headerRows: 1,
                    widths: ['*', 'auto', 'auto'],
                    body: tablaAnuladas,
                  },
                  layout: 'lightHorizontalLines',
                  margin: [0, 0, 0, 20],
                },
              ]
            : []),
          // Sección de Consolidado por Forma de Pago de Órdenes
          ...(reporteData.consolidadoFormaPagoOrdenes?.length > 0
            ? [
                {
                  text: 'Consolidado por Forma de Pago de Órdenes',
                  style: 'sectionHeader',
                  margin: [0, 20, 0, 5],
                },
                {
                  table: {
                    headerRows: 1,
                    widths: ['*', 'auto', 'auto', 'auto'],
                    body: [
                      ['Forma de Pago', 'Cant. Facturas', 'Cant. Combustible (L)', 'Total ($)'],
                      ...(reporteData.consolidadoFormaPagoOrdenes || []).map((item) => [
                        item.formaPago || '',
                        item.cantidadFacturas || 0,
                        (item.cantidadCombustible || 0).toLocaleString('es-CO', {
                          minimumFractionDigits: 2,
                          maximumFractionDigits: 2,
                        }),
                        cop.format(item.total || 0),
                      ]),
                      [
                        { text: 'TOTALES', bold: true },
                        {
                          text: reporteData.consolidadoFormaPagoOrdenes.reduce(
                            (sum, item) => sum + (item.cantidadFacturas || 0),
                            0,
                          ),
                          bold: true,
                        },
                        {
                          text: reporteData.consolidadoFormaPagoOrdenes
                            .reduce((sum, item) => sum + (item.cantidadCombustible || 0), 0)
                            .toLocaleString('es-CO', {
                              minimumFractionDigits: 2,
                              maximumFractionDigits: 2,
                            }),
                          bold: true,
                        },
                        {
                          text: cop.format(
                            reporteData.consolidadoFormaPagoOrdenes.reduce(
                              (sum, item) => sum + (item.total || 0),
                              0,
                            ),
                          ),
                          bold: true,
                        },
                      ],
                    ],
                  },
                  layout: 'lightHorizontalLines',
                  margin: [0, 0, 0, 20],
                },
              ]
            : []),
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
          sectionHeader: {
            fontSize: 12,
            bold: true,
            fillColor: '#f0f0f0',
            margin: [0, 5, 0, 5],
          },
        },
      }

      pdfMake
        .createPdf(docDefinition)
        .download(
          `reporte_fiscal_${fechaInicial}_${fechaFinal}_${
            new Date().toISOString().split('T')[0]
          }.pdf`,
        )
      toastRef.current?.addMessage('Reporte PDF descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error generating PDF:', error)
      toastRef.current?.addMessage('Error al generar el PDF', 'error')
    }
  }

  const handlefechaInicialSelectedChange = (event) => {
    const selectedDate = event.target.value

    if (isValidDate(selectedDate)) {
      setFechaInicial(selectedDate)
    }
  }
  const handlefechaFinalSelectedChange = (event) => {
    const selectedDate = event.target.value

    if (isValidDate(selectedDate)) {
      setFechaFinal(selectedDate)
    }
  }

  // Función para verificar si la fecha es válida
  const isValidDate = (date) => {
    const pattern = /^\d{4}-\d{2}-\d{2}$/
    return pattern.test(date)
  }

  return (
    <>
      <Toast ref={toastRef}></Toast>

      <CCard className="mb-4">
        <CCardHeader>
          <h4 className="mb-0">
            <CIcon icon={cilChartPie} className="me-2" />
            Reporte Fiscal
          </h4>
        </CCardHeader>
        <CCardBody>
          <CRow>
            <CCol md={4}>
              <div className="mb-3">
                <label className="form-label">
                  <CIcon icon={cilCalendar} className="me-1" />
                  Fecha Inicial *
                </label>
                <CFormInput
                  type="date"
                  value={fechaInicial}
                  onChange={handlefechaInicialSelectedChange}
                />
              </div>
            </CCol>
            <CCol md={4}>
              <div className="mb-3">
                <label className="form-label">
                  <CIcon icon={cilCalendar} className="me-1" />
                  Fecha Final *
                </label>
                <CFormInput
                  type="date"
                  value={fechaFinal}
                  onChange={handlefechaFinalSelectedChange}
                />
              </div>
            </CCol>
            <CCol md={4}>
              <div className="mb-3">
                <label className="form-label">&nbsp;</label>
                <div>
                  <CButton
                    color="primary"
                    onClick={FiltrarReporteFiscalHandler}
                    disabled={loading}
                    className="w-100"
                  >
                    {loading ? (
                      <>
                        <CSpinner size="sm" className="me-2" />
                        Generando...
                      </>
                    ) : (
                      <>
                        <CIcon icon={cilSearch} className="me-2" />
                        Generar Reporte
                      </>
                    )}
                  </CButton>
                </div>
              </div>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {showResults && reporteData && (
        <>
          {/* Tarjetas de resumen fiscal */}
          <CRow className="mb-4">
            <CCol md={4}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-success">
                    {cop.format(calcularTotalOrdenes(reporteData.consolidadosOrdenes))}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Ventas no Facturadas
                  </div>
                  <div className="small text-muted">
                    {reporteData.totalDeOrdenes || 0} órdenes de despacho
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={4}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-info">
                    {calcularCantidadTotal(reporteData.consolidadosOrdenes).toFixed(2)}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Galones Vendidos
                  </div>
                  <div className="small text-muted">Total despachado</div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={4}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-warning">
                    {cop.format(
                      calcularTotalOrdenesAnuladas(reporteData.consolidadoOrdenesAnuladas),
                    )}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Ventas Facturadas
                  </div>
                  <div className="small text-muted">
                    {reporteData.totalOrdenesAnuladas || 0} órdenes de despacho
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>

          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <h5 className="mb-0">
                <CIcon icon={cilCalculator} className="me-2" />
                Detalle Fiscal
              </h5>
              <CButtonGroup>
                <CButton color="success" onClick={descargarReporte}>
                  <CIcon icon={cilCloudDownload} className="me-2" />
                  Descargar PDF
                </CButton>
                <CButton color="info" onClick={descargarReporteExcel}>
                  <CIcon icon={cilSpreadsheet} className="me-2" />
                  Descargar Excel
                </CButton>
              </CButtonGroup>
            </CCardHeader>
            <CCardBody>
              {/* Resumen general */}
              <CTable striped hover responsive>
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>Concepto</CTableHeaderCell>
                    <CTableHeaderCell className="text-end">Valor</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  <CTableRow>
                    <CTableDataCell>
                      <strong>Total de Órdenes de despacho</strong>
                    </CTableDataCell>
                    <CTableDataCell className="text-end fw-bold">
                      {reporteData.totalDeOrdenes || 0}
                    </CTableDataCell>
                  </CTableRow>
                  <CTableRow>
                    <CTableDataCell>
                      <strong>Órdenes de despacho Facturadas</strong>
                    </CTableDataCell>
                    <CTableDataCell className="text-end fw-bold text-danger">
                      {reporteData.totalOrdenesAnuladas || 0}
                    </CTableDataCell>
                  </CTableRow>
                  <CTableRow>
                    <CTableDataCell>
                      <strong>Total Ventas no Facturadas</strong>
                    </CTableDataCell>
                    <CTableDataCell className="text-end fw-bold text-success">
                      {cop.format(calcularTotalOrdenes(reporteData.consolidadosOrdenes))}
                    </CTableDataCell>
                  </CTableRow>
                  <CTableRow>
                    <CTableDataCell>
                      <strong>Total Galones Despachados</strong>
                    </CTableDataCell>
                    <CTableDataCell className="text-end fw-bold text-info">
                      {calcularCantidadTotal(reporteData.consolidadoOrdenesAnuladas).toFixed(2)} Gal
                    </CTableDataCell>
                  </CTableRow>
                  <CTableRow className="table-active">
                    <CTableDataCell>
                      <strong>VALOR TOTAL FACTURADO</strong>
                    </CTableDataCell>
                    <CTableDataCell className="text-end fw-bold text-warning fs-5">
                      {cop.format(
                        calcularTotalOrdenesAnuladas(reporteData.consolidadoOrdenesAnuladas),
                      )}
                    </CTableDataCell>
                  </CTableRow>
                </CTableBody>
              </CTable>

              {/* Detalle por combustible */}
              {reporteData.consolidadosOrdenes && reporteData.consolidadosOrdenes.length > 0 && (
                <div className="mt-4">
                  <h6>
                    <CIcon icon={cilCalculator} className="me-2" />
                    Ventas por Combustible
                  </h6>
                  <CTable striped hover responsive>
                    <CTableHead>
                      <CTableRow>
                        <CTableHeaderCell>Combustible</CTableHeaderCell>
                        <CTableHeaderCell className="text-end">Cantidad (Gal)</CTableHeaderCell>
                        <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {reporteData.consolidadosOrdenes.map((item, index) => (
                        <CTableRow key={index}>
                          <CTableDataCell>
                            <CBadge color="info">{(item.combustible || '').trim()}</CBadge>
                          </CTableDataCell>
                          <CTableDataCell className="text-end">
                            {item.cantidad?.toFixed(2) || '0.00'}
                          </CTableDataCell>
                          <CTableDataCell className="text-end fw-bold text-success">
                            {cop.format(item.total || 0)}
                          </CTableDataCell>
                        </CTableRow>
                      ))}
                    </CTableBody>
                  </CTable>
                </div>
              )}

              {/* Órdenes facturadas */}
              {reporteData.consolidadoOrdenesAnuladas &&
                reporteData.consolidadoOrdenesAnuladas.length > 0 && (
                  <div className="mt-4">
                    <h6>
                      <CIcon icon={cilCalculator} className="me-2" />
                      Órdenes de despacho Facturadas por Combustible
                    </h6>
                    <CAlert color="warning" className="mb-2">
                      <small>
                        Total Facturadas: {reporteData.totalOrdenesAnuladas} órdenes de despacho
                      </small>
                    </CAlert>
                    <CTable striped hover responsive>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>Combustible</CTableHeaderCell>
                          <CTableHeaderCell className="text-end">Cantidad (Gal)</CTableHeaderCell>
                          <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {reporteData.consolidadoOrdenesAnuladas.map((item, index) => (
                          <CTableRow key={index}>
                            <CTableDataCell>
                              <CBadge color="danger">{(item.combustible || '').trim()}</CBadge>
                            </CTableDataCell>
                            <CTableDataCell className="text-end">
                              {item.cantidad?.toFixed(2) || '0.00'}
                            </CTableDataCell>
                            <CTableDataCell className="text-end fw-bold text-danger">
                              {cop.format(item.total || 0)}
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                      </CTableBody>
                    </CTable>
                  </div>
                )}

              {/* Consolidado Formas de Pago */}
              {reporteData.consolidadoFormaPagoOrdenes &&
                reporteData.consolidadoFormaPagoOrdenes.length > 0 && (
                  <div className="mt-4">
                    <h6>
                      <CIcon icon={cilCreditCard} className="me-2" />
                      Consolidado Formas de Pago de Órdenes
                    </h6>
                    <CAlert color="info" className="mb-2">
                      <small>
                        Total de Formas de Pago: {reporteData.consolidadoFormaPagoOrdenes.length}{' '}
                        métodos
                      </small>
                    </CAlert>
                    <CTable striped hover responsive>
                      <CTableHead>
                        <CTableRow>
                          <CTableHeaderCell>Forma de Pago</CTableHeaderCell>
                          <CTableHeaderCell className="text-end">
                            Cantidad de Facturas
                          </CTableHeaderCell>
                          <CTableHeaderCell className="text-end">
                            Cantidad de Combustible (L)
                          </CTableHeaderCell>
                          <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                        </CTableRow>
                      </CTableHead>
                      <CTableBody>
                        {reporteData.consolidadoFormaPagoOrdenes.map((item, index) => (
                          <CTableRow key={index}>
                            <CTableDataCell>
                              <CBadge color="primary">{item.formaPago || 'No especificado'}</CBadge>
                            </CTableDataCell>
                            <CTableDataCell className="text-end">
                              {item.cantidadFacturas || 0}
                            </CTableDataCell>
                            <CTableDataCell className="text-end">
                              {(item.cantidadCombustible || 0).toLocaleString('es-CO', {
                                minimumFractionDigits: 2,
                                maximumFractionDigits: 2,
                              })}
                            </CTableDataCell>
                            <CTableDataCell className="text-end fw-bold text-primary">
                              {cop.format(item.total || 0)}
                            </CTableDataCell>
                          </CTableRow>
                        ))}
                        <CTableRow className="table-active">
                          <CTableDataCell>
                            <strong>TOTALES</strong>
                          </CTableDataCell>
                          <CTableDataCell className="text-end fw-bold">
                            {reporteData.consolidadoFormaPagoOrdenes.reduce(
                              (sum, item) => sum + (item.cantidadFacturas || 0),
                              0,
                            )}
                          </CTableDataCell>
                          <CTableDataCell className="text-end fw-bold">
                            {reporteData.consolidadoFormaPagoOrdenes
                              .reduce((sum, item) => sum + (item.cantidadCombustible || 0), 0)
                              .toLocaleString('es-CO', {
                                minimumFractionDigits: 2,
                                maximumFractionDigits: 2,
                              })}
                          </CTableDataCell>
                          <CTableDataCell className="text-end fw-bold text-primary fs-6">
                            {cop.format(
                              reporteData.consolidadoFormaPagoOrdenes.reduce(
                                (sum, item) => sum + (item.total || 0),
                                0,
                              ),
                            )}
                          </CTableDataCell>
                        </CTableRow>
                      </CTableBody>
                    </CTable>
                  </div>
                )}
            </CCardBody>
          </CCard>
        </>
      )}

      {showResults && !reporteData && (
        <CCard>
          <CCardBody>
            <CAlert color="warning">
              No se encontraron datos fiscales para el período especificado.
            </CAlert>
          </CCardBody>
        </CCard>
      )}
    </>
  )
}

export default ReporteFiscal
