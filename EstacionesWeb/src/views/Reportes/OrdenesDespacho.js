import { React, useState, useRef, useEffect } from 'react'
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
  CPagination,
  CPaginationItem,
  CInputGroup,
  CInputGroupText,
  CButtonGroup,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilCalendar,
  cilSearch,
  cilTruck,
  cilCalculator,
  cilClock,
  cilUser,
  cilLocationPin,
  cilPhone,
  cilCloudDownload,
  cilSpreadsheet,
} from '@coreui/icons'
import Toast from '../toast/Toast'
import * as XLSX from 'xlsx'

var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

const OrdenesDespacho = () => {
  const navigate = useNavigate()
  const toastRef = useRef()
  const ordenesService = new OrdenesDespachoService()

  // Estados
  const [fechaInicial, setFechaInicial] = useState('')
  const [fechaFinal, setFechaFinal] = useState('')
  const [ordenes, setOrdenes] = useState([])
  const [loading, setLoading] = useState(false)
  const [showResults, setShowResults] = useState(false)
  const [currentPage, setCurrentPage] = useState(1)
  const [itemsPerPage] = useState(10)
  const [searchTerm, setSearchTerm] = useState('')
  const [sinFacturar, setSinFacturar] = useState(false)

  // Función para obtener la fecha de ayer
  const getYesterdayDate = () => {
    const yesterday = new Date()
    yesterday.setDate(yesterday.getDate() - 1)
    return yesterday.toISOString().split('T')[0]
  }

  // Función para obtener la fecha de hoy
  const getTodayDate = () => {
    const today = new Date()
    return today.toISOString().split('T')[0]
  }

  // Inicializar fechas al cargar el componente
  useEffect(() => {
    const today = getTodayDate()
    setFechaInicial(today)
    setFechaFinal(today)
    // Cargar órdenes del día actual automáticamente
    handleSearch(today, today)
  }, [])

  // Función para buscar órdenes
  const handleSearch = async (fechaIni = fechaInicial, fechaFin = fechaFinal) => {
    if (!fechaIni || !fechaFin) {
      toastRef.current?.addMessage('Debe seleccionar las fechas inicial y final', 'error')
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
      setCurrentPage(1)

      toastRef.current?.addMessage(`Se encontraron ${(response || []).length} órdenes`, 'success')
    } catch (error) {
      console.error('Error:', error)
      toastRef.current?.addMessage('Error al buscar las órdenes', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Función para obtener el color del badge según el estado
  const getEstadoBadgeColor = (estado) => {
    switch (estado?.toLowerCase()) {
      case 'pendiente':
        return 'warning'
      case 'anulada':
      case 'anulado':
        return 'danger'
      case 'completado':
      case 'completada':
      case 'facturado':
      case 'facturada':
        return 'success'
      case 'en_proceso':
      case 'en proceso':
      case 'procesando':
        return 'info'
      default:
        return 'secondary'
    }
  }

  // Función para transformar el estado mostrado al usuario
  const getEstadoDisplay = (estado) => {
    if (!estado) return 'N/A'
    const estadoLower = estado.toLowerCase()
    if (estadoLower === 'anulada' || estadoLower === 'anulado') {
      return 'Facturada'
    }
    return estado
  }

  // Función para formatear el ID de factura electrónica
  const formatIdFacturaElectronica = (idFacturaElectronica) => {
    if (!idFacturaElectronica) return 'N/A'

    try {
      // El formato es: Ok:EADM401708:743158686f2baef1408ce0b373f6bdcb980414e911744b18f9f63a945291e38a27d2139f38bba6199c95ff752a96387a
      const parts = idFacturaElectronica.split(':')

      if (parts.length >= 3) {
        const estado = parts[0] // Ok o Error
        const consecutivo = parts[1] // EADM401708
        const cufe = parts[2] // El hash largo

        // Truncar el CUFE para mostrar solo los primeros 10 caracteres
        const cufeCorto = cufe.length > 10 ? cufe.substring(0, 10) + '...' : cufe

        return {
          completo: `${consecutivo} (${cufeCorto})`,
          consecutivo: consecutivo,
          cufe: cufe,
          estado: estado,
        }
      }

      return 'Formato inválido'
    } catch (error) {
      return 'Error al procesar'
    }
  }

  // Función para formatear fecha
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A'
    const date = new Date(dateString)
    return date.toLocaleDateString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  }

  // Función para formatear moneda
  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('es-ES', {
      style: 'currency',
      currency: 'COP',
    }).format(amount || 0)
  }

  const getFormaPago = (orden) => {
    return orden.formaDePago || orden.formaPago || orden.FormaDePago || 'N/A'
  }

  // Filtrar órdenes por término de búsqueda y por el checkbox sin facturar
  const filteredOrdenes = ordenes
    .filter(
      (orden) =>
        orden.numeroTransaccion?.toString().includes(searchTerm) ||
        orden.idVentaLocal?.toString().includes(searchTerm) ||
        orden.nombreTercero?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        orden.identificacion?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        orden.placa?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        orden.combustible?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        orden.vendedor?.toLowerCase().includes(searchTerm.toLowerCase()),
    )
    .filter((orden) => {
      if (!sinFacturar) return true
      const id = orden.idFacturaElectronica
      const result = !id || (!id.startsWith('Ok') && !id.startsWith('ok'))
      console.log('Filtrando orden:', orden, 'idFacturaElectronica:', id, 'result:', result)
      return result
    })

  // Paginación
  const indexOfLastItem = currentPage * itemsPerPage
  const indexOfFirstItem = indexOfLastItem - itemsPerPage
  const currentItems = filteredOrdenes.slice(indexOfFirstItem, indexOfLastItem)
  const totalPages = Math.ceil(filteredOrdenes.length / itemsPerPage)

  // Función para ir a detalle de orden
  const goToOrdenDetalle = (orden) => {
    navigate(`/OrdenDespachoDetalle/${orden.guid || orden.idVentaLocal}`, {
      state: { orden: orden },
    })
  }

  // Función para descargar tabla en PDF
  const descargarTablaPDF = () => {
    if (!filteredOrdenes.length) {
      toastRef.current?.addMessage('No hay órdenes para descargar', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'

      // Crear las filas de la tabla
      const tableRows = [
        // Encabezado de la tabla
        [
          { text: 'ID Transacción', style: 'tableHeader' },
          { text: 'Fecha', style: 'tableHeader' },
          { text: 'Cliente', style: 'tableHeader' },
          { text: 'Forma de Pago', style: 'tableHeader' },
          { text: 'Combustible', style: 'tableHeader' },
          { text: 'Placa', style: 'tableHeader' },
          { text: 'Cantidad', style: 'tableHeader' },
          { text: 'ID Factura', style: 'tableHeader' },
          { text: 'Estado', style: 'tableHeader' },
          { text: 'Total', style: 'tableHeader' },
        ],
        // Filas de datos
        ...filteredOrdenes.map((orden) => {
          const facturaInfo = formatIdFacturaElectronica(orden.idFacturaElectronica)
          const idFactura = typeof facturaInfo === 'object' ? facturaInfo.completo : facturaInfo

          return [
            orden.numeroTransaccion || orden.idVentaLocal || 'N/A',
            formatDate(orden.fecha),
            `${orden.nombreTercero || 'N/A'}\n${orden.identificacion || ''}`,
            getFormaPago(orden),
            orden.combustible || 'N/A',
            orden.placa || 'N/A',
            orden.cantidad || '0',
            idFactura,
            getEstadoDisplay(orden.estado),
            formatCurrency(orden.total),
          ]
        }),
      ]

      const docDefinition = {
        pageSize: 'A4',
        pageOrientation: 'landscape',
        pageMargins: [40, 60, 40, 60],
        content: [
          // Encabezado
          {
            columns: [
              {
                width: '*',
                stack: [
                  { text: estacionNombre, style: 'header' },
                  { text: `NIT: ${estacionNit}`, style: 'subheader' },
                ],
              },
              {
                width: 'auto',
                stack: [
                  { text: 'REPORTE DE ÓRDENES DE DESPACHO', style: 'title', alignment: 'right' },
                  {
                    text: `Período: ${fechaInicial} - ${fechaFinal}`,
                    style: 'normal',
                    alignment: 'right',
                  },
                  {
                    text: `Generado: ${new Date().toLocaleDateString('es-ES')}`,
                    style: 'normal',
                    alignment: 'right',
                  },
                ],
              },
            ],
            margin: [0, 0, 0, 20],
          },

          // Resumen estadísticas
          {
            text: 'RESUMEN',
            style: 'sectionHeader',
            margin: [0, 10, 0, 5],
          },
          {
            columns: [
              {
                width: '25%',
                text: [{ text: 'Total Órdenes: ', bold: true }, estadisticas.total.toString()],
              },
              {
                width: '25%',
                text: [{ text: 'Pendientes: ', bold: true }, estadisticas.pendientes.toString()],
              },
              {
                width: '25%',
                text: [{ text: 'Completadas: ', bold: true }, estadisticas.completadas.toString()],
              },
              {
                width: '25%',
                text: [
                  { text: 'Total Monto: ', bold: true },
                  formatCurrency(estadisticas.totalMonto),
                ],
              },
            ],
            margin: [0, 0, 0, 20],
          },

          // Tabla de órdenes
          {
            text: 'DETALLE DE ÓRDENES',
            style: 'sectionHeader',
            margin: [0, 10, 0, 5],
          },
          {
            table: {
              headerRows: 1,
              widths: ['auto', 'auto', '*', 'auto', 'auto', 'auto', 'auto', 'auto', 'auto', 'auto'],
              body: tableRows,
            },
            layout: {
              fillColor: function (rowIndex) {
                return rowIndex === 0 ? '#CCCCCC' : null
              },
              hLineWidth: function (i, node) {
                return i === 0 || i === 1 || i === node.table.body.length ? 1 : 0.5
              },
              vLineWidth: function (i) {
                return 0.5
              },
            },
          },
        ],
        styles: {
          header: {
            fontSize: 16,
            bold: true,
          },
          subheader: {
            fontSize: 12,
            bold: true,
          },
          title: {
            fontSize: 14,
            bold: true,
          },
          sectionHeader: {
            fontSize: 12,
            bold: true,
            color: '#333333',
            fillColor: '#f0f0f0',
            margin: [0, 5, 0, 5],
          },
          tableHeader: {
            bold: true,
            fontSize: 9,
            color: 'black',
            alignment: 'center',
          },
          normal: {
            fontSize: 10,
          },
        },
        defaultStyle: {
          fontSize: 8,
        },
      }

      const fechaReporte = new Date().toISOString().split('T')[0]
      pdfMake
        .createPdf(docDefinition)
        .download(`ordenes_despacho_${fechaInicial}_${fechaFinal}_${fechaReporte}.pdf`)
      toastRef.current?.addMessage('Reporte descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error downloading report:', error)
      toastRef.current?.addMessage('Error al descargar el reporte', 'error')
    }
  }

  // Función para descargar tabla en Excel
  const descargarTablaExcel = () => {
    if (!filteredOrdenes.length) {
      toastRef.current?.addMessage('No hay órdenes para descargar', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'

      // Crear nuevo libro de trabajo
      const workbook = XLSX.utils.book_new()

      // Hoja 1: Resumen General
      const resumenData = [
        ['REPORTE DE ÓRDENES DE DESPACHO'],
        [''],
        ['Estación:', estacionNombre],
        ['NIT:', estacionNit],
        ['Período:', `${fechaInicial} - ${fechaFinal}`],
        ['Generado:', new Date().toLocaleDateString('es-ES')],
        [''],
        ['RESUMEN ESTADÍSTICAS'],
        ['Total Órdenes:', estadisticas.total],
        ['Pendientes:', estadisticas.pendientes],
        ['Facturadas:', estadisticas.anuladas],
        ['Completadas:', estadisticas.completadas],
        ['Total Monto:', estadisticas.totalMonto],
      ]

      const resumenSheet = XLSX.utils.aoa_to_sheet(resumenData)

      // Aplicar estilos al resumen
      resumenSheet['!cols'] = [{ width: 20 }, { width: 30 }]

      XLSX.utils.book_append_sheet(workbook, resumenSheet, 'Resumen General')

      // Hoja 2: Detalle de Órdenes
      const ordenesData = [
        [
          'ID Transacción',
          'Fecha',
          'Cliente',
          'Identificación',
          'Forma de Pago',
          'Combustible',
          'Placa',
          'Cantidad',
          'Consecutivo Factura',
          'CUFE',
          'Estado Factura',
          'Estado',
          'Total',
          'Vendedor',
        ],
        ...filteredOrdenes.map((orden) => {
          const facturaInfo = formatIdFacturaElectronica(orden.idFacturaElectronica)
          const isValidFactura = typeof facturaInfo === 'object'

          return [
            orden.numeroTransaccion || orden.idVentaLocal || 'N/A',
            orden.fecha ? new Date(orden.fecha).toLocaleDateString('es-ES') : 'N/A',
            orden.nombreTercero || 'N/A',
            orden.identificacion || 'N/A',
            getFormaPago(orden),
            orden.combustible || 'N/A',
            orden.placa || 'N/A',
            orden.cantidad || 0,
            isValidFactura ? facturaInfo.consecutivo || 'N/A' : 'N/A',
            isValidFactura ? facturaInfo.cufe || 'N/A' : 'N/A',
            isValidFactura ? facturaInfo.estado || 'N/A' : 'N/A',
            getEstadoDisplay(orden.estado),
            orden.total || 0,
            orden.vendedor || 'N/A',
          ]
        }),
      ]

      const ordenesSheet = XLSX.utils.aoa_to_sheet(ordenesData)

      // Configurar anchos de columna
      ordenesSheet['!cols'] = [
        { width: 15 }, // ID Transacción
        { width: 12 }, // Fecha
        { width: 25 }, // Cliente
        { width: 15 }, // Identificación
        { width: 18 }, // Forma de Pago
        { width: 15 }, // Combustible
        { width: 12 }, // Placa
        { width: 10 }, // Cantidad
        { width: 18 }, // Consecutivo Factura
        { width: 35 }, // CUFE
        { width: 15 }, // Estado Factura
        { width: 12 }, // Estado
        { width: 15 }, // Total
        { width: 20 }, // Vendedor
      ]

      XLSX.utils.book_append_sheet(workbook, ordenesSheet, 'Detalle Órdenes')

      // Hoja 3: Por Estado
      const ordenesGroupData = [['ÓRDENES POR ESTADO'], [''], ['Estado', 'Cantidad', 'Monto Total']]

      // Agrupar por estado
      const estadosGroup = filteredOrdenes.reduce((acc, orden) => {
        const estado = getEstadoDisplay(orden.estado) || 'Sin Estado'
        if (!acc[estado]) {
          acc[estado] = { cantidad: 0, monto: 0 }
        }
        acc[estado].cantidad += 1
        acc[estado].monto += orden.total || 0
        return acc
      }, {})

      Object.entries(estadosGroup).forEach(([estado, data]) => {
        ordenesGroupData.push([estado, data.cantidad, data.monto])
      })

      const estadosSheet = XLSX.utils.aoa_to_sheet(ordenesGroupData)
      estadosSheet['!cols'] = [{ width: 20 }, { width: 15 }, { width: 20 }]

      XLSX.utils.book_append_sheet(workbook, estadosSheet, 'Por Estado')

      // Hoja 4: Por Combustible
      const combustibleGroupData = [
        ['ÓRDENES POR COMBUSTIBLE'],
        [''],
        ['Combustible', 'Cantidad', 'Litros Total', 'Monto Total'],
      ]

      const combustiblesGroup = filteredOrdenes.reduce((acc, orden) => {
        const combustible = orden.combustible || 'Sin Especificar'
        if (!acc[combustible]) {
          acc[combustible] = { cantidad: 0, litros: 0, monto: 0 }
        }
        acc[combustible].cantidad += 1
        acc[combustible].litros += orden.cantidad || 0
        acc[combustible].monto += orden.total || 0
        return acc
      }, {})

      Object.entries(combustiblesGroup).forEach(([combustible, data]) => {
        combustibleGroupData.push([combustible, data.cantidad, data.litros, data.monto])
      })

      const combustibleSheet = XLSX.utils.aoa_to_sheet(combustibleGroupData)
      combustibleSheet['!cols'] = [{ width: 20 }, { width: 15 }, { width: 15 }, { width: 20 }]

      XLSX.utils.book_append_sheet(workbook, combustibleSheet, 'Por Combustible')

      // Descargar archivo
      const fechaReporte = new Date().toISOString().split('T')[0]
      const fileName = `ordenes_despacho_${fechaInicial}_${fechaFinal}_${fechaReporte}.xlsx`

      XLSX.writeFile(workbook, fileName)
      toastRef.current?.addMessage('Reporte Excel descargado exitosamente', 'success')
    } catch (error) {
      console.error('Error downloading Excel report:', error)
      toastRef.current?.addMessage('Error al descargar el reporte Excel', 'error')
    }
  }

  // Calcular estadísticas
  const estadisticas = {
    total: filteredOrdenes.length,
    pendientes: filteredOrdenes.filter((o) => o.estado?.toLowerCase() === 'pendiente').length,
    anuladas: filteredOrdenes.filter((o) => o.estado?.toLowerCase() === 'anulada').length,
    completadas: filteredOrdenes.filter(
      (o) =>
        o.estado?.toLowerCase() === 'completado' ||
        o.estado?.toLowerCase() === 'facturado' ||
        (!o.estado?.toLowerCase().includes('anulad') &&
          !o.estado?.toLowerCase().includes('pendiente')),
    ).length,
    totalMonto: filteredOrdenes.reduce((sum, o) => sum + (o.total || 0), 0),
  }

  // Función para reenviar a facturación usando el servicio
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
          <CRow>
            <CCol md={3}>
              <div className="mb-3">
                <CFormLabel htmlFor="fechaInicial">
                  <CIcon icon={cilCalendar} className="me-1" />
                  Fecha Inicial
                </CFormLabel>
                <CFormInput
                  type="date"
                  id="fechaInicial"
                  value={fechaInicial}
                  onChange={(e) => setFechaInicial(e.target.value)}
                />
              </div>
            </CCol>
            <CCol md={3}>
              <div className="mb-3">
                <CFormLabel htmlFor="fechaFinal">
                  <CIcon icon={cilCalendar} className="me-1" />
                  Fecha Final
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
                <CFormLabel htmlFor="search">
                  <CIcon icon={cilSearch} className="me-1" />
                  Buscar
                </CFormLabel>
                <CInputGroup>
                  <CInputGroupText>
                    <CIcon icon={cilSearch} />
                  </CInputGroupText>
                  <CFormInput
                    placeholder="Buscar por ID, cliente, placa, combustible..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                </CInputGroup>
              </div>
            </CCol>
            <CCol md={2}>
              <div className="mb-3">
                <CFormLabel>&nbsp;</CFormLabel>
                <div>
                  <CButton
                    color="primary"
                    onClick={() => {
                      handleSearch()
                      setSinFacturar(false) // Desmarcar al buscar
                    }}
                    disabled={loading}
                    className="w-100 mb-2"
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
                  <div>
                    <CFormLabel className="small">
                      <input
                        type="checkbox"
                        checked={sinFacturar}
                        onChange={() => setSinFacturar(!sinFacturar)}
                        style={{ marginRight: '6px' }}
                        disabled={!showResults || ordenes.length === 0}
                      />
                      Órdenes sin facturar
                    </CFormLabel>
                  </div>
                </div>
              </div>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      {showResults && (
        <>
          {/* Botón para reenviar a facturación arriba de la tabla */}
          {sinFacturar && filteredOrdenes.length > 0 && (
            <div className="mb-3 text-end">
              <CButton color="warning" onClick={() => reenviarAFacturacion(filteredOrdenes)}>
                <CIcon icon={cilCloudDownload} className="me-2" />
                Reenviar a facturación ({filteredOrdenes.length})
              </CButton>
            </div>
          )}

          {/* Tarjetas de resumen */}
          <CRow className="mb-4">
            <CCol md={2}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-primary">{estadisticas.total}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Órdenes
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={2}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-warning">{estadisticas.pendientes}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Pendientes
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={2}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-danger">{estadisticas.anuladas}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Facturadas
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={2}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-success">{estadisticas.completadas}</div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Completadas
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
            <CCol md={4}>
              <CCard className="text-center">
                <CCardBody>
                  <div className="fs-4 fw-semibold text-success">
                    {formatCurrency(estadisticas.totalMonto)}
                  </div>
                  <div className="text-medium-emphasis text-uppercase fw-semibold small">
                    Total Monto
                  </div>
                </CCardBody>
              </CCard>
            </CCol>
          </CRow>

          <CCard>
            <CCardHeader className="d-flex justify-content-between align-items-center">
              <h5 className="mb-0">Resultados ({filteredOrdenes.length} órdenes)</h5>
              <CButtonGroup>
                <CButton
                  color="danger"
                  variant="outline"
                  onClick={descargarTablaPDF}
                  disabled={!filteredOrdenes.length}
                >
                  <CIcon icon={cilCloudDownload} className="me-2" />
                  PDF
                </CButton>
                <CButton
                  color="success"
                  variant="outline"
                  onClick={descargarTablaExcel}
                  disabled={!filteredOrdenes.length}
                >
                  <CIcon icon={cilSpreadsheet} className="me-2" />
                  Excel
                </CButton>
              </CButtonGroup>
            </CCardHeader>
            <CCardBody>
              {!filteredOrdenes.length ? (
                <CAlert color="warning">
                  No se encontraron órdenes para los criterios especificados.
                </CAlert>
              ) : (
                <>
                  <CTable striped hover responsive>
                    <CTableHead>
                      <CTableRow>
                        <CTableHeaderCell>ID Transacción</CTableHeaderCell>
                        <CTableHeaderCell>Fecha</CTableHeaderCell>
                        <CTableHeaderCell>Cliente</CTableHeaderCell>
                        <CTableHeaderCell>Combustible</CTableHeaderCell>
                        <CTableHeaderCell>Placa</CTableHeaderCell>
                        <CTableHeaderCell>Cantidad</CTableHeaderCell>
                        <CTableHeaderCell>ID Factura</CTableHeaderCell>
                        <CTableHeaderCell>Estado</CTableHeaderCell>
                        <CTableHeaderCell>Total</CTableHeaderCell>
                        <CTableHeaderCell>Acciones</CTableHeaderCell>
                      </CTableRow>
                    </CTableHead>
                    <CTableBody>
                      {currentItems.map((orden) => (
                        <CTableRow key={orden.guid || orden.idVentaLocal}>
                          <CTableDataCell>
                            <strong>
                              {orden.numeroTransaccion || orden.idVentaLocal || 'N/A'}
                            </strong>
                          </CTableDataCell>
                          <CTableDataCell>
                            <small>{formatDate(orden.fecha)}</small>
                          </CTableDataCell>
                          <CTableDataCell>
                            <div>
                              <strong>{orden.nombreTercero || 'N/A'}</strong>
                              {orden.identificacion && (
                                <div className="small text-muted">{orden.identificacion}</div>
                              )}
                            </div>
                          </CTableDataCell>
                          <CTableDataCell>
                            <CBadge color="info">{orden.combustible || 'N/A'}</CBadge>
                          </CTableDataCell>
                          <CTableDataCell>
                            <CBadge color="secondary">{orden.placa || 'N/A'}</CBadge>
                          </CTableDataCell>
                          <CTableDataCell className="text-center">
                            <CBadge color="primary">{orden.cantidad || '0'}</CBadge>
                          </CTableDataCell>
                          <CTableDataCell>
                            {(() => {
                              const facturaInfo = formatIdFacturaElectronica(
                                orden.idFacturaElectronica,
                              )
                              const isValid =
                                facturaInfo !== 'N/A' &&
                                facturaInfo !== 'Formato inválido' &&
                                facturaInfo !== 'Error al procesar'

                              if (!isValid) {
                                return <CBadge color="secondary">Sin Factura</CBadge>
                              }

                              const badgeColor = facturaInfo.estado === 'Ok' ? 'success' : 'warning'

                              return (
                                <div>
                                  <CBadge color={badgeColor} className="mb-1">
                                    {facturaInfo.consecutivo}
                                  </CBadge>
                                  {facturaInfo.cufe && (
                                    <div className="small text-muted" title={facturaInfo.cufe}>
                                      CUFE: {facturaInfo.cufe.substring(0, 8)}...
                                    </div>
                                  )}
                                </div>
                              )
                            })()}
                          </CTableDataCell>
                          <CTableDataCell>
                            <CBadge color={getEstadoBadgeColor(orden.estado)}>
                              {getEstadoDisplay(orden.estado)}
                            </CBadge>
                          </CTableDataCell>
                          <CTableDataCell className="fw-bold text-success">
                            {formatCurrency(orden.total)}
                          </CTableDataCell>
                          <CTableDataCell>
                            <CButton color="info" size="sm" onClick={() => goToOrdenDetalle(orden)}>
                              <CIcon icon={cilCalculator} size="sm" className="me-1" />
                              Ver
                            </CButton>
                          </CTableDataCell>
                        </CTableRow>
                      ))}
                    </CTableBody>
                  </CTable>

                  {/* Paginación */}
                  {totalPages > 1 && (
                    <div className="d-flex justify-content-center mt-3">
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
                </>
              )}
            </CCardBody>
          </CCard>
        </>
      )}

      <Toast ref={toastRef} />
    </>
  )
}

export default OrdenesDespacho
