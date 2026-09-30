import { React, useState, useEffect, useRef } from 'react'
import EstacionesDropdown from '../estaciones/EstacionesDropdown'
import GetCuposPorAutomotores from '../../services/GetCuposPorAutomotores'
import { Link, useNavigate } from 'react-router-dom'
import { Page, Text, View, Document, StyleSheet } from '@react-pdf/renderer'
import CIcon from '@coreui/icons-react'
import {
  cilPlus,
  cilPencil,
  cilX,
  cilCloudDownload,
  cilSpreadsheet,
  cilTruck,
  cilSearch,
  cilReload,
  cilInfo,
  cilMoney,
  cilUser,
} from '@coreui/icons'
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
  CButtonGroup,
  CCard,
  CCardBody,
  CCardHeader,
  CSpinner,
  CBadge,
  CAlert,
  CInputGroup,
  CInputGroupText,
  CPagination,
  CPaginationItem,
  CProgress,
  CTooltip,
} from '@coreui/react'
import Toast from '../toast/Toast'
import * as XLSX from 'xlsx'

var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

let cop = Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
})
const PorAutomotores = () => {
  let navigate = useNavigate()
  const [cupos, setCupos] = useState([])
  const [filteredCupos, setFilteredCupos] = useState([])
  const [totalCupos, setTotalCupos] = useState(0)
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const [itemsPerPage] = useState(10)

  const toastRef = useRef()

  // Calcular estadísticas
  const statistics = {
    totalAutomotores: filteredCupos.length,
    totalCupoAsignado: filteredCupos.reduce((sum, cupo) => sum + (cupo.cupoAsignado || 0), 0),
    totalCupoDisponible: filteredCupos.reduce((sum, cupo) => sum + (cupo.cupoDisponible || 0), 0),
    totalDebe: filteredCupos.reduce(
      (sum, cupo) => sum + ((cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0)),
      0,
    ),
    porcentajeUtilizado:
      filteredCupos.reduce((sum, cupo) => sum + (cupo.cupoAsignado || 0), 0) > 0
        ? ((filteredCupos.reduce((sum, cupo) => sum + (cupo.cupoAsignado || 0), 0) -
            filteredCupos.reduce((sum, cupo) => sum + (cupo.cupoDisponible || 0), 0)) /
            filteredCupos.reduce((sum, cupo) => sum + (cupo.cupoAsignado || 0), 0)) *
          100
        : 0,
  }

  const fetchCupos = async () => {
    try {
      setLoading(true)
      const estacionGuid = localStorage.getItem('estacionGuid')
      if (!estacionGuid) {
        navigate('/Login', { replace: true })
        return
      }

      const response = await GetCuposPorAutomotores(estacionGuid)
      if (response === 'fail') {
        navigate('/Login', { replace: true })
      } else {
        // El nuevo servicio retorna un array de objetos con información de automotores
        if (Array.isArray(response)) {
          setCupos(response)
          setFilteredCupos(response)
          setTotalCupos(response.length)
          toastRef.current?.addMessage(
            `Se cargaron ${response.length} automotores exitosamente`,
            'success',
          )
        } else {
          // Fallback para compatibilidad
          setCupos([])
          setFilteredCupos([])
          setTotalCupos(0)
          toastRef.current?.addMessage('No se encontraron datos de automotores', 'warning')
        }
      }
    } catch (error) {
      console.error('Error fetching cupos automotores:', error)
      toastRef.current?.addMessage('Error al cargar los datos de automotores', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Función de búsqueda
  const handleSearch = (term) => {
    setSearchTerm(term)
    setCurrentPage(1)

    if (!term.trim()) {
      setFilteredCupos(cupos)
      return
    }

    const filtered = cupos.filter(
      (cupo) =>
        cupo.cliente?.toLowerCase().includes(term.toLowerCase()) ||
        cupo.coD_CLI?.toLowerCase().includes(term.toLowerCase()) ||
        cupo.nit?.toLowerCase().includes(term.toLowerCase()) ||
        cupo.placa?.toLowerCase().includes(term.toLowerCase()),
    )

    setFilteredCupos(filtered)
  }

  // Función para obtener el color del badge según el porcentaje de uso del cupo
  const getCupoStatusColor = (cupoAsignado, cupoDisponible) => {
    if (!cupoAsignado || cupoAsignado === 0) return 'secondary'
    const porcentajeUtilizado = ((cupoAsignado - cupoDisponible) / cupoAsignado) * 100

    if (porcentajeUtilizado >= 90) return 'danger'
    if (porcentajeUtilizado >= 70) return 'warning'
    if (porcentajeUtilizado >= 50) return 'info'
    return 'success'
  }

  // Función para obtener el texto del estado del cupo
  const getCupoStatusText = (cupoAsignado, cupoDisponible) => {
    if (!cupoAsignado || cupoAsignado === 0) return 'Sin Cupo'
    const porcentajeUtilizado = ((cupoAsignado - cupoDisponible) / cupoAsignado) * 100

    if (porcentajeUtilizado >= 90) return 'Crítico'
    if (porcentajeUtilizado >= 70) return 'Alerta'
    if (porcentajeUtilizado >= 50) return 'Moderado'
    return 'Disponible'
  }

  // Paginación
  const indexOfLastItem = currentPage * itemsPerPage
  const indexOfFirstItem = indexOfLastItem - itemsPerPage
  const currentItems = filteredCupos.slice(indexOfFirstItem, indexOfLastItem)
  const totalPages = Math.ceil(filteredCupos.length / itemsPerPage)

  const descargarReporte = async (event) => {
    if (!filteredCupos.length) {
      toastRef.current?.addMessage('No hay datos para generar el reporte PDF', 'error')
      return
    }

    try {
      var today = new Date()
      var dd = String(today.getDate()).padStart(2, '0')
      var mm = String(today.getMonth() + 1).padStart(2, '0') //January is 0!
      var yyyy = today.getFullYear()

      today = dd + '/' + mm + '/' + yyyy
      let estacionNombre = localStorage.getItem('estacionNombre')
      let estacionNit = localStorage.getItem('estacionNit')

      let cuposTabla = [
        ['Cliente', 'Código', 'Nit/CC', 'Placa', 'Cupo asignado', 'Cupo disponible', 'Debe'],
      ]
      let cuposTableElements = filteredCupos.map((cupo) => [
        cupo.cliente || '',
        cupo.coD_CLI || '',
        cupo.nit || '',
        cupo.placa || '',
        { text: cop.format(cupo.cupoAsignado || 0), style: 'tableRight' },
        { text: cop.format(cupo.cupoDisponible || 0), style: 'tableRight' },
        {
          text: cop.format((cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0)),
          style: 'tableRight',
        },
      ])
      cuposTabla = cuposTabla.concat(cuposTableElements)

      var dd = {
        watermark: {
          text: 'SIGES Soluciones',
          color: 'blue',
          opacity: 0.1,
          bold: true,
          italics: false,
        },
        footer: function (currentPage, pageCount) {
          return (
            'SIGES Soluciones Reportes - ' +
            estacionNombre +
            ' - ' +
            currentPage.toString() +
            ' of ' +
            pageCount
          )
        },

        content: [
          {
            stack: [estacionNombre, { text: 'Nit ' + estacionNit, style: 'subheader' }],
            style: 'header',
          },
          {
            text: 'Reporte de Cupos de Automotores',
            style: 'header',
          },
          {
            text: `Generado el: ${today} | Total de registros: ${filteredCupos.length}`,
            style: 'normal',
            margin: [0, 10, 0, 20],
          },
          {
            style: 'tableExample',
            table: {
              headerRows: 1,
              body: cuposTabla,
            },
          },
        ],

        styles: {
          header: {
            fontSize: 18,
            bold: true,
            alignment: 'center',
          },
          subheader: {
            fontSize: 14,
          },
          normal: {
            fontSize: 10,
            alignment: 'center',
          },
          superMargin: {
            fontSize: 15,
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

      pdfMake
        .createPdf(dd)
        .download('ReporteCuposAutomotores - ' + estacionNombre + ' - ' + today + '.pdf')

      toastRef.current?.addMessage('Reporte PDF descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error generating PDF:', error)
      toastRef.current?.addMessage('Error al generar el reporte PDF', 'error')
    }
  }

  const descargarReporteExcel = () => {
    if (!filteredCupos.length) {
      toastRef.current?.addMessage('No hay datos para exportar', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'
      const today = new Date().toLocaleDateString('es-ES')

      // Crear nuevo libro de trabajo
      const workbook = XLSX.utils.book_new()

      // Hoja 1: Resumen General
      const resumenData = [
        ['REPORTE DE CUPOS POR AUTOMOTORES'],
        [''],
        ['Estación:', estacionNombre],
        ['NIT:', estacionNit],
        ['Fecha de generación:', today],
        ['Total de automotores:', filteredCupos.length],
        [''],
        ['RESUMEN FINANCIERO'],
        ['Total cupos asignados:', statistics.totalCupoAsignado],
        ['Total cupos disponibles:', statistics.totalCupoDisponible],
        ['Total debe:', statistics.totalDebe],
        ['Porcentaje utilizado:', `${statistics.porcentajeUtilizado.toFixed(2)}%`],
      ]

      const resumenSheet = XLSX.utils.aoa_to_sheet(resumenData)
      resumenSheet['!cols'] = [{ width: 25 }, { width: 30 }]
      XLSX.utils.book_append_sheet(workbook, resumenSheet, 'Resumen General')

      // Hoja 2: Detalle de Automotores
      const automotoresData = [
        ['Cliente', 'Código', 'Placa', 'Cupo Asignado', 'Cupo Disponible', 'Debe', 'Estado'],
        ...filteredCupos.map((cupo) => [
          cupo.cliente || '',
          cupo.coD_CLI || '',
          cupo.placa || '',
          cupo.cupoAsignado || 0,
          cupo.cupoDisponible || 0,
          (cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0),
          getCupoStatusText(cupo.cupoAsignado, cupo.cupoDisponible),
        ]),
      ]

      const automotoresSheet = XLSX.utils.aoa_to_sheet(automotoresData)
      automotoresSheet['!cols'] = [
        { width: 30 }, // Cliente
        { width: 15 }, // Código
        { width: 15 }, // Placa
        { width: 18 }, // Cupo Asignado
        { width: 18 }, // Cupo Disponible
        { width: 15 }, // Debe
        { width: 15 }, // Estado
      ]

      XLSX.utils.book_append_sheet(workbook, automotoresSheet, 'Detalle Automotores')

      // Hoja 3: Por Cliente
      const clientesGroupData = [
        ['RESUMEN POR CLIENTE'],
        [''],
        [
          'Cliente',
          'Cantidad Automotores',
          'Cupo Total Asignado',
          'Cupo Total Disponible',
          'Debe Total',
        ],
      ]

      const clientesGroup = filteredCupos.reduce((acc, cupo) => {
        const cliente = cupo.cliente || 'Sin Cliente'
        if (!acc[cliente]) {
          acc[cliente] = { cantidad: 0, asignado: 0, disponible: 0 }
        }
        acc[cliente].cantidad += 1
        acc[cliente].asignado += cupo.cupoAsignado || 0
        acc[cliente].disponible += cupo.cupoDisponible || 0
        return acc
      }, {})

      Object.entries(clientesGroup).forEach(([cliente, data]) => {
        clientesGroupData.push([
          cliente,
          data.cantidad,
          data.asignado,
          data.disponible,
          data.asignado - data.disponible,
        ])
      })

      const clientesGroupSheet = XLSX.utils.aoa_to_sheet(clientesGroupData)
      clientesGroupSheet['!cols'] = [
        { width: 30 }, // Cliente
        { width: 20 }, // Cantidad
        { width: 20 }, // Asignado
        { width: 20 }, // Disponible
        { width: 15 }, // Debe
      ]
      XLSX.utils.book_append_sheet(workbook, clientesGroupSheet, 'Por Cliente')

      // Descargar archivo
      const fileName = `ReporteCuposAutomotores_${estacionNombre.replace(/\s+/g, '_')}_${
        new Date().toISOString().split('T')[0]
      }.xlsx`
      XLSX.writeFile(workbook, fileName)

      toastRef.current?.addMessage('Reporte Excel descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error downloading Excel report:', error)
      toastRef.current?.addMessage('Error al descargar el reporte Excel', 'error')
    }
  }

  useEffect(() => {
    fetchCupos()
  }, [])

  return (
    <>
      <Toast ref={toastRef}></Toast>

      {/* Header Card */}
      <CCard className="mb-4">
        <CCardHeader>
          <h4 className="mb-0">
            <CIcon icon={cilTruck} className="me-2" />
            Reporte de Cupos por Automotores
          </h4>
        </CCardHeader>
        <CCardBody>
          <CRow className="align-items-end">
            <CCol md={6}>
              <div className="mb-3">
                <label className="form-label">
                  <CIcon icon={cilSearch} className="me-1" />
                  Buscar Automotor
                </label>
                <CInputGroup>
                  <CInputGroupText>
                    <CIcon icon={cilSearch} />
                  </CInputGroupText>
                  <CFormInput
                    type="text"
                    placeholder="Buscar por cliente, código, placa o NIT..."
                    value={searchTerm}
                    onChange={(e) => handleSearch(e.target.value)}
                  />
                </CInputGroup>
              </div>
            </CCol>
            <CCol md={6} className="text-end">
              <CButtonGroup className="mb-3">
                <CTooltip content="Actualizar datos">
                  <CButton color="info" variant="outline" onClick={fetchCupos} disabled={loading}>
                    {loading ? <CSpinner size="sm" /> : <CIcon icon={cilReload} className="me-1" />}
                    Actualizar
                  </CButton>
                </CTooltip>
                <CTooltip content="Descargar reporte en PDF">
                  <CButton
                    color="danger"
                    variant="outline"
                    onClick={descargarReporte}
                    disabled={loading || !filteredCupos.length}
                  >
                    <CIcon icon={cilCloudDownload} className="me-2" />
                    PDF
                  </CButton>
                </CTooltip>
                <CTooltip content="Descargar reporte en Excel">
                  <CButton
                    color="success"
                    variant="outline"
                    onClick={descargarReporteExcel}
                    disabled={loading || !filteredCupos.length}
                  >
                    <CIcon icon={cilSpreadsheet} className="me-2" />
                    Excel
                  </CButton>
                </CTooltip>
              </CButtonGroup>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {/* Statistics Cards */}
      {!loading && filteredCupos.length > 0 && (
        <CRow className="mb-4">
          <CCol sm={6} lg={3}>
            <CCard className="text-center bg-primary text-white">
              <CCardBody>
                <div className="fs-4 fw-semibold">{statistics.totalAutomotores}</div>
                <div className="text-uppercase fw-semibold small">Total Automotores</div>
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={3}>
            <CCard className="text-center bg-info text-white">
              <CCardBody>
                <div className="fs-4 fw-semibold">{cop.format(statistics.totalCupoAsignado)}</div>
                <div className="text-uppercase fw-semibold small">Cupo Total Asignado</div>
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={3}>
            <CCard className="text-center bg-success text-white">
              <CCardBody>
                <div className="fs-4 fw-semibold">{cop.format(statistics.totalCupoDisponible)}</div>
                <div className="text-uppercase fw-semibold small">Cupo Disponible</div>
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={3}>
            <CCard className="text-center bg-warning text-white">
              <CCardBody>
                <div className="fs-4 fw-semibold">{cop.format(statistics.totalDebe)}</div>
                <div className="text-uppercase fw-semibold small">Total Debe</div>
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      )}

      {/* Usage Progress */}
      {!loading && filteredCupos.length > 0 && (
        <CCard className="mb-4">
          <CCardHeader>
            <h6 className="mb-0">
              <CIcon icon={cilInfo} className="me-2" />
              Utilización General de Cupos
            </h6>
          </CCardHeader>
          <CCardBody>
            <div className="d-flex justify-content-between align-items-center mb-2">
              <span>Porcentaje de Utilización</span>
              <span className="fw-bold">{statistics.porcentajeUtilizado.toFixed(1)}%</span>
            </div>
            <CProgress
              height={20}
              value={statistics.porcentajeUtilizado}
              color={
                statistics.porcentajeUtilizado >= 90
                  ? 'danger'
                  : statistics.porcentajeUtilizado >= 70
                  ? 'warning'
                  : 'success'
              }
            />
            <div className="mt-2">
              <small className="text-muted">
                {statistics.porcentajeUtilizado >= 90
                  ? '⚠️ Nivel crítico de utilización'
                  : statistics.porcentajeUtilizado >= 70
                  ? '⚡ Nivel alto de utilización'
                  : '✅ Nivel normal de utilización'}
              </small>
            </div>
          </CCardBody>
        </CCard>
      )}

      {/* Main Data Table */}
      <CCard>
        <CCardHeader className="d-flex justify-content-between align-items-center">
          <h5 className="mb-0">
            Detalle de Automotores
            {searchTerm && (
              <CBadge color="info" className="ms-2">
                {filteredCupos.length} de {cupos.length} registros
              </CBadge>
            )}
          </h5>
        </CCardHeader>
        <CCardBody>
          {loading ? (
            <div className="text-center py-5">
              <CSpinner size="lg" className="me-2" />
              <div className="mt-2">Cargando datos de automotores...</div>
            </div>
          ) : filteredCupos.length === 0 ? (
            <CAlert color="warning" className="text-center">
              <CIcon icon={cilInfo} className="me-2" />
              {searchTerm
                ? 'No se encontraron automotores que coincidan con la búsqueda.'
                : 'No hay datos de automotores disponibles.'}
            </CAlert>
          ) : (
            <>
              <CTable striped hover responsive>
                <CTableHead>
                  <CTableRow>
                    <CTableHeaderCell>
                      <CIcon icon={cilUser} className="me-1" />
                      Cliente
                    </CTableHeaderCell>
                    <CTableHeaderCell>Código</CTableHeaderCell>
                    <CTableHeaderCell>NIT/CC</CTableHeaderCell>
                    <CTableHeaderCell>
                      <CIcon icon={cilTruck} className="me-1" />
                      Placa
                    </CTableHeaderCell>
                    <CTableHeaderCell className="text-end">
                      <CIcon icon={cilMoney} className="me-1" />
                      Cupo Asignado
                    </CTableHeaderCell>
                    <CTableHeaderCell className="text-end">Cupo Disponible</CTableHeaderCell>
                    <CTableHeaderCell className="text-end">Debe</CTableHeaderCell>
                    <CTableHeaderCell className="text-center">Estado</CTableHeaderCell>
                  </CTableRow>
                </CTableHead>
                <CTableBody>
                  {currentItems.map((cupo, index) => (
                    <CTableRow key={index}>
                      <CTableDataCell>
                        <div>
                          <strong>{cupo.cliente || 'Sin Cliente'}</strong>
                        </div>
                      </CTableDataCell>
                      <CTableDataCell>
                        <CBadge color="secondary">{cupo.coD_CLI || 'N/A'}</CBadge>
                      </CTableDataCell>
                      <CTableDataCell>{cupo.nit || 'N/A'}</CTableDataCell>
                      <CTableDataCell>
                        <CBadge color="info">{cupo.placa || 'N/A'}</CBadge>
                      </CTableDataCell>
                      <CTableDataCell className="text-end fw-bold">
                        {cop.format(cupo.cupoAsignado || 0)}
                      </CTableDataCell>
                      <CTableDataCell className="text-end fw-bold text-success">
                        {cop.format(cupo.cupoDisponible || 0)}
                      </CTableDataCell>
                      <CTableDataCell className="text-end fw-bold text-danger">
                        {cop.format((cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0))}
                      </CTableDataCell>
                      <CTableDataCell className="text-center">
                        <CBadge color={getCupoStatusColor(cupo.cupoAsignado, cupo.cupoDisponible)}>
                          {getCupoStatusText(cupo.cupoAsignado, cupo.cupoDisponible)}
                        </CBadge>
                      </CTableDataCell>
                    </CTableRow>
                  ))}
                </CTableBody>
              </CTable>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="d-flex justify-content-center mt-4">
                  <CPagination>
                    <CPaginationItem
                      disabled={currentPage === 1}
                      onClick={() => setCurrentPage(currentPage - 1)}
                    >
                      Anterior
                    </CPaginationItem>
                    {[...Array(totalPages)].map((_, index) => (
                      <CPaginationItem
                        key={index + 1}
                        active={currentPage === index + 1}
                        onClick={() => setCurrentPage(index + 1)}
                      >
                        {index + 1}
                      </CPaginationItem>
                    ))}
                    <CPaginationItem
                      disabled={currentPage === totalPages}
                      onClick={() => setCurrentPage(currentPage + 1)}
                    >
                      Siguiente
                    </CPaginationItem>
                  </CPagination>
                </div>
              )}

              {/* Summary Footer */}
              <div className="mt-4 pt-3 border-top">
                <CRow>
                  <CCol md={6}>
                    <small className="text-muted">
                      Mostrando {indexOfFirstItem + 1} a{' '}
                      {Math.min(indexOfLastItem, filteredCupos.length)} de {filteredCupos.length}{' '}
                      registros
                    </small>
                  </CCol>
                  <CCol md={6} className="text-end">
                    <CButtonGroup size="sm">
                      <CButton
                        color="danger"
                        variant="outline"
                        onClick={descargarReporte}
                        disabled={!filteredCupos.length}
                      >
                        <CIcon icon={cilCloudDownload} className="me-1" />
                        PDF
                      </CButton>
                      <CButton
                        color="success"
                        variant="outline"
                        onClick={descargarReporteExcel}
                        disabled={!filteredCupos.length}
                      >
                        <CIcon icon={cilSpreadsheet} className="me-1" />
                        Excel
                      </CButton>
                    </CButtonGroup>
                  </CCol>
                </CRow>
              </div>
            </>
          )}
        </CCardBody>
      </CCard>
    </>
  )
}

export default PorAutomotores
