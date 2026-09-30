import { React, useState, useEffect, useRef } from 'react'
import EstacionesDropdown from '../estaciones/EstacionesDropdown'
import GetCuposPorClientes from '../../services/GetCuposPorClientes'
import { Link, useNavigate } from 'react-router-dom'
import { Page, Text, View, Document, StyleSheet } from '@react-pdf/renderer'
import CIcon from '@coreui/icons-react'
import {
  cilPlus,
  cilPencil,
  cilX,
  cilCloudDownload,
  cilSpreadsheet,
  cilUser,
  cilSearch,
  cilReload,
  cilInfo,
  cilMoney,
  cilChart,
  cilDescription,
  cilWallet,
  cilCreditCard,
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
  CToaster,
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

const PorCliente = () => {
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
    totalClientes: filteredCupos.length,
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
    clientesConDeuda: filteredCupos.filter(
      (cupo) => (cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0) > 0,
    ).length,
  }

  const fetchCupos = async () => {
    try {
      setLoading(true)
      const estacionGuid = localStorage.getItem('estacionGuid')
      if (!estacionGuid) {
        navigate('/Login', { replace: true })
        return
      }

      const response = await GetCuposPorClientes(estacionGuid)
      if (response === 'fail') {
        navigate('/Login', { replace: true })
      } else {
        // El nuevo servicio retorna un array de objetos con información de clientes
        if (Array.isArray(response)) {
          setCupos(response)
          setFilteredCupos(response)
          setTotalCupos(response.length)
          toastRef.current?.addMessage(
            `Se cargaron ${response.length} clientes exitosamente`,
            'success',
          )
        } else {
          // Fallback para compatibilidad
          setCupos([])
          setFilteredCupos([])
          setTotalCupos(0)
          toastRef.current?.addMessage('No se encontraron datos de clientes', 'warning')
        }
      }
    } catch (error) {
      console.error('Error fetching cupos clientes:', error)
      toastRef.current?.addMessage('Error al cargar los datos de clientes', 'error')
    } finally {
      setLoading(false)
    }
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

  // Función para formatear moneda
  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(amount || 0)
  }

  // Función para manejar la búsqueda
  const handleSearch = (term) => {
    setSearchTerm(term)
    setCurrentPage(1) // Reset to first page when searching
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

      let cuposTabla = [['Cliente', 'Código', 'Nit/CC', 'Cupo asignado', 'Cupo disponible', 'Debe']]
      let cuposTableElements = filteredCupos.map((cupo) => [
        cupo.cliente || '',
        cupo.coD_CLI || '',
        cupo.nit || '',
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
            text: 'Reporte de Cupos de Clientes',
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
        .download('ReporteCuposClientes - ' + estacionNombre + ' - ' + today + '.pdf')

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
        ['REPORTE DE CUPOS POR CLIENTES'],
        [''],
        ['Estación:', estacionNombre],
        ['NIT:', estacionNit],
        ['Fecha de generación:', today],
        ['Total de clientes:', filteredCupos.length],
        ['Clientes con deuda:', statistics.clientesConDeuda],
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

      // Hoja 2: Detalle de Clientes
      const clientesData = [
        ['Cliente', 'Código', 'Nit/CC', 'Cupo Asignado', 'Cupo Disponible', 'Debe', 'Estado'],
        ...filteredCupos.map((cupo) => [
          cupo.cliente || '',
          cupo.coD_CLI || '',
          cupo.nit || '',
          cupo.cupoAsignado || 0,
          cupo.cupoDisponible || 0,
          (cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0),
          getCupoStatusText(cupo.cupoAsignado, cupo.cupoDisponible),
        ]),
      ]

      const clientesSheet = XLSX.utils.aoa_to_sheet(clientesData)
      clientesSheet['!cols'] = [
        { width: 30 }, // Cliente
        { width: 15 }, // Código
        { width: 20 }, // Nit/CC
        { width: 18 }, // Cupo Asignado
        { width: 18 }, // Cupo Disponible
        { width: 15 }, // Debe
        { width: 15 }, // Estado
      ]

      XLSX.utils.book_append_sheet(workbook, clientesSheet, 'Detalle Clientes')

      // Hoja 3: Análisis de Deuda
      const deudaData = [
        ['ANÁLISIS DE DEUDA POR CLIENTE'],
        [''],
        ['Cliente', 'Debe', 'Porcentaje del Total'],
      ]

      const totalDeuda = filteredCupos.reduce(
        (sum, cupo) => sum + ((cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0)),
        0,
      )

      filteredCupos
        .filter((cupo) => (cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0) > 0)
        .sort(
          (a, b) =>
            (b.cupoAsignado || 0) -
            (b.cupoDisponible || 0) -
            ((a.cupoAsignado || 0) - (a.cupoDisponible || 0)),
        )
        .forEach((cupo) => {
          const debe = (cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0)
          const porcentaje = totalDeuda > 0 ? (debe / totalDeuda) * 100 : 0
          deudaData.push([cupo.cliente || '', debe, `${porcentaje.toFixed(2)}%`])
        })

      const deudaSheet = XLSX.utils.aoa_to_sheet(deudaData)
      deudaSheet['!cols'] = [{ width: 30 }, { width: 18 }, { width: 20 }]
      XLSX.utils.book_append_sheet(workbook, deudaSheet, 'Análisis Deuda')

      // Descargar archivo
      const fileName = `ReporteCuposClientes_${estacionNombre.replace(/\s+/g, '_')}_${
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
      <CToaster ref={toastRef} />
      <div className="page-header mb-4">
        <CRow>
          <CCol>
            <h1 className="h3 mb-0 d-flex align-items-center">
              <CIcon icon={cilUser} className="me-2" />
              Cupos por Clientes
            </h1>
            <p className="text-medium-emphasis mb-0">
              Gestión y consulta de cupos asignados a clientes
            </p>
          </CCol>
          <CCol xs="auto">
            <CButtonGroup>
              <CButton
                color="primary"
                onClick={fetchCupos}
                disabled={loading}
                className="d-flex align-items-center"
              >
                {loading ? (
                  <CSpinner size="sm" className="me-2" />
                ) : (
                  <CIcon icon={cilReload} className="me-2" />
                )}
                {loading ? 'Cargando...' : 'Actualizar'}
              </CButton>
              {filteredCupos.length > 0 && (
                <>
                  <CButton
                    color="success"
                    variant="outline"
                    onClick={descargarReporte}
                    className="d-flex align-items-center"
                  >
                    <CIcon icon={cilDescription} className="me-2" />
                    PDF
                  </CButton>
                  <CButton
                    color="info"
                    variant="outline"
                    onClick={descargarReporteExcel}
                    className="d-flex align-items-center"
                  >
                    <CIcon icon={cilSpreadsheet} className="me-2" />
                    Excel
                  </CButton>
                </>
              )}
            </CButtonGroup>
          </CCol>
        </CRow>
      </div>

      {/* Statistics Cards */}
      {filteredCupos.length > 0 && (
        <CRow className="mb-4">
          <CCol sm={6} lg={3}>
            <CCard className="mb-3 border-start border-start-4 border-primary">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-primary">{statistics.totalClientes}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Clientes
                  </div>
                </div>
                <CIcon icon={cilUser} size="xl" className="text-primary" />
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={3}>
            <CCard className="mb-3 border-start border-start-4 border-success">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-success">
                    {formatCurrency(statistics.totalCupoAsignado)}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Asignado
                  </div>
                </div>
                <CIcon icon={cilMoney} size="xl" className="text-success" />
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={3}>
            <CCard className="mb-3 border-start border-start-4 border-info">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-info">
                    {formatCurrency(statistics.totalCupoDisponible)}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Disponible
                  </div>
                </div>
                <CIcon icon={cilWallet} size="xl" className="text-info" />
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={3}>
            <CCard className="mb-3 border-start border-start-4 border-warning">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-warning">
                    {formatCurrency(statistics.totalDebe)}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Debe
                  </div>
                </div>
                <CIcon icon={cilCreditCard} size="xl" className="text-warning" />
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      )}

      {/* Search and Controls */}
      <CCard className="mb-4">
        <CCardBody>
          <CRow className="align-items-center">
            <CCol md={6}>
              <div className="position-relative">
                <CIcon
                  icon={cilSearch}
                  className="position-absolute"
                  style={{ left: '12px', top: '50%', transform: 'translateY(-50%)', zIndex: 10 }}
                />
                <CFormInput
                  type="text"
                  placeholder="Buscar por cliente, código o NIT..."
                  value={searchTerm}
                  onChange={(e) => handleSearch(e.target.value)}
                  style={{ paddingLeft: '40px' }}
                />
              </div>
            </CCol>
            <CCol md={6} className="text-md-end mt-2 mt-md-0">
              {filteredCupos.length > 0 && (
                <span className="text-medium-emphasis">
                  Mostrando{' '}
                  {Math.min(itemsPerPage, filteredCupos.length - (currentPage - 1) * itemsPerPage)}{' '}
                  de {filteredCupos.length} clientes
                  {searchTerm && ` (filtrado de ${cupos.length} total)`}
                </span>
              )}
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {/* Main Content */}
      <CCard>
        <CCardBody>
          {loading ? (
            <div className="text-center py-5">
              <CSpinner color="primary" style={{ width: '3rem', height: '3rem' }} />
              <div className="mt-3 text-medium-emphasis">Cargando información de cupos...</div>
            </div>
          ) : currentItems.length > 0 ? (
            <>
              <div className="table-responsive">
                <table className="table table-hover table-striped">
                  <thead className="table-dark">
                    <tr>
                      <th>Cliente</th>
                      <th>Código</th>
                      <th>Nit/CC</th>
                      <th className="text-end">Cupo Asignado</th>
                      <th className="text-end">Cupo Disponible</th>
                      <th className="text-end">Debe</th>
                      <th className="text-center">Estado</th>
                    </tr>
                  </thead>
                  <tbody>
                    {currentItems.map((cupo, index) => {
                      const debe = (cupo.cupoAsignado || 0) - (cupo.cupoDisponible || 0)
                      return (
                        <tr key={index}>
                          <td>
                            <div className="fw-semibold">{cupo.cliente || 'N/A'}</div>
                          </td>
                          <td>
                            <code className="text-muted">{cupo.coD_CLI || 'N/A'}</code>
                          </td>
                          <td>{cupo.nit || 'N/A'}</td>
                          <td className="text-end fw-semibold">
                            {formatCurrency(cupo.cupoAsignado)}
                          </td>
                          <td className="text-end text-success fw-semibold">
                            {formatCurrency(cupo.cupoDisponible)}
                          </td>
                          <td className="text-end fw-semibold">
                            <span className={debe > 0 ? 'text-danger' : 'text-muted'}>
                              {formatCurrency(debe)}
                            </span>
                          </td>
                          <td className="text-center">
                            <CBadge
                              color={getCupoStatusColor(cupo.cupoAsignado, cupo.cupoDisponible)}
                              className="rounded-pill"
                            >
                              {getCupoStatusText(cupo.cupoAsignado, cupo.cupoDisponible)}
                            </CBadge>
                          </td>
                        </tr>
                      )
                    })}
                  </tbody>
                </table>
              </div>

              {/* Pagination */}
              {totalPages > 1 && (
                <div className="d-flex justify-content-between align-items-center mt-4">
                  <div className="text-medium-emphasis">
                    Página {currentPage} de {totalPages}
                  </div>
                  <CPagination className="mb-0">
                    <CPaginationItem disabled={currentPage === 1} onClick={() => setCurrentPage(1)}>
                      Primera
                    </CPaginationItem>
                    <CPaginationItem
                      disabled={currentPage === 1}
                      onClick={() => setCurrentPage(currentPage - 1)}
                    >
                      Anterior
                    </CPaginationItem>

                    {[...Array(Math.min(5, totalPages))].map((_, index) => {
                      const pageNumber = Math.max(
                        1,
                        Math.min(currentPage - 2 + index, totalPages - 4 + index + 1),
                      )
                      if (pageNumber <= totalPages && pageNumber >= 1) {
                        return (
                          <CPaginationItem
                            key={pageNumber}
                            active={currentPage === pageNumber}
                            onClick={() => setCurrentPage(pageNumber)}
                          >
                            {pageNumber}
                          </CPaginationItem>
                        )
                      }
                      return null
                    })}

                    <CPaginationItem
                      disabled={currentPage === totalPages}
                      onClick={() => setCurrentPage(currentPage + 1)}
                    >
                      Siguiente
                    </CPaginationItem>
                    <CPaginationItem
                      disabled={currentPage === totalPages}
                      onClick={() => setCurrentPage(totalPages)}
                    >
                      Última
                    </CPaginationItem>
                  </CPagination>
                </div>
              )}
            </>
          ) : (
            <div className="text-center py-5">
              <CIcon icon={cilUser} size="3xl" className="text-medium-emphasis mb-3" />
              <h5 className="text-medium-emphasis">
                {searchTerm ? 'No se encontraron clientes' : 'No hay datos disponibles'}
              </h5>
              <p className="text-medium-emphasis mb-4">
                {searchTerm
                  ? `No hay clientes que coincidan con "${searchTerm}"`
                  : 'Haga clic en "Actualizar" para cargar la información de cupos'}
              </p>
              {searchTerm && (
                <CButton color="primary" variant="outline" onClick={() => handleSearch('')}>
                  Limpiar búsqueda
                </CButton>
              )}
            </div>
          )}
        </CCardBody>
      </CCard>
    </>
  )
}

export default PorCliente
