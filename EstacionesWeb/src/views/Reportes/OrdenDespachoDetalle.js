import { React, useState, useRef, useEffect } from 'react'
import { useNavigate, useParams, useLocation } from 'react-router-dom'
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
  CSpinner,
  CBadge,
  CAlert,
  CCollapse,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import {
  cilArrowLeft,
  cilPrint,
  cilTruck,
  cilCalculator,
  cilUser,
  cilLocationPin,
  cilPhone,
  cilCalendar,
  cilTask,
} from '@coreui/icons'
import Toast from '../toast/Toast'
import FacturaElectronicaService from '../../services/FacturaElectronicaService'
import QRCodeDisplay from '../../components/QRCodeDisplay'

var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

const OrdenDespachoDetalle = () => {
  const navigate = useNavigate()
  const { ordenId } = useParams()
  const location = useLocation()
  const toastRef = useRef()

  // Estados
  const [orden, setOrden] = useState(null)
  const [loading, setLoading] = useState(true)
  const [facturaElectronica, setFacturaElectronica] = useState(null)
  const [loadingFacturaElectronica, setLoadingFacturaElectronica] = useState(false)
  const [mostrarFacturaElectronica, setMostrarFacturaElectronica] = useState(false)

  // Cargar datos de la orden al montar el componente
  useEffect(() => {
    loadOrdenDetalle()
  }, [ordenId])

  // Función para cargar el detalle de la orden
  const loadOrdenDetalle = async () => {
    setLoading(true)
    try {
      // Try to get order from navigation state first
      if (location.state?.orden) {
        setOrden(location.state.orden)

        // Si hay ID de factura electrónica, cargar la información
        if (location.state.orden.idFacturaElectronica && location.state.orden.idVentaLocal) {
          await loadFacturaElectronica(location.state.orden.idVentaLocal)
        }
      } else {
        // If no order in state, show error
        toastRef.current?.addMessage('No se encontraron datos de la orden', 'error')
        navigate('/OrdenesDespacho')
        return
      }
    } catch (error) {
      console.error('Error:', error)
      toastRef.current?.addMessage('Error al cargar la orden', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Función para cargar información de factura electrónica
  const loadFacturaElectronica = async (idVentaLocal) => {
    if (!idVentaLocal) return

    setLoadingFacturaElectronica(true)
    try {
      const data = await FacturaElectronicaService.obtenerInformacionFacturaElectronica(
        idVentaLocal,
      )
      const parsedData = FacturaElectronicaService.parseFacturaElectronicaData(data)

      if (parsedData) {
        setFacturaElectronica(parsedData)
      } else {
        toastRef.current?.addMessage(
          'Error al procesar información de factura electrónica',
          'warning',
        )
      }
    } catch (error) {
      console.error('Error al cargar factura electrónica:', error)
      toastRef.current?.addMessage('Error al cargar información de factura electrónica', 'error')
    } finally {
      setLoadingFacturaElectronica(false)
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

  // Función para obtener el color del badge según el estado
  const getEstadoBadgeColor = (estado) => {
    switch (estado?.toLowerCase()) {
      case 'pendiente':
        return 'warning'
      case 'en_proceso':
      case 'en proceso':
        return 'info'
      case 'despachado':
      case 'completado':
        return 'success'
      case 'cancelado':
        return 'danger'
      default:
        return 'secondary'
    }
  }

  // Función para imprimir orden
  const imprimirOrden = () => {
    if (!orden) {
      toastRef.current?.addMessage('No hay datos para imprimir', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'
      const estacionDireccion = localStorage.getItem('estacionDireccion') || ''
      const estacionTelefono = localStorage.getItem('estacionTelefono') || ''

      const docDefinition = {
        pageSize: 'A4',
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
                  ...(estacionDireccion
                    ? [{ text: `Dirección: ${estacionDireccion}`, style: 'normal' }]
                    : []),
                  ...(estacionTelefono
                    ? [{ text: `Teléfono: ${estacionTelefono}`, style: 'normal' }]
                    : []),
                ],
              },
              {
                width: 'auto',
                stack: [
                  { text: 'ORDEN DE DESPACHO', style: 'title', alignment: 'right' },
                  {
                    text: `No. ${orden.numeroTransaccion || orden.idVentaLocal}`,
                    style: 'orderNumber',
                    alignment: 'right',
                  },
                  { text: formatDate(orden.fecha), style: 'normal', alignment: 'right' },
                ],
              },
            ],
            margin: [0, 0, 0, 20],
          },

          // Información del cliente
          {
            text: 'INFORMACIÓN DEL CLIENTE',
            style: 'sectionHeader',
            margin: [0, 10, 0, 5],
          },
          {
            columns: [
              {
                width: '50%',
                stack: [
                  { text: [{ text: 'Cliente: ', bold: true }, orden.nombreTercero || 'N/A'] },
                  { text: [{ text: 'Documento: ', bold: true }, orden.identificacion || 'N/A'] },
                  { text: [{ text: 'Forma de Pago: ', bold: true }, orden.formaDePago || 'N/A'] },
                ],
              },
              {
                width: '50%',
                stack: [
                  { text: [{ text: 'Placa: ', bold: true }, orden.placa || 'N/A'] },
                  { text: [{ text: 'Combustible: ', bold: true }, orden.combustible || 'N/A'] },
                  { text: [{ text: 'Estado: ', bold: true }, orden.estado || 'N/A'] },
                ],
              },
            ],
            margin: [0, 0, 0, 20],
          },

          // Detalle de productos
          {
            text: 'DETALLE DE LA VENTA',
            style: 'sectionHeader',
            margin: [0, 10, 0, 5],
          },
          {
            table: {
              headerRows: 1,
              widths: ['*', 'auto', 'auto', 'auto', 'auto'],
              body: [
                [
                  { text: 'Combustible', style: 'tableHeader' },
                  { text: 'Cantidad', style: 'tableHeader', alignment: 'center' },
                  { text: 'Precio Unit.', style: 'tableHeader', alignment: 'right' },
                  { text: 'Descuento', style: 'tableHeader', alignment: 'right' },
                  { text: 'Total', style: 'tableHeader', alignment: 'right' },
                ],
                [
                  orden.combustible || 'N/A',
                  { text: orden.cantidad || '0', alignment: 'center' },
                  { text: formatCurrency(orden.precio), alignment: 'right' },
                  { text: formatCurrency(orden.descuento || 0), alignment: 'right' },
                  { text: formatCurrency(orden.total), alignment: 'right', bold: true },
                ],
              ],
            },
            layout: {
              fillColor: function (rowIndex) {
                return rowIndex === 0 ? '#CCCCCC' : null
              },
            },
            margin: [0, 0, 0, 20],
          },

          // Totales
          {
            columns: [
              { width: '*', text: '' },
              {
                width: 200,
                table: {
                  widths: ['*', 'auto'],
                  body: [
                    [
                      { text: 'Subtotal:', bold: true },
                      {
                        text: formatCurrency(orden.subTotal + orden.descuento || 0),
                        alignment: 'right',
                      },
                    ],
                    [
                      { text: 'Descuentos:', bold: true },
                      { text: formatCurrency(orden.descuento || 0), alignment: 'right' },
                    ],
                    [
                      { text: 'TOTAL:', bold: true, fontSize: 12 },
                      {
                        text: formatCurrency(orden.total || 0),
                        alignment: 'right',
                        bold: true,
                        fontSize: 12,
                      },
                    ],
                  ],
                },
                layout: 'noBorders',
              },
            ],
          },

          // Información adicional de despacho
          {
            text: 'INFORMACIÓN DE DESPACHO',
            style: 'sectionHeader',
            margin: [0, 20, 0, 5],
          },
          {
            columns: [
              {
                width: '50%',
                stack: [
                  { text: [{ text: 'Surtidor: ', bold: true }, orden.surtidor || 'N/A'] },
                  { text: [{ text: 'Cara: ', bold: true }, orden.cara || 'N/A'] },
                  { text: [{ text: 'Manguera: ', bold: true }, orden.manguera || 'N/A'] },
                ],
              },
              {
                width: '50%',
                stack: [
                  { text: [{ text: 'Vendedor: ', bold: true }, orden.vendedor || 'N/A'] },
                  { text: [{ text: 'Kilometraje: ', bold: true }, orden.kilometraje || 'N/A'] },
                  { text: [{ text: 'ID Venta: ', bold: true }, orden.idVentaLocal || 'N/A'] },
                ],
              },
            ],
            margin: [0, 0, 0, 20],
          },

          // Información de factura electrónica (si está disponible)
          ...(facturaElectronica
            ? [
                {
                  text: 'INFORMACIÓN DE FACTURA ELECTRÓNICA',
                  style: 'sectionHeader',
                  margin: [0, 20, 0, 5],
                },
                {
                  columns: [
                    {
                      width: '50%',
                      stack: [
                        {
                          text: [
                            { text: 'Consecutivo: ', bold: true },
                            facturaElectronica.consecutivo || 'N/A',
                          ],
                        },
                        {
                          text: [
                            { text: 'Fecha: ', bold: true },
                            facturaElectronica.fecha || 'N/A',
                          ],
                        },
                      ],
                    },
                    {
                      width: '50%',
                      stack: [
                        {
                          text: [{ text: 'CUFE: ', bold: true }, facturaElectronica.cufe || 'N/A'],
                        },
                        {
                          text: [
                            { text: 'Consulta DIAN: ', bold: true },
                            'https://catalogo-vpfe.dian.gov.co/',
                          ],
                        },
                      ],
                    },
                  ],
                  margin: [0, 0, 0, 20],
                },
              ]
            : []),

          // Firmas
          {
            columns: [
              {
                width: '45%',
                stack: [
                  { text: '\n\n\n', fontSize: 1 },
                  { text: '_________________________', alignment: 'center' },
                  { text: 'Firma Autorizada', alignment: 'center', fontSize: 10 },
                  { text: estacionNombre, alignment: 'center', fontSize: 10 },
                ],
              },
              { width: '10%', text: '' },
              {
                width: '45%',
                stack: [
                  { text: '\n\n\n', fontSize: 1 },
                  { text: '_________________________', alignment: 'center' },
                  { text: 'Recibido por', alignment: 'center', fontSize: 10 },
                  { text: orden.nombreTercero || '', alignment: 'center', fontSize: 10 },
                ],
              },
            ],
            margin: [0, 30, 0, 0],
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
          orderNumber: {
            fontSize: 14,
            bold: true,
            color: '#0066cc',
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
            fontSize: 10,
            color: 'black',
          },
          normal: {
            fontSize: 10,
          },
        },
      }

      pdfMake.createPdf(docDefinition).print()
      toastRef.current?.addMessage('Orden enviada a impresión', 'success')
    } catch (error) {
      console.error('Error printing:', error)
      toastRef.current?.addMessage('Error al imprimir la orden', 'error')
    }
  }

  // Función para descargar PDF
  const descargarPDF = () => {
    if (!orden) {
      toastRef.current?.addMessage('No hay datos para descargar', 'error')
      return
    }

    try {
      const estacionNombre = localStorage.getItem('estacionNombre') || 'Estación'
      const estacionNit = localStorage.getItem('estacionNit') || 'N/A'
      const estacionDireccion = localStorage.getItem('estacionDireccion') || ''
      const estacionTelefono = localStorage.getItem('estacionTelefono') || ''

      const docDefinition = {
        pageSize: 'A4',
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
                  ...(estacionDireccion
                    ? [{ text: `Dirección: ${estacionDireccion}`, style: 'normal' }]
                    : []),
                  ...(estacionTelefono
                    ? [{ text: `Teléfono: ${estacionTelefono}`, style: 'normal' }]
                    : []),
                ],
              },
              {
                width: 'auto',
                stack: [
                  { text: 'ORDEN DE DESPACHO', style: 'title', alignment: 'right' },
                  {
                    text: `No. ${orden.numeroTransaccion || orden.idVentaLocal}`,
                    style: 'orderNumber',
                    alignment: 'right',
                  },
                  { text: formatDate(orden.fecha), style: 'normal', alignment: 'right' },
                ],
              },
            ],
            margin: [0, 0, 0, 20],
          },

          // Información del cliente
          {
            text: 'INFORMACIÓN DEL CLIENTE',
            style: 'sectionHeader',
            margin: [0, 10, 0, 5],
          },
          {
            columns: [
              {
                width: '50%',
                stack: [
                  { text: [{ text: 'Cliente: ', bold: true }, orden.nombreTercero || 'N/A'] },
                  { text: [{ text: 'Documento: ', bold: true }, orden.identificacion || 'N/A'] },
                  { text: [{ text: 'Forma de Pago: ', bold: true }, orden.formaDePago || 'N/A'] },
                ],
              },
              {
                width: '50%',
                stack: [
                  { text: [{ text: 'Placa: ', bold: true }, orden.placa || 'N/A'] },
                  { text: [{ text: 'Combustible: ', bold: true }, orden.combustible || 'N/A'] },
                  { text: [{ text: 'Estado: ', bold: true }, orden.estado || 'N/A'] },
                ],
              },
            ],
            margin: [0, 0, 0, 20],
          },

          // Detalle de productos
          {
            text: 'DETALLE DE LA VENTA',
            style: 'sectionHeader',
            margin: [0, 10, 0, 5],
          },
          {
            table: {
              headerRows: 1,
              widths: ['*', 'auto', 'auto', 'auto', 'auto'],
              body: [
                [
                  { text: 'Combustible', style: 'tableHeader' },
                  { text: 'Cantidad', style: 'tableHeader', alignment: 'center' },
                  { text: 'Precio Unit.', style: 'tableHeader', alignment: 'right' },
                  { text: 'Descuento', style: 'tableHeader', alignment: 'right' },
                  { text: 'Total', style: 'tableHeader', alignment: 'right' },
                ],
                [
                  orden.combustible || 'N/A',
                  { text: orden.cantidad || '0', alignment: 'center' },
                  { text: formatCurrency(orden.precio), alignment: 'right' },
                  { text: formatCurrency(orden.descuento || 0), alignment: 'right' },
                  { text: formatCurrency(orden.total), alignment: 'right', bold: true },
                ],
              ],
            },
            layout: {
              fillColor: function (rowIndex) {
                return rowIndex === 0 ? '#CCCCCC' : null
              },
            },
            margin: [0, 0, 0, 20],
          },

          // Totales
          {
            columns: [
              { width: '*', text: '' },
              {
                width: 200,
                table: {
                  widths: ['*', 'auto'],
                  body: [
                    [
                      { text: 'Subtotal:', bold: true },
                      {
                        text: formatCurrency(orden.subTotal + orden.descuento || 0),
                        alignment: 'right',
                      },
                    ],
                    [
                      { text: 'Descuentos:', bold: true },
                      { text: formatCurrency(orden.descuento || 0), alignment: 'right' },
                    ],
                    [
                      { text: 'TOTAL:', bold: true, fontSize: 12 },
                      {
                        text: formatCurrency(orden.total || 0),
                        alignment: 'right',
                        bold: true,
                        fontSize: 12,
                      },
                    ],
                  ],
                },
                layout: 'noBorders',
              },
            ],
          },

          // Información adicional de despacho
          {
            text: 'INFORMACIÓN DE DESPACHO',
            style: 'sectionHeader',
            margin: [0, 20, 0, 5],
          },
          {
            columns: [
              {
                width: '50%',
                stack: [
                  { text: [{ text: 'Surtidor: ', bold: true }, orden.surtidor || 'N/A'] },
                  { text: [{ text: 'Cara: ', bold: true }, orden.cara || 'N/A'] },
                  { text: [{ text: 'Manguera: ', bold: true }, orden.manguera || 'N/A'] },
                ],
              },
              {
                width: '50%',
                stack: [
                  { text: [{ text: 'Vendedor: ', bold: true }, orden.vendedor || 'N/A'] },
                  { text: [{ text: 'Kilometraje: ', bold: true }, orden.kilometraje || 'N/A'] },
                  { text: [{ text: 'ID Venta: ', bold: true }, orden.idVentaLocal || 'N/A'] },
                ],
              },
            ],
            margin: [0, 0, 0, 20],
          },

          // Información de factura electrónica (si está disponible)
          ...(facturaElectronica
            ? [
                {
                  text: 'INFORMACIÓN DE FACTURA ELECTRÓNICA',
                  style: 'sectionHeader',
                  margin: [0, 20, 0, 5],
                },
                {
                  columns: [
                    {
                      width: '50%',
                      stack: [
                        {
                          text: [
                            { text: 'Consecutivo: ', bold: true },
                            facturaElectronica.consecutivo || 'N/A',
                          ],
                        },
                        {
                          text: [
                            { text: 'Fecha: ', bold: true },
                            facturaElectronica.fecha || 'N/A',
                          ],
                        },
                      ],
                    },
                    {
                      width: '50%',
                      stack: [
                        {
                          text: [{ text: 'CUFE: ', bold: true }, facturaElectronica.cufe || 'N/A'],
                        },
                        {
                          text: [
                            { text: 'Consulta DIAN: ', bold: true },
                            'https://catalogo-vpfe.dian.gov.co/',
                          ],
                        },
                      ],
                    },
                  ],
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
          orderNumber: {
            fontSize: 14,
            bold: true,
            color: '#0066cc',
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
            fontSize: 10,
            color: 'black',
          },
          normal: {
            fontSize: 10,
          },
        },
      }

      pdfMake
        .createPdf(docDefinition)
        .download(
          `orden_despacho_${orden.numeroTransaccion || orden.idVentaLocal}_${
            new Date().toISOString().split('T')[0]
          }.pdf`,
        )
      toastRef.current?.addMessage('Orden descargada exitosamente', 'success')
    } catch (error) {
      console.error('Error downloading:', error)
      toastRef.current?.addMessage('Error al descargar la orden', 'error')
    }
  }

  if (loading) {
    return (
      <div className="text-center p-5">
        <CSpinner color="primary" size="lg" />
        <div className="mt-3">Cargando orden de despacho...</div>
      </div>
    )
  }

  if (!orden) {
    return <CAlert color="danger">No se encontró la orden de despacho solicitada.</CAlert>
  }

  return (
    <>
      <CRow className="mb-3">
        <CCol>
          <CButton color="secondary" variant="outline" onClick={() => navigate('/OrdenesDespacho')}>
            <CIcon icon={cilArrowLeft} className="me-2" />
            Volver a Órdenes
          </CButton>
        </CCol>
      </CRow>

      <CCard className="mb-4">
        <CCardHeader className="d-flex justify-content-between align-items-center">
          <h4 className="mb-0">
            <CIcon icon={cilTruck} className="me-2" />
            Orden de Despacho #{orden.numeroTransaccion || orden.idVentaLocal || 'N/A'}
          </h4>
          <div className="d-flex gap-2">
            <CButton color="success" onClick={imprimirOrden}>
              <CIcon icon={cilPrint} className="me-2" />
              Imprimir
            </CButton>
          </div>
        </CCardHeader>
        <CCardBody>
          {/* Información general */}
          <CRow className="mb-4">
            <CCol md={6}>
              <h6>
                <CIcon icon={cilUser} className="me-2" />
                Información del Cliente
              </h6>
              <table className="table table-sm">
                <tbody>
                  <tr>
                    <td>
                      <strong>Cliente:</strong>
                    </td>
                    <td>{orden.nombreTercero || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Documento:</strong>
                    </td>
                    <td>{orden.identificacion || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Forma de Pago:</strong>
                    </td>
                    <td>{orden.formaDePago || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Vendedor:</strong>
                    </td>
                    <td>{orden.vendedor || 'N/A'}</td>
                  </tr>
                </tbody>
              </table>
            </CCol>
            <CCol md={6}>
              <h6>
                <CIcon icon={cilTruck} className="me-2" />
                Información de la Venta
              </h6>
              <table className="table table-sm">
                <tbody>
                  <tr>
                    <td>
                      <strong>Placa:</strong>
                    </td>
                    <td>
                      <CBadge color="secondary">{orden.placa || 'N/A'}</CBadge>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Combustible:</strong>
                    </td>
                    <td>{orden.combustible || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Fecha:</strong>
                    </td>
                    <td>{formatDate(orden.fecha)}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Estado:</strong>
                    </td>
                    <td>
                      <CBadge color={getEstadoBadgeColor(orden.estado)}>
                        {orden.estado || 'N/A'}
                      </CBadge>
                    </td>
                  </tr>
                </tbody>
              </table>
            </CCol>
          </CRow>

          {/* Información de la transacción */}
          <CRow className="mb-4">
            <CCol md={6}>
              <h6>
                <CIcon icon={cilLocationPin} className="me-2" />
                Información de Despacho
              </h6>
              <table className="table table-sm">
                <tbody>
                  <tr>
                    <td>
                      <strong>Surtidor:</strong>
                    </td>
                    <td>{orden.surtidor || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Cara:</strong>
                    </td>
                    <td>{orden.cara || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Manguera:</strong>
                    </td>
                    <td>{orden.manguera || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Kilometraje:</strong>
                    </td>
                    <td>{orden.kilometraje || 'N/A'}</td>
                  </tr>
                </tbody>
              </table>
            </CCol>
            <CCol md={6}>
              <h6>
                <CIcon icon={cilCalculator} className="me-2" />
                Información de Facturación
              </h6>
              <table className="table table-sm">
                <tbody>
                  <tr>
                    <td>
                      <strong>ID Factura:</strong>
                    </td>
                    <td>{orden.idFactura || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>ID Venta Local:</strong>
                    </td>
                    <td>{orden.idVentaLocal || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>ID Factura Electrónica:</strong>
                    </td>
                    <td className="text-break small">{orden.idFacturaElectronica || 'N/A'}</td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Fecha Reporte:</strong>
                    </td>
                    <td>{formatDate(orden.fechaReporte)}</td>
                  </tr>
                </tbody>
              </table>
            </CCol>
          </CRow>

          {/* Información de Factura Electrónica */}
          {orden.idFacturaElectronica && (
            <CRow className="mb-4">
              <CCol>
                <CCard className="border-primary">
                  <CCardHeader className="bg-primary text-white">
                    <CButton
                      color="primary"
                      variant="ghost"
                      className="text-white p-0 border-0 d-flex align-items-center"
                      onClick={() => setMostrarFacturaElectronica(!mostrarFacturaElectronica)}
                    >
                      <span className="me-2">{mostrarFacturaElectronica ? '▲' : '▼'}</span>
                      <CIcon icon={cilTask} className="me-2" />
                      Información de Factura Electrónica
                    </CButton>
                  </CCardHeader>
                  <CCollapse visible={mostrarFacturaElectronica}>
                    <CCardBody>
                      {loadingFacturaElectronica ? (
                        <div className="text-center p-3">
                          <CSpinner size="sm" className="me-2" />
                          Cargando información de factura electrónica...
                        </div>
                      ) : facturaElectronica ? (
                        <CRow>
                          <CCol md={6}>
                            <table className="table table-sm">
                              <tbody>
                                <tr>
                                  <td>
                                    <strong>Consecutivo:</strong>
                                  </td>
                                  <td>{facturaElectronica.consecutivo}</td>
                                </tr>
                                <tr>
                                  <td>
                                    <strong>Fecha:</strong>
                                  </td>
                                  <td>{facturaElectronica.fecha}</td>
                                </tr>
                                <tr>
                                  <td>
                                    <strong>CUFE:</strong>
                                  </td>
                                  <td className="text-break small">{facturaElectronica.cufe}</td>
                                </tr>
                                <tr>
                                  <td>
                                    <strong>Consultar en DIAN:</strong>
                                  </td>
                                  <td>
                                    <a
                                      href={facturaElectronica.qrUrl}
                                      target="_blank"
                                      rel="noopener noreferrer"
                                      className="btn btn-sm btn-outline-primary"
                                    >
                                      Ver en DIAN
                                    </a>
                                  </td>
                                </tr>
                              </tbody>
                            </table>
                          </CCol>
                          <CCol md={6}>
                            <div className="text-center">
                              <p className="small text-muted mb-2">
                                Código QR para consulta en DIAN
                              </p>
                              <QRCodeDisplay value={facturaElectronica.qrUrl} size={150} />
                            </div>
                          </CCol>
                        </CRow>
                      ) : (
                        <CAlert color="warning" className="mb-0">
                          No se pudo cargar la información de la factura electrónica.
                        </CAlert>
                      )}
                    </CCardBody>
                  </CCollapse>
                </CCard>
              </CCol>
            </CRow>
          )}

          {/* Detalle de la venta */}
          <h6>
            <CIcon icon={cilCalculator} className="me-2" />
            Detalle de la Venta
          </h6>
          <CTable striped hover responsive>
            <CTableHead>
              <CTableRow>
                <CTableHeaderCell>Combustible</CTableHeaderCell>
                <CTableHeaderCell>Cantidad</CTableHeaderCell>
                <CTableHeaderCell>Precio Unit.</CTableHeaderCell>
                <CTableHeaderCell>Descuento</CTableHeaderCell>
                <CTableHeaderCell>Total</CTableHeaderCell>
              </CTableRow>
            </CTableHead>
            <CTableBody>
              <CTableRow>
                <CTableDataCell>{orden.combustible || 'N/A'}</CTableDataCell>
                <CTableDataCell className="text-center">
                  <CBadge color="primary">{orden.cantidad || 0}</CBadge>
                </CTableDataCell>
                <CTableDataCell className="text-end">{formatCurrency(orden.precio)}</CTableDataCell>
                <CTableDataCell className="text-end">
                  {formatCurrency(orden.descuento || 0)}
                </CTableDataCell>
                <CTableDataCell className="text-end fw-bold text-success">
                  {formatCurrency(orden.total)}
                </CTableDataCell>
              </CTableRow>
            </CTableBody>
          </CTable>

          {/* Totales */}
          <CRow className="mt-4">
            <CCol md={8}></CCol>
            <CCol md={4}>
              <table className="table table-sm">
                <tbody>
                  <tr>
                    <td>
                      <strong>Subtotal:</strong>
                    </td>
                    <td className="text-end">
                      {formatCurrency(orden.subTotal + orden.descuento || 0)}
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <strong>Descuentos:</strong>
                    </td>
                    <td className="text-end text-danger">
                      -{formatCurrency(orden.descuento || 0)}
                    </td>
                  </tr>
                  <tr className="table-active">
                    <td>
                      <strong>TOTAL:</strong>
                    </td>
                    <td className="text-end fw-bold text-success fs-5">
                      {formatCurrency(orden.total)}
                    </td>
                  </tr>
                </tbody>
              </table>
            </CCol>
          </CRow>
        </CCardBody>
      </CCard>

      <Toast ref={toastRef} />
    </>
  )
}

export default OrdenDespachoDetalle
