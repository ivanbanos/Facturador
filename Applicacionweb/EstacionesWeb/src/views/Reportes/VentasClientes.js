import { React, useState, useEffect, useRef } from 'react'
import Filtrarfacturas from '../../services/Filtrarfacturas'
import FiltrarOrdenes from '../../services/FiltrarOrdenes'
import { Link, useNavigate } from 'react-router-dom'
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
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CFormSelect,
  CSpinner,
  CBadge,
  CAlert,
  CButtonGroup,
  CToaster,
  CPagination,
  CPaginationItem,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilCalendar,
  cilCloudDownload,
  cilSearch,
  cilContact,
  cilCalculator,
  cilSpreadsheet,
  cilDescription,
  cilReload,
  cilUser,
  cilMoney,
  cilWallet,
  cilCreditCard,
  cilTruck,
  cilCog,
} from '@coreui/icons'
import Toast from '../toast/Toast'
import * as XLSX from 'xlsx'

var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

let cop = Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
})

const VentasClientes = () => {
  let navigate = useNavigate()
  const [identificacion, setIdentificacion] = useState('')
  const [fechaInicial, setFechaInicial] = useState('')
  const [fechaFinal, setFechaFinal] = useState('')
  const [placas, setPlacas] = useState([])
  const [facturas, setFacturas] = useState([])
  const [ordenes, setOrdenes] = useState([])
  const [loading, setLoading] = useState(false)
  const [hasSearched, setHasSearched] = useState(false)
  const [filteredData, setFilteredData] = useState([])
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)
  const itemsPerPage = 10

  const toastRef = useRef()

  // Combine and filter data based on search term
  useEffect(() => {
    if (!hasSearched) {
      setFilteredData([])
      return
    }

    const allTransactions = [
      ...facturas.map((f) => ({ ...f, tipo: 'FACTURA', consecutivo: f.consecutivo })),
      ...ordenes.map((o) => ({ ...o, tipo: 'ORDEN', consecutivo: o.idVentaLocal })),
    ]

    if (!searchTerm) {
      setFilteredData(allTransactions)
    } else {
      const filtered = allTransactions.filter(
        (item) =>
          (item.placa || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
          (item.combustible || '').toLowerCase().includes(searchTerm.toLowerCase()) ||
          (item.consecutivo || '').toString().toLowerCase().includes(searchTerm.toLowerCase()),
      )
      setFilteredData(filtered)
    }
    setCurrentPage(1)
  }, [facturas, ordenes, searchTerm, hasSearched])

  // Calculate statistics
  const statistics = {
    totalTransacciones: filteredData.length,
    totalFacturas: filteredData.filter((item) => item.tipo === 'FACTURA').length,
    totalOrdenes: filteredData.filter((item) => item.tipo === 'ORDEN').length,
    totalPlacas: [...new Set(filteredData.map((item) => item.placa))].length,
    totalVentas: filteredData.reduce((sum, item) => sum + (item.total || 0), 0),
    totalCantidad: filteredData.reduce((sum, item) => sum + (item.cantidad || 0), 0),
  }

  // Pagination
  const totalPages = Math.ceil(filteredData.length / itemsPerPage)
  const currentPageData = filteredData.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage,
  )
  const FiltrarfacturasHandler = async () => {
    if (!identificacion || identificacion === '') {
      toastRef.current?.addMessage('Debe colocar identificación', 'error')
      return
    }

    if (!fechaInicial || !fechaFinal) {
      toastRef.current?.addMessage('Debe seleccionar fechas inicial y final', 'error')
      return
    }

    setLoading(true)
    setHasSearched(false)

    try {
      let response = await Filtrarfacturas(fechaInicial, fechaFinal, identificacion)
      if (response === 'fail') {
        navigate('/Login', { replace: true })
        return
      }

      setFacturas(response)

      let responseOrdenes = await FiltrarOrdenes(fechaInicial, fechaFinal, identificacion)
      if (responseOrdenes === 'fail') {
        navigate('/Login', { replace: true })
        return
      }

      setOrdenes(responseOrdenes)

      const allPlacas = [
        ...new Set(
          [...response.map((x) => x.placa), ...responseOrdenes.map((x) => x.placa)].filter(
            (placa) => placa,
          ),
        ),
      ]

      setPlacas(allPlacas)
      setHasSearched(true)

      toastRef.current?.addMessage(
        `Se encontraron ${response.length} facturas y ${responseOrdenes.length} órdenes`,
        'success',
      )
    } catch (error) {
      console.error('Error fetching data:', error)
      toastRef.current?.addMessage('Error al consultar los datos', 'error')
    } finally {
      setLoading(false)
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

  const handleIdentificacionSelectedChange = (event) => {
    const value = event.target.value
    setIdentificacion(value)
  }

  // Función para verificar si la fecha es válida
  const isValidDate = (date) => {
    const pattern = /^\d{4}-\d{2}-\d{2}$/
    return pattern.test(date)
  }

  const descargarReporte = async (event) => {
    let estacionNombre = localStorage.getItem('estacionNombre')
    let estacionNit = localStorage.getItem('estacionNit')

    var elements = [
      {
        stack: [estacionNombre, { text: 'Nit ' + estacionNit, style: 'subheader' }],
        style: 'header',
      },
      {
        text: 'Reporte de ventas por Cliente',
        style: 'header',
      },
      {
        text: 'Cliente ' + identificacion,
      },
      {
        text: 'Fecha Inicial ' + fechaInicial + ' Fecha Final ' + fechaFinal,
      },
    ]
    for (const placa of placas) {
      let placaTabla = [
        ['Tipo', 'Consecutivo', 'Fecha', 'Articulo', 'Km Actual', 'Cantidad', 'Valor'],
      ]
      let placasFacturaTableElements = facturas
        .filter((x) => x.placa == placa)
        .map((venta) => [
          'F',
          venta.consecutivo,
          venta.fecha,
          venta.combustible,
          venta.kilometraje,
          venta.cantidad,
          { text: cop.format(venta.total), style: 'tableRight' },
        ])
      placaTabla = placaTabla.concat(placasFacturaTableElements)
      let placasOrdenesTableElements = ordenes
        .filter((x) => x.placa == placa)
        .map((venta) => [
          'O',
          venta.idVentaLocal,
          venta.fecha,
          venta.combustible,
          venta.kilometraje,
          venta.cantidad,
          { text: cop.format(venta.total), style: 'tableRight' },
        ])
      placaTabla = placaTabla.concat(placasOrdenesTableElements)
      elements = elements.concat({
        stack: ['Placa ' + placa],
      })
      elements = elements.concat({
        style: 'tableExample',
        table: {
          headerRows: 1,
          body: placaTabla,
        },
      })
      elements = elements.concat({
        stack: [
          'Total ' +
            [
              ...new Set(
                [
                  ...ordenes.filter((x) => x.placa == placa).map((x) => x.total),
                  ...facturas.filter((x) => x.placa == placa).map((x) => x.total),
                ].filter((x, i, a) => a.indexOf(x) == i),
              ),
            ].reduce((accumulator, value) => {
              return accumulator + value
            }, 0),
        ],
      })
    }
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

      content: elements,

      styles: {
        header: {
          fontSize: 18,
          bold: true,
          alignment: 'center',
        },
        subheader: {
          fontSize: 14,
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
      .download(
        'ReporteVentas - ' +
          estacionNombre +
          ' - ' +
          identificacion +
          ' - ' +
          fechaInicial +
          '-' +
          fechaFinal +
          '.pdf',
      )
  }

  const descargarReporteExcel = () => {
    if (!filteredData.length) {
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
        ['REPORTE DE VENTAS POR CLIENTE'],
        [''],
        ['Estación:', estacionNombre],
        ['NIT:', estacionNit],
        ['Cliente:', identificacion],
        ['Período:', `${fechaInicial} - ${fechaFinal}`],
        ['Fecha de generación:', today],
        [''],
        ['RESUMEN ESTADÍSTICO'],
        ['Total transacciones:', statistics.totalTransacciones],
        ['Total facturas:', statistics.totalFacturas],
        ['Total órdenes:', statistics.totalOrdenes],
        ['Total placas:', statistics.totalPlacas],
        ['Total ventas:', statistics.totalVentas],
        ['Total cantidad:', statistics.totalCantidad],
      ]

      const resumenSheet = XLSX.utils.aoa_to_sheet(resumenData)
      resumenSheet['!cols'] = [{ width: 25 }, { width: 30 }]
      XLSX.utils.book_append_sheet(workbook, resumenSheet, 'Resumen General')

      // Hoja 2: Detalle por Placa
      for (const placa of placas) {
        const placaFacturas = facturas.filter((x) => x.placa === placa)
        const placaOrdenes = ordenes.filter((x) => x.placa === placa)

        const placaData = [
          [`PLACA: ${placa}`],
          [''],
          ['Tipo', 'Consecutivo', 'Fecha', 'Artículo', 'Km Actual', 'Cantidad', 'Valor'],
          ...placaFacturas.map((venta) => [
            'FACTURA',
            venta.consecutivo,
            venta.fecha,
            venta.combustible,
            venta.kilometraje,
            venta.cantidad,
            venta.total,
          ]),
          ...placaOrdenes.map((venta) => [
            'ORDEN',
            venta.idVentaLocal,
            venta.fecha,
            venta.combustible,
            venta.kilometraje,
            venta.cantidad,
            venta.total,
          ]),
          [''],
          [
            'TOTAL:',
            '',
            '',
            '',
            '',
            '',
            [...placaFacturas, ...placaOrdenes].reduce((sum, venta) => sum + (venta.total || 0), 0),
          ],
        ]

        const placaSheet = XLSX.utils.aoa_to_sheet(placaData)
        placaSheet['!cols'] = [
          { width: 12 }, // Tipo
          { width: 15 }, // Consecutivo
          { width: 15 }, // Fecha
          { width: 20 }, // Artículo
          { width: 12 }, // Km Actual
          { width: 12 }, // Cantidad
          { width: 15 }, // Valor
        ]

        const placaName = placa.replace(/[^\w\s]/gi, '').substring(0, 31) // Excel sheet name limit
        XLSX.utils.book_append_sheet(workbook, placaSheet, `Placa_${placaName}`)
      }

      // Hoja 3: Todas las transacciones
      const todasData = [
        ['TODAS LAS TRANSACCIONES'],
        [''],
        ['Tipo', 'Placa', 'Consecutivo', 'Fecha', 'Artículo', 'Km Actual', 'Cantidad', 'Valor'],
        ...filteredData.map((venta) => [
          venta.tipo,
          venta.placa,
          venta.consecutivo,
          venta.fecha,
          venta.combustible,
          venta.kilometraje,
          venta.cantidad,
          venta.total,
        ]),
      ]

      const todasSheet = XLSX.utils.aoa_to_sheet(todasData)
      todasSheet['!cols'] = [
        { width: 12 }, // Tipo
        { width: 12 }, // Placa
        { width: 15 }, // Consecutivo
        { width: 15 }, // Fecha
        { width: 20 }, // Artículo
        { width: 12 }, // Km Actual
        { width: 12 }, // Cantidad
        { width: 15 }, // Valor
      ]
      XLSX.utils.book_append_sheet(workbook, todasSheet, 'Todas las Transacciones')

      // Descargar archivo
      const fileName = `ReporteVentas_${identificacion.replace(
        /\s+/g,
        '_',
      )}_${fechaInicial}_${fechaFinal}_${new Date().toISOString().split('T')[0]}.xlsx`
      XLSX.writeFile(workbook, fileName)

      toastRef.current?.addMessage('Reporte Excel descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error downloading Excel report:', error)
      toastRef.current?.addMessage('Error al descargar el reporte Excel', 'error')
    }
  }

  // Función para formatear moneda
  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-CO', {
      style: 'currency',
      currency: 'COP',
      minimumFractionDigits: 0,
    }).format(amount || 0)
  }

  // Función para obtener el color del badge según el tipo
  const getTypeColor = (tipo) => {
    return tipo === 'FACTURA' ? 'success' : 'info'
  }

  // Función para manejar la búsqueda
  const handleSearch = (term) => {
    setSearchTerm(term)
    setCurrentPage(1)
  }

  return (
    <>
      <CToaster ref={toastRef} />
      <div className="page-header mb-4">
        <CRow>
          <CCol>
            <h1 className="h3 mb-0 d-flex align-items-center">
              <CIcon icon={cilUser} className="me-2" />
              Ventas por Cliente
            </h1>
            <p className="text-medium-emphasis mb-0">
              Consulta detallada de ventas por cliente y período
            </p>
          </CCol>
        </CRow>
      </div>

      {/* Search Form */}
      <CCard className="mb-4">
        <CCardHeader>
          <h5 className="mb-0 d-flex align-items-center">
            <CIcon icon={cilSearch} className="me-2" />
            Criterios de Búsqueda
          </h5>
        </CCardHeader>
        <CCardBody>
          <CRow className="g-3">
            <CCol md={4}>
              <CFormLabel htmlFor="fechaInicial">Fecha Inicial</CFormLabel>
              <CFormInput
                type="date"
                id="fechaInicial"
                value={fechaInicial}
                onChange={handlefechaInicialSelectedChange}
                placeholder="Seleccione fecha inicial"
              />
            </CCol>
            <CCol md={4}>
              <CFormLabel htmlFor="fechaFinal">Fecha Final</CFormLabel>
              <CFormInput
                type="date"
                id="fechaFinal"
                value={fechaFinal}
                onChange={handlefechaFinalSelectedChange}
                placeholder="Seleccione fecha final"
              />
            </CCol>
            <CCol md={4}>
              <CFormLabel htmlFor="identificacion">Identificación del Cliente</CFormLabel>
              <CFormInput
                type="text"
                id="identificacion"
                value={identificacion}
                onChange={handleIdentificacionSelectedChange}
                placeholder="Ingrese identificación"
              />
            </CCol>
            <CCol xs={12}>
              <CButton
                color="primary"
                onClick={FiltrarfacturasHandler}
                disabled={loading}
                className="d-flex align-items-center"
              >
                {loading ? (
                  <CSpinner size="sm" className="me-2" />
                ) : (
                  <CIcon icon={cilSearch} className="me-2" />
                )}
                {loading ? 'Buscando...' : 'Buscar Ventas'}
              </CButton>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {/* Statistics Cards */}
      {hasSearched && filteredData.length > 0 && (
        <CRow className="mb-4">
          <CCol sm={6} lg={2}>
            <CCard className="mb-3 border-start border-start-4 border-primary">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-primary">
                    {statistics.totalTransacciones}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Transacciones
                  </div>
                </div>
                <CIcon icon={cilCalculator} size="xl" className="text-primary" />
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={2}>
            <CCard className="mb-3 border-start border-start-4 border-success">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-success">{statistics.totalFacturas}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Facturas
                  </div>
                </div>
                <CIcon icon={cilDescription} size="xl" className="text-success" />
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={2}>
            <CCard className="mb-3 border-start border-start-4 border-info">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-info">{statistics.totalOrdenes}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Órdenes
                  </div>
                </div>
                <CIcon icon={cilCloudDownload} size="xl" className="text-info" />
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={2}>
            <CCard className="mb-3 border-start border-start-4 border-warning">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-warning">{statistics.totalPlacas}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Placas
                  </div>
                </div>
                <CIcon icon={cilTruck} size="xl" className="text-warning" />
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={2}>
            <CCard className="mb-3 border-start border-start-4 border-danger">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-danger">
                    {formatCurrency(statistics.totalVentas)}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Ventas
                  </div>
                </div>
                <CIcon icon={cilMoney} size="xl" className="text-danger" />
              </CCardBody>
            </CCard>
          </CCol>
          <CCol sm={6} lg={2}>
            <CCard className="mb-3 border-start border-start-4 border-dark">
              <CCardBody className="pb-0 d-flex justify-content-between align-items-start">
                <div>
                  <div className="fs-4 fw-semibold text-dark">
                    {statistics.totalCantidad.toLocaleString()}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Cantidad
                  </div>
                </div>
                <CIcon icon={cilCog} size="xl" className="text-dark" />
              </CCardBody>
            </CCard>
          </CCol>
        </CRow>
      )}

      {/* Search and Export Controls */}
      {hasSearched && filteredData.length > 0 && (
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
                    placeholder="Buscar por placa, combustible o consecutivo..."
                    value={searchTerm}
                    onChange={(e) => handleSearch(e.target.value)}
                    style={{ paddingLeft: '40px' }}
                  />
                </div>
              </CCol>
              <CCol md={6} className="text-md-end mt-2 mt-md-0">
                <CButtonGroup>
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
                </CButtonGroup>
              </CCol>
            </CRow>
          </CCardBody>
        </CCard>
      )}

      {/* Main Content */}
      <CCard>
        <CCardBody>
          {loading ? (
            <div className="text-center py-5">
              <CSpinner color="primary" style={{ width: '3rem', height: '3rem' }} />
              <div className="mt-3 text-medium-emphasis">Consultando ventas del cliente...</div>
            </div>
          ) : !hasSearched ? (
            <div className="text-center py-5">
              <CIcon icon={cilSearch} size="3xl" className="text-medium-emphasis mb-3" />
              <h5 className="text-medium-emphasis">Consulta de Ventas por Cliente</h5>
              <p className="text-medium-emphasis mb-4">
                Complete los criterios de búsqueda y haga clic en &quot;Buscar Ventas&quot; para
                consultar las transacciones
              </p>
            </div>
          ) : filteredData.length === 0 ? (
            <div className="text-center py-5">
              <CIcon icon={cilUser} size="3xl" className="text-medium-emphasis mb-3" />
              <h5 className="text-medium-emphasis">No se encontraron ventas</h5>
              <p className="text-medium-emphasis mb-4">
                No hay transacciones registradas para el cliente &quot;{identificacion}&quot; en el
                período seleccionado
              </p>
            </div>
          ) : (
            <>
              <div className="d-flex justify-content-between align-items-center mb-3">
                <h6 className="mb-0">
                  Transacciones encontradas ({filteredData.length})
                  {searchTerm && ` - Filtrado por: "${searchTerm}"`}
                </h6>
                <span className="text-medium-emphasis">
                  Mostrando{' '}
                  {Math.min(itemsPerPage, filteredData.length - (currentPage - 1) * itemsPerPage)}{' '}
                  de {filteredData.length} transacciones
                </span>
              </div>

              <div className="table-responsive">
                <table className="table table-hover table-striped">
                  <thead className="table-dark">
                    <tr>
                      <th>Tipo</th>
                      <th>Placa</th>
                      <th>Consecutivo</th>
                      <th>Fecha</th>
                      <th>Combustible</th>
                      <th className="text-end">Kilometraje</th>
                      <th className="text-end">Cantidad</th>
                      <th className="text-end">Valor</th>
                    </tr>
                  </thead>
                  <tbody>
                    {currentPageData.map((venta, index) => (
                      <tr key={`${venta.tipo}-${venta.consecutivo}-${index}`}>
                        <td>
                          <CBadge color={getTypeColor(venta.tipo)} className="rounded-pill">
                            {venta.tipo}
                          </CBadge>
                        </td>
                        <td>
                          <code className="text-muted fw-bold">{venta.placa || 'N/A'}</code>
                        </td>
                        <td>{venta.consecutivo || 'N/A'}</td>
                        <td>{venta.fecha || 'N/A'}</td>
                        <td>
                          <div className="d-flex align-items-center">
                            <CIcon icon={cilCog} className="me-2 text-muted" />
                            {venta.combustible || 'N/A'}
                          </div>
                        </td>
                        <td className="text-end text-muted">
                          {(venta.kilometraje || 0).toLocaleString()}
                        </td>
                        <td className="text-end fw-semibold">
                          {(venta.cantidad || 0).toLocaleString()}
                        </td>
                        <td className="text-end fw-semibold text-success">
                          {formatCurrency(venta.total)}
                        </td>
                      </tr>
                    ))}
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
          )}
        </CCardBody>
      </CCard>
    </>
  )
}

export default VentasClientes
