import { React, useState, useRef } from 'react'
import FiltrarInfoTurnos from '../../services/FiltrarInfoTurnos'
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
  CButtonGroup,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilCalendar,
  cilCloudDownload,
  cilSearch,
  cilClock,
  cilCalculator,
  cilSpreadsheet,
} from '@coreui/icons'
import Toast from '../toast/Toast'
import * as XLSX from 'xlsx'

var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

const cop = Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  minimumFractionDigits: 0,
})

const Turnos = () => {
  const navigate = useNavigate()
  const toastRef = useRef()

  // Estados
  const [fechaInicial, setFechaInicial] = useState('')
  const [fechaFinal, setFechaFinal] = useState('')
  const [turnos, setTurnos] = useState([])
  const [turnosDetalle, setTurnosDetalle] = useState([])
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
      const response = await FiltrarInfoTurnos(fechaInicial, fechaFinal)
      if (response === 'fail') {
        navigate('/Login', { replace: true })
        return
      }

      setTurnosDetalle(response || [])
      setTurnos([...new Set(response.map((item) => item.turno))])
      setShowResults(true)

      toastRef.current?.addMessage('Reporte de turnos generado exitosamente', 'success')
    } catch (error) {
      console.error('Error:', error)
      toastRef.current?.addMessage('Error al generar el reporte', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Función para descargar reporte PDF
  const descargarReporte = () => {
    if (!turnosDetalle.length) {
      toastRef.current?.addMessage('No hay datos para generar el reporte', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'

      const docDefinition = {
        pageSize: 'A4',
        pageOrientation: 'landscape',
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
            text: 'Reporte de Turnos',
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
          ...turnos
            .map((turno) => [
              {
                text: `Turno: ${turno}`,
                style: 'sectionHeader',
                margin: [0, 10, 0, 5],
              },
              {
                table: {
                  headerRows: 1,
                  widths: ['auto', 'auto', '*', 'auto', 'auto', 'auto', 'auto', 'auto'],
                  body: [
                    [
                      'Manguera',
                      'Surtidor',
                      'Combustible',
                      'Apertura',
                      'Cierre',
                      'Diferencia',
                      'Precio',
                      'Total',
                    ],
                    ...turnosDetalle
                      .filter((t) => t.turno === turno)
                      .map((t) => [
                        t.manguera || 'N/A',
                        t.surtidor || 'N/A',
                        (t.combustible || '').trim(),
                        t.apertura?.toFixed(3) || '0.000',
                        t.cierre?.toFixed(3) || '0.000',
                        t.diferencia?.toFixed(3) || '0.000',
                        cop.format(t.precio || 0),
                        cop.format(t.total || 0),
                      ]),
                  ],
                },
                layout: 'lightHorizontalLines',
                margin: [0, 0, 0, 10],
              },
              {
                text: `Total turno: ${cop.format(
                  turnosDetalle
                    .filter((t) => t.turno === turno)
                    .reduce((sum, t) => sum + (t.total || 0), 0),
                )} | Galones: ${turnosDetalle
                  .filter((t) => t.turno === turno)
                  .reduce((sum, t) => sum + (t.diferencia || 0), 0)
                  .toFixed(3)}`,
                style: 'total',
                margin: [0, 0, 0, 20],
              },
            ])
            .flat(),
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
          total: {
            fontSize: 12,
            bold: true,
            alignment: 'right',
          },
        },
        defaultStyle: {
          fontSize: 8,
        },
      }

      pdfMake
        .createPdf(docDefinition)
        .download(
          `turnos_${fechaInicial}_${fechaFinal}_${new Date().toISOString().split('T')[0]}.pdf`,
        )
      toastRef.current?.addMessage('Reporte descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error generating PDF:', error)
      toastRef.current?.addMessage('Error al generar el PDF', 'error')
    }
  }

  // Función para descargar reporte Excel
  const descargarReporteExcel = () => {
    if (!turnosDetalle.length) {
      toastRef.current?.addMessage('No hay datos para generar el reporte', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'

      // Crear nuevo libro de trabajo
      const workbook = XLSX.utils.book_new()

      // Hoja 1: Resumen General
      const resumenData = [
        ['REPORTE DE TURNOS'],
        [''],
        ['Estación:', estacionNombre],
        ['NIT:', estacionNit],
        ['Período:', `${fechaInicial} - ${fechaFinal}`],
        ['Fecha de generación:', new Date().toLocaleDateString('es-ES')],
        [''],
        ['RESUMEN ESTADÍSTICAS'],
        ['Total turnos:', turnos.length],
        ['Total mangueras:', turnosDetalle.length],
        [
          'Total galones:',
          turnosDetalle.reduce((sum, t) => sum + (t.diferencia || 0), 0).toFixed(3),
        ],
        ['Total monto:', turnosDetalle.reduce((sum, t) => sum + (t.total || 0), 0)],
      ]

      const resumenSheet = XLSX.utils.aoa_to_sheet(resumenData)
      resumenSheet['!cols'] = [{ width: 25 }, { width: 30 }]
      XLSX.utils.book_append_sheet(workbook, resumenSheet, 'Resumen General')

      // Hoja 2: Detalle Completo
      const detalleData = [
        [
          'Turno',
          'Manguera',
          'Surtidor',
          'Combustible',
          'Apertura',
          'Cierre',
          'Diferencia (Gal)',
          'Precio',
          'Total',
        ],
        ...turnosDetalle.map((detalle) => [
          detalle.turno || 'N/A',
          detalle.manguera || 'N/A',
          detalle.surtidor || 'N/A',
          (detalle.combustible || '').trim(),
          detalle.apertura?.toFixed(3) || '0.000',
          detalle.cierre?.toFixed(3) || '0.000',
          detalle.diferencia?.toFixed(3) || '0.000',
          detalle.precio || 0,
          detalle.total || 0,
        ]),
      ]

      const detalleSheet = XLSX.utils.aoa_to_sheet(detalleData)
      detalleSheet['!cols'] = [
        { width: 12 }, // Turno
        { width: 12 }, // Manguera
        { width: 12 }, // Surtidor
        { width: 18 }, // Combustible
        { width: 12 }, // Apertura
        { width: 12 }, // Cierre
        { width: 15 }, // Diferencia
        { width: 15 }, // Precio
        { width: 15 }, // Total
      ]
      XLSX.utils.book_append_sheet(workbook, detalleSheet, 'Detalle Completo')

      // Hoja 3: Resumen por Turno
      const turnosResumenData = [
        ['RESUMEN POR TURNO'],
        [''],
        ['Turno', 'Mangueras', 'Galones Total', 'Monto Total', 'Surtidores', 'Combustibles'],
      ]

      turnos.forEach((turno) => {
        const stats = calcularEstadisticasTurno(turno)
        turnosResumenData.push([
          turno,
          stats.totalMangueras,
          stats.totalGalones.toFixed(3),
          stats.totalMonto,
          stats.surtidores,
          stats.combustibles.join(', '),
        ])
      })

      const turnosResumenSheet = XLSX.utils.aoa_to_sheet(turnosResumenData)
      turnosResumenSheet['!cols'] = [
        { width: 15 }, // Turno
        { width: 15 }, // Mangueras
        { width: 15 }, // Galones
        { width: 18 }, // Monto
        { width: 12 }, // Surtidores
        { width: 30 }, // Combustibles
      ]
      XLSX.utils.book_append_sheet(workbook, turnosResumenSheet, 'Resumen por Turno')

      // Hoja 4: Por Combustible
      const combustibleData = [
        ['ANÁLISIS POR COMBUSTIBLE'],
        [''],
        ['Combustible', 'Mangueras', 'Galones Total', 'Monto Total', 'Precio Promedio'],
      ]

      const combustiblesGroup = turnosDetalle.reduce((acc, detalle) => {
        const combustible = (detalle.combustible || '').trim() || 'Sin Especificar'
        if (!acc[combustible]) {
          acc[combustible] = { mangueras: 0, galones: 0, monto: 0, precios: [] }
        }
        acc[combustible].mangueras += 1
        acc[combustible].galones += detalle.diferencia || 0
        acc[combustible].monto += detalle.total || 0
        if (detalle.precio) acc[combustible].precios.push(detalle.precio)
        return acc
      }, {})

      Object.entries(combustiblesGroup).forEach(([combustible, data]) => {
        const precioPromedio =
          data.precios.length > 0
            ? data.precios.reduce((sum, p) => sum + p, 0) / data.precios.length
            : 0
        combustibleData.push([
          combustible,
          data.mangueras,
          data.galones.toFixed(3),
          data.monto,
          precioPromedio.toFixed(2),
        ])
      })

      const combustibleSheet = XLSX.utils.aoa_to_sheet(combustibleData)
      combustibleSheet['!cols'] = [
        { width: 20 }, // Combustible
        { width: 15 }, // Mangueras
        { width: 15 }, // Galones
        { width: 18 }, // Monto
        { width: 18 }, // Precio Promedio
      ]
      XLSX.utils.book_append_sheet(workbook, combustibleSheet, 'Por Combustible')

      // Descargar archivo
      const fileName = `ReporteTurnos_${fechaInicial}_${fechaFinal}_${
        new Date().toISOString().split('T')[0]
      }.xlsx`
      XLSX.writeFile(workbook, fileName)

      toastRef.current?.addMessage('Reporte Excel descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error downloading Excel report:', error)
      toastRef.current?.addMessage('Error al descargar el reporte Excel', 'error')
    }
  }

  // Calcular estadísticas del turno
  const calcularEstadisticasTurno = (turno) => {
    const detallesTurno = turnosDetalle.filter((t) => t.turno === turno)
    return {
      totalMangueras: detallesTurno.length,
      totalMonto: detallesTurno.reduce((sum, t) => sum + (t.total || 0), 0),
      totalGalones: detallesTurno.reduce((sum, t) => sum + (t.diferencia || 0), 0),
      surtidores: [...new Set(detallesTurno.map((t) => t.surtidor).filter(Boolean))].length,
      combustibles: [
        ...new Set(detallesTurno.map((t) => (t.combustible || '').trim()).filter(Boolean)),
      ],
    }
  }

  return (
    <>
      <CCard className="mb-4">
        <CCardHeader>
          <h4 className="mb-0">
            <CIcon icon={cilClock} className="me-2" />
            Reporte de Turnos
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
        <CCard>
          <CCardHeader className="d-flex justify-content-between align-items-center">
            <h5 className="mb-0">Resultados del Reporte</h5>
            <CButtonGroup>
              <CButton
                color="danger"
                variant="outline"
                onClick={descargarReporte}
                disabled={!turnosDetalle.length}
              >
                <CIcon icon={cilCloudDownload} className="me-2" />
                PDF
              </CButton>
              <CButton
                color="success"
                variant="outline"
                onClick={descargarReporteExcel}
                disabled={!turnosDetalle.length}
              >
                <CIcon icon={cilSpreadsheet} className="me-2" />
                Excel
              </CButton>
            </CButtonGroup>
          </CCardHeader>
          <CCardBody>
            {!turnosDetalle.length ? (
              <CAlert color="warning">
                No se encontraron turnos para el período especificado.
              </CAlert>
            ) : (
              <CAccordion flush>
                {turnos.map((turno, index) => {
                  const stats = calcularEstadisticasTurno(turno)
                  return (
                    <CAccordionItem key={turno} itemKey={index}>
                      <CAccordionHeader>
                        <div className="d-flex justify-content-between align-items-center w-100 me-3">
                          <div className="d-flex align-items-center">
                            <CIcon icon={cilClock} className="me-2" />
                            <strong>Turno: {turno}</strong>
                          </div>
                          <div className="d-flex gap-3">
                            <CBadge color="info">
                              {stats.surtidores} surtidor{stats.surtidores !== 1 ? 'es' : ''}
                            </CBadge>
                            <CBadge color="primary">
                              {stats.totalMangueras} manguera{stats.totalMangueras !== 1 ? 's' : ''}
                            </CBadge>
                            <CBadge color="warning">{stats.totalGalones.toFixed(2)} Gal</CBadge>
                            <CBadge color="success">{cop.format(stats.totalMonto)}</CBadge>
                          </div>
                        </div>
                      </CAccordionHeader>
                      <CAccordionBody>
                        <CTable striped hover responsive>
                          <CTableHead>
                            <CTableRow>
                              <CTableHeaderCell>Manguera</CTableHeaderCell>
                              <CTableHeaderCell>Surtidor</CTableHeaderCell>
                              <CTableHeaderCell>Combustible</CTableHeaderCell>
                              <CTableHeaderCell className="text-end">Apertura</CTableHeaderCell>
                              <CTableHeaderCell className="text-end">Cierre</CTableHeaderCell>
                              <CTableHeaderCell className="text-end">
                                Diferencia (Gal)
                              </CTableHeaderCell>
                              <CTableHeaderCell className="text-end">Precio</CTableHeaderCell>
                              <CTableHeaderCell className="text-end">Total</CTableHeaderCell>
                            </CTableRow>
                          </CTableHead>
                          <CTableBody>
                            {turnosDetalle
                              .filter((t) => t.turno === turno)
                              .map((detalle, detailIndex) => (
                                <CTableRow key={`${turno}-${detailIndex}`}>
                                  <CTableDataCell>
                                    <CBadge color="secondary">{detalle.manguera || 'N/A'}</CBadge>
                                  </CTableDataCell>
                                  <CTableDataCell>
                                    <CBadge color="info">{detalle.surtidor || 'N/A'}</CBadge>
                                  </CTableDataCell>
                                  <CTableDataCell>
                                    <CBadge color="primary">
                                      {(detalle.combustible || '').trim()}
                                    </CBadge>
                                  </CTableDataCell>
                                  <CTableDataCell className="text-end">
                                    {detalle.apertura?.toFixed(3) || '0.000'}
                                  </CTableDataCell>
                                  <CTableDataCell className="text-end">
                                    {detalle.cierre?.toFixed(3) || '0.000'}
                                  </CTableDataCell>
                                  <CTableDataCell className="text-end fw-bold text-info">
                                    {detalle.diferencia?.toFixed(3) || '0.000'}
                                  </CTableDataCell>
                                  <CTableDataCell className="text-end">
                                    {cop.format(detalle.precio || 0)}
                                  </CTableDataCell>
                                  <CTableDataCell className="text-end fw-bold text-success">
                                    {cop.format(detalle.total || 0)}
                                  </CTableDataCell>
                                </CTableRow>
                              ))}
                          </CTableBody>
                        </CTable>

                        <div className="mt-3 p-3 bg-light rounded">
                          <CRow>
                            <CCol md={3}>
                              <strong>Mangueras: </strong>
                              <span className="text-primary">{stats.totalMangueras}</span>
                            </CCol>
                            <CCol md={3}>
                              <strong>Surtidores: </strong>
                              <span className="text-info">{stats.surtidores}</span>
                            </CCol>
                            <CCol md={3}>
                              <strong>Total Galones: </strong>
                              <span className="text-warning fw-bold">
                                {stats.totalGalones.toFixed(3)}
                              </span>
                            </CCol>
                            <CCol md={3}>
                              <strong>Total Monto: </strong>
                              <span className="text-success fw-bold">
                                {cop.format(stats.totalMonto)}
                              </span>
                            </CCol>
                          </CRow>
                          {stats.combustibles.length > 0 && (
                            <CRow className="mt-2">
                              <CCol>
                                <strong>Combustibles: </strong>
                                {stats.combustibles.map((combustible, idx) => (
                                  <CBadge key={idx} color="outline-primary" className="me-1">
                                    {combustible}
                                  </CBadge>
                                ))}
                              </CCol>
                            </CRow>
                          )}
                        </div>
                      </CAccordionBody>
                    </CAccordionItem>
                  )
                })}
              </CAccordion>
            )}
          </CCardBody>
        </CCard>
      )}

      <Toast ref={toastRef} />
    </>
  )
}

export default Turnos
